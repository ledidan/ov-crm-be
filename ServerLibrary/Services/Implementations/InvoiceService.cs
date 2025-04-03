using MongoDB.Driver;
using Data.MongoModels;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Data.DTOs;
using Data.Entities;
using Data.Responses;
using MongoDB.Bson;
using Data.Enums;
using ServerLibrary.Helpers;

namespace ServerLibrary.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _appContext;
        private readonly IMongoCollection<InvoiceDetails> _invoicesDetailsCollection;
        public InvoiceService(MongoDbContext dbContext, AppDbContext appContext, IMapper mapper)
        {
            _invoicesDetailsCollection = dbContext.InvoiceDetails;
            _appContext = appContext;
            _mapper = mapper;
        }

        public async Task<GeneralResponse?> CreateInvoiceAsync(InvoiceDTO invoiceDto, Employee employee, Partner partner)
        {
            Console.WriteLine($"Starting CreateInvoiceAsync for Employee ID: {employee?.Id}, Partner ID: {partner?.Id}");
            var codeGenerator = new GenerateNextCode(_appContext);
            var strategy = _appContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                Console.WriteLine("Execution strategy started.");
                using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Transaction started.");

                    if (invoiceDto == null || invoiceDto.InvoiceDetails.Count == 0)
                    {
                        Console.WriteLine("Invalid invoice or invoice details detected.");
                        return new GeneralResponse(false, "Hóa đơn hoặc chi tiết hóa đơn không hợp lệ.");
                    }
                    Console.WriteLine($"InvoiceDTO valid. InvoiceDetails count: {invoiceDto.InvoiceDetails.Count}");

                    Console.WriteLine("Mapping InvoiceDTO to Invoice entity...");
                    var invoice = _mapper.Map<Invoice>(invoiceDto);
                    if (string.IsNullOrEmpty(invoice.InvoiceRequestName))
                    {
                        invoice.InvoiceRequestName = await codeGenerator.GenerateNextCodeAsync<Invoice>("HĐ", c => c.InvoiceRequestName, c => c.Partner.Id == partner.Id);
                    }
                    invoice.OwnerId = employee.Id;
                    invoice.Partner = partner;
                    _appContext.Invoices.Add(invoice);
                    await _appContext.SaveChangesAsync();
                    Console.WriteLine($"Invoice added to SQL database. Invoice ID: {invoice.Id}");

                    if (invoiceDto.Orders != null && invoiceDto.Orders.Any())
                    {
                        Console.WriteLine($"Processing {invoiceDto.Orders.Count} Order IDs from InvoiceDTO...");
                        var invoiceOrders = invoiceDto.Orders.Select(orderId => new InvoiceOrders
                        {
                            InvoiceId = invoice.Id,
                            OrderId = orderId,
                            PartnerId = partner.Id
                        }).ToList();

                        Console.WriteLine($"Adding {invoiceOrders.Count} records to InvoiceOrders table...");
                        await _appContext.InvoiceOrders.AddRangeAsync(invoiceOrders);
                        await _appContext.SaveChangesAsync();
                        Console.WriteLine("InvoiceOrders added to database successfully.");
                    }
                    else
                    {
                        Console.WriteLine("No Orders provided in InvoiceDTO.");
                    }
                    Console.WriteLine("Creating InvoiceDetails for MongoDB...");
                    var invoiceDetails = invoiceDto.InvoiceDetails.Select(detailDto => new InvoiceDetails
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        InvoiceId = invoice.Id,
                        InvoiceRequestName = invoice.InvoiceRequestName,
                        PartnerId = partner.Id,
                        PartnerName = partner.Name,
                        CustomerId = detailDto.CustomerId,
                        CustomerName = detailDto.CustomerName,
                        OrderId = detailDto.OrderId,
                        SaleOrderNo = detailDto.SaleOrderNo,
                        ProductId = detailDto.ProductId,
                        ProductCode = detailDto.ProductCode,
                        ProductName = detailDto.ProductName,
                        TaxID = detailDto.TaxID,
                        TaxAmount = detailDto.TaxAmount,
                        TaxIDText = detailDto.TaxIDText,
                        DiscountRate = detailDto.DiscountRate,
                        DiscountAmount = detailDto.DiscountAmount,
                        UnitPrice = detailDto.UnitPrice,
                        Total = detailDto.Total,
                        QuantityInstock = detailDto.QuantityInstock,
                        UsageUnitID = detailDto.UsageUnitID,
                        UsageUnitIDText = detailDto.UsageUnitIDText,
                        Quantity = detailDto.Quantity,
                        AmountSummary = detailDto.AmountSummary,
                        CreatedAt = DateTime.Now,
                    }).ToList();
                    Console.WriteLine($"Prepared {invoiceDetails.Count} InvoiceDetails.");

                    Console.WriteLine("Inserting InvoiceDetails into MongoDB...");
                    await _invoicesDetailsCollection.InsertManyAsync(invoiceDetails);
                    Console.WriteLine("InvoiceDetails inserted into MongoDB successfully.");

                    Console.WriteLine("Committing transaction...");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Hóa đơn được tạo thành công. Mã hóa đơn: {invoice.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }

                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(false, $"Không thể tạo hóa đơn: {ex.Message}");
                }
            });
        }

        public async Task<GeneralResponse?> UpdateInvoiceAsync(int id, InvoiceDTO invoiceDTO, Employee employee, Partner partner)
        {
            Console.WriteLine($"Starting UpdateInvoiceAsync for Invoice ID: {id}, Employee ID: {employee?.Id}, Partner ID: {partner?.Id}");
            var codeGenerator = new GenerateNextCode(_appContext);
            var strategy = _appContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                Console.WriteLine("Execution strategy started.");
                using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Transaction started.");

                    Console.WriteLine("Fetching existing invoice from database...");
                    Invoice? existingInvoice = await _appContext.Invoices.Include(c => c.InvoiceEmployees).Include(c => c.InvoiceOrders)
                        .FirstOrDefaultAsync(o => o.Id == id
                            && (o.OwnerId == employee.Id || o.InvoiceEmployees
                                .Any(oe => oe.EmployeeId == employee.Id && oe.AccessLevel == AccessLevel.Write))
                            && o.Partner == partner);

                    if (existingInvoice == null)
                    {
                        Console.WriteLine($"Invoice with ID {id} not found.");
                        return new GeneralResponse(false, "Không tìm thấy hóa đơn.");
                    }
                    Console.WriteLine($"Invoice found: ID {existingInvoice.Id}, OwnerId {existingInvoice.OwnerId}");

                    Console.WriteLine("Mapping invoiceDTO to existingInvoice...");
                    _mapper.Map(invoiceDTO, existingInvoice);
                    if (string.IsNullOrEmpty(existingInvoice.InvoiceRequestName))
                    {
                        existingInvoice.InvoiceRequestName = await codeGenerator.GenerateNextCodeAsync<Invoice>("HĐ", c => c.InvoiceRequestName, c => c.Partner.Id == partner.Id);
                    }
                    existingInvoice.OwnerId = employee.Id;
                    existingInvoice.Partner = partner;

                    Console.WriteLine("Updating invoice in database...");
                    _appContext.Invoices.Update(existingInvoice);
                    await _appContext.SaveChangesAsync();
                    Console.WriteLine("Invoice updated in SQL database successfully.");

                    Console.WriteLine("Fetching existing InvoiceOrders from database...");

                    var existingInvoiceOrders = existingInvoice.InvoiceOrders ?? new List<InvoiceOrders>();
                    Console.WriteLine($"Found {existingInvoiceOrders.Count} existing InvoiceOrders.");

                    if (invoiceDTO.Orders != null && invoiceDTO.Orders.Any())
                    {
                        Console.WriteLine($"Processing {invoiceDTO.Orders.Count} Order IDs from InvoiceDTO...");
                        var newOrderIds = invoiceDTO.Orders.ToHashSet();
                        var existingOrderIds = existingInvoiceOrders.Select(io => io.OrderId).ToHashSet();

                        // Xóa các InvoiceOrders không còn trong danh sách mới
                        var orderIdsToDelete = existingOrderIds.Except(newOrderIds).ToList();
                        if (orderIdsToDelete.Any())
                        {
                            Console.WriteLine($"Removing {orderIdsToDelete.Count} obsolete InvoiceOrders...");
                            var toRemove = existingInvoiceOrders.Where(io => orderIdsToDelete.Contains(io.OrderId)).ToList();
                            _appContext.InvoiceOrders.RemoveRange(toRemove);
                            await _appContext.SaveChangesAsync();
                            Console.WriteLine("Obsolete InvoiceOrders removed.");
                        }

                        // Thêm các InvoiceOrders mới
                        var orderIdsToAdd = newOrderIds.Except(existingOrderIds).ToList();
                        if (orderIdsToAdd.Any())
                        {
                            Console.WriteLine($"Adding {orderIdsToAdd.Count} new InvoiceOrders...");
                            var newInvoiceOrders = orderIdsToAdd.Select(orderId => new InvoiceOrders
                            {
                                InvoiceId = existingInvoice.Id,
                                OrderId = orderId,
                                PartnerId = partner.Id
                            }).ToList();
                            await _appContext.InvoiceOrders.AddRangeAsync(newInvoiceOrders);
                            await _appContext.SaveChangesAsync();
                            Console.WriteLine("New InvoiceOrders added.");
                        }
                    }
                    else
                    {
                        // Nếu invoiceDTO.Orders là null hoặc rỗng, xóa tất cả InvoiceOrders hiện tại
                        if (existingInvoiceOrders.Any())
                        {
                            Console.WriteLine("No Orders provided in InvoiceDTO. Removing all existing InvoiceOrders...");
                            _appContext.InvoiceOrders.RemoveRange(existingInvoiceOrders);
                            await _appContext.SaveChangesAsync();
                            Console.WriteLine("All existing InvoiceOrders removed.");
                        }
                    }
                    Console.WriteLine("Fetching existing invoice details from MongoDB...");
                    var existingInvoiceDetails = await _invoicesDetailsCollection.Find(d => d.InvoiceId == id).ToListAsync();
                    Console.WriteLine($"Found {existingInvoiceDetails.Count} existing invoice details.");

                    var updatedInvoiceDetails = new List<ReplaceOneModel<InvoiceDetails>>();
                    var newInvoiceDetails = new List<InvoiceDetails>();
                    var detailIdsToKeep = new HashSet<string>();

                    Console.WriteLine("Processing invoice details...");
                    foreach (var detailDto in invoiceDTO.InvoiceDetails ?? Enumerable.Empty<InvoiceDetailDTO>())
                    {
                        var detailId = string.IsNullOrEmpty(detailDto.Id) ? ObjectId.GenerateNewId().ToString() : detailDto.Id;
                        var existingDetail = existingInvoiceDetails.FirstOrDefault(d => d.Id == detailId);

                        if (existingDetail != null)
                        {
                            Console.WriteLine($"Updating existing detail ID: {detailId}");
                            existingDetail.OrderId = detailDto.OrderId;
                            existingDetail.CustomerId = detailDto.CustomerId;
                            existingDetail.CustomerName = detailDto.CustomerName;
                            existingDetail.ProductId = detailDto.ProductId;
                            existingDetail.ProductCode = detailDto.ProductCode;
                            existingDetail.ProductName = detailDto.ProductName;
                            existingDetail.TaxID = detailDto.TaxID;
                            existingDetail.TaxAmount = detailDto.TaxAmount;
                            existingDetail.TaxIDText = detailDto.TaxIDText;
                            existingDetail.DiscountRate = detailDto.DiscountRate;
                            existingDetail.DiscountAmount = detailDto.DiscountAmount;
                            existingDetail.UnitPrice = detailDto.UnitPrice;
                            existingDetail.QuantityInstock = detailDto.QuantityInstock;
                            existingDetail.UsageUnitID = detailDto.UsageUnitID;
                            existingDetail.UsageUnitIDText = detailDto.UsageUnitIDText;
                            existingDetail.Quantity = detailDto.Quantity;
                            existingDetail.AmountSummary = detailDto.AmountSummary;
                            existingDetail.Total = detailDto.Total;
                            existingDetail.PartnerId = partner.Id;
                            existingDetail.UpdatedAt = DateTime.Now;
                            updatedInvoiceDetails.Add(new ReplaceOneModel<InvoiceDetails>(
                                Builders<InvoiceDetails>.Filter.Eq(d => d.Id, detailId),
                                existingDetail
                            )
                            { IsUpsert = true });
                        }
                        else
                        {
                            Console.WriteLine($"Creating new detail ID: {detailId}");
                            var newDetail = new InvoiceDetails
                            {
                                Id = detailId,
                                PartnerId = partner.Id,
                                InvoiceId = existingInvoice.Id,
                                InvoiceRequestName = existingInvoice.InvoiceRequestName,
                                OrderId = detailDto.OrderId,
                                SaleOrderNo = detailDto.SaleOrderNo,
                                CustomerId = detailDto.CustomerId,
                                CustomerName = detailDto.CustomerName,
                                ProductId = detailDto.ProductId,
                                ProductCode = detailDto.ProductCode,
                                ProductName = detailDto.ProductName,
                                TaxID = detailDto.TaxID,
                                TaxAmount = detailDto.TaxAmount,
                                TaxIDText = detailDto.TaxIDText,
                                DiscountRate = detailDto.DiscountRate,
                                DiscountAmount = detailDto.DiscountAmount,
                                UnitPrice = detailDto.UnitPrice,
                                QuantityInstock = detailDto.QuantityInstock,
                                UsageUnitID = detailDto.UsageUnitID,
                                UsageUnitIDText = detailDto.UsageUnitIDText,
                                Quantity = detailDto.Quantity,
                                Total = detailDto.Total,
                                AmountSummary = detailDto.AmountSummary,
                                CreatedAt = DateTime.Now,
                            };
                            newInvoiceDetails.Add(newDetail);
                        }
                        detailIdsToKeep.Add(detailId);
                    }

                    var detailIdsToDelete = existingInvoiceDetails.Select(d => d.Id).Except(detailIdsToKeep).ToList();
                    if (detailIdsToDelete.Any())
                    {
                        Console.WriteLine($"Deleting {detailIdsToDelete.Count} obsolete invoice details...");
                        await _invoicesDetailsCollection.DeleteManyAsync(d => detailIdsToDelete.Contains(d.Id));
                        Console.WriteLine("Obsolete invoice details deleted.");
                    }

                    if (updatedInvoiceDetails.Any())
                    {
                        Console.WriteLine($"Performing bulk update for {updatedInvoiceDetails.Count} invoice details...");
                        await _invoicesDetailsCollection.BulkWriteAsync(updatedInvoiceDetails);
                        Console.WriteLine("Bulk update completed.");
                    }

                    if (newInvoiceDetails.Any())
                    {
                        Console.WriteLine($"Inserting {newInvoiceDetails.Count} new invoice details...");
                        await _invoicesDetailsCollection.InsertManyAsync(newInvoiceDetails);
                        Console.WriteLine("New invoice details inserted.");
                    }

                    Console.WriteLine("Committing transaction...");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, "Hóa đơn được cập nhật thành công.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }

                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(false, $"Không thể cập nhật hóa đơn: {ex.Message}");
                }
            });
        }

        public async Task<List<InvoiceDTO>> GetAllInvoicesAsync(Employee employee, Partner partner)
        {
            try
            {
                if (employee == null)
                {
                    throw new ArgumentNullException(nameof(employee), "Employee cannot be null.");
                }
                var invoices = await _appContext.Invoices
                      .Where(o =>
                          o.Partner == partner && o.OwnerId == employee.Id ||
                          o.InvoiceEmployees.Any(oe => oe.EmployeeId == employee.Id))
                          .Include(oce => oce.InvoiceEmployees)
                      .ToListAsync();

                if (!invoices.Any())
                {
                    return new List<InvoiceDTO>();
                }
                var invoiceIds = invoices.Select(o => o.Id.ToString()).ToList();
                var orderDetailsDict = (await _invoicesDetailsCollection
                    .Find(d => invoiceIds.Contains(d.InvoiceId.ToString()))
                    .ToListAsync())
                    .GroupBy(d => d.InvoiceId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var invoiceDtos = invoices.Select(invoice =>
                {
                    var dto = _mapper.Map<InvoiceDTO>(invoice);
                    dto.InvoiceDetails = orderDetailsDict.ContainsKey(invoice.Id)
                        ? orderDetailsDict[invoice.Id].Select(d => new InvoiceDetailDTO
                        {
                            Id = d.Id,
                            OrderId = d.OrderId,
                            InvoiceId = d.InvoiceId,
                            PartnerId = d.PartnerId,
                            ProductId = d.ProductId,
                            ProductCode = d.ProductCode,
                            ProductName = d.ProductName,
                            TaxID = d.TaxID,
                            TaxAmount = d.TaxAmount,
                            TaxIDText = d.TaxIDText,
                            DiscountRate = d.DiscountRate,
                            DiscountAmount = d.DiscountAmount,
                            UnitPrice = d.UnitPrice,
                            QuantityInstock = d.QuantityInstock,
                            Total = d.Total,
                            UsageUnitID = d.UsageUnitID,
                            UsageUnitIDText = d.UsageUnitIDText,
                            Quantity = d.Quantity,
                            AmountSummary = d.AmountSummary
                        }).ToList()
                        : new List<InvoiceDetailDTO>();

                    return dto;
                }).ToList();

                return invoiceDtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve invoices: {ex.Message}");
            }
        }


        public async Task<InvoiceDTO?> GetInvoiceByIdAsync(int id, Employee employee, Partner partner)
        {
            try
            {
                if (employee == null)
                {
                    throw new ArgumentNullException(nameof(employee), "Employee cannot be null.");
                }

                var invoice = await _appContext.Invoices
                    .Where(o => o.Id == id &&
                                (o.Partner == partner && o.OwnerId == employee.Id ||
                                 o.InvoiceEmployees.Any(oe => oe.EmployeeId == employee.Id)))
                    .Include(o => o.InvoiceEmployees)
                    .FirstOrDefaultAsync();

                if (invoice == null)
                {
                    throw new KeyNotFoundException($"Order with ID {id} not found for this employee.");
                }

                var invoiceDetails = await _invoicesDetailsCollection
                    .Find(d => d.InvoiceId.Value.ToString() == id.ToString())
                    .ToListAsync();

                var invoiceDto = _mapper.Map<InvoiceDTO>(invoice);
                invoiceDto.InvoiceDetails = invoiceDetails.Select(d => new InvoiceDetailDTO
                {
                    Id = d.Id,
                    OrderId = d.OrderId,
                    InvoiceId = d.InvoiceId,
                    PartnerId = d.PartnerId,
                    ProductId = d.ProductId,
                    ProductCode = d.ProductCode,
                    ProductName = d.ProductName,
                    TaxID = d.TaxID,
                    TaxAmount = d.TaxAmount,
                    TaxIDText = d.TaxIDText,
                    DiscountRate = d.DiscountRate,
                    DiscountAmount = d.DiscountAmount,
                    UnitPrice = d.UnitPrice,
                    QuantityInstock = d.QuantityInstock,
                    UsageUnitID = d.UsageUnitID,
                    Total = d.Total,
                    UsageUnitIDText = d.UsageUnitIDText,
                    Quantity = d.Quantity,
                    AmountSummary = d.AmountSummary
                }).ToList();

                return invoiceDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve order: {ex.Message}");
            }
        }

        public async Task<GeneralResponse?> UpdateFieldIdAsync(int id, InvoiceDTO invoiceDTO, Employee employee, Partner partner)
        {
            using var transaction = await _appContext.Database.BeginTransactionAsync();
            try
            {
                if (invoiceDTO == null || id <= 0)
                {
                    return new GeneralResponse(false, "Vui lòng truyền ID hoá đơn vào !");
                }

                Invoice? existingInvoice = await _appContext.Invoices
                    .Include(c => c.InvoiceEmployees)
                    .FirstOrDefaultAsync(o => o.Id == id
                        && (o.OwnerId == employee.Id || o.InvoiceEmployees.Any(ie => ie.EmployeeId == employee.Id && ie.AccessLevel == AccessLevel.Write))
                        && o.Partner == partner);

                if (existingInvoice == null)
                {
                    return new GeneralResponse(false, "Không thể cập nhật hoá đơn hiện tại");
                }

                // Overwrite all fields
                _mapper.Map(invoiceDTO, existingInvoice);

                existingInvoice.OwnerId = employee.Id;
                existingInvoice.Partner = partner;

                _appContext.Invoices.Update(existingInvoice);
                await _appContext.SaveChangesAsync();

                // Update MongoDB Invoice Details
                var existingInvoiceDetails = await _invoicesDetailsCollection.Find(d => d.InvoiceId == id).ToListAsync();

                var updatedInvoiceDetails = new List<ReplaceOneModel<InvoiceDetails>>();
                var newInvoiceDetails = new List<InvoiceDetails>();
                var detailIdsToKeep = new HashSet<string>();

                foreach (var detailDto in invoiceDTO.InvoiceDetails)
                {
                    var detailId = string.IsNullOrEmpty(detailDto.Id) ? ObjectId.GenerateNewId().ToString() : detailDto.Id;
                    var existingDetail = existingInvoiceDetails.FirstOrDefault(d => d.Id == detailId);

                    if (existingDetail != null)
                    {
                        // Overwrite all fields in existing detail
                        _mapper.Map(detailDto, existingDetail);
                        existingDetail.PartnerId = partner.Id;

                        updatedInvoiceDetails.Add(new ReplaceOneModel<InvoiceDetails>(
                            Builders<InvoiceDetails>.Filter.Eq(d => d.Id, detailId),
                            existingDetail
                        )
                        { IsUpsert = true });
                    }
                    else
                    {
                        var newDetail = _mapper.Map<InvoiceDetails>(detailDto);
                        newDetail.Id = detailId;
                        newDetail.InvoiceId = existingInvoice.Id;
                        newDetail.PartnerId = partner.Id;

                        newInvoiceDetails.Add(newDetail);
                    }

                    detailIdsToKeep.Add(detailId);
                }

                // Remove details not included in the update request
                var detailIdsToDelete = existingInvoiceDetails.Select(d => d.Id).Except(detailIdsToKeep).ToList();
                if (detailIdsToDelete.Any())
                {
                    await _invoicesDetailsCollection.DeleteManyAsync(d => detailIdsToDelete.Contains(d.Id));
                }

                // Perform Bulk Update in MongoDB
                if (updatedInvoiceDetails.Any())
                {
                    await _invoicesDetailsCollection.BulkWriteAsync(updatedInvoiceDetails);
                }
                if (newInvoiceDetails.Any())
                {
                    await _invoicesDetailsCollection.InsertManyAsync(newInvoiceDetails);
                }

                await transaction.CommitAsync();
                return new GeneralResponse(true, "Invoice updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new GeneralResponse(false, $"Failed to update Invoice: {ex.Message}");
            }
        }

        public async Task<GeneralResponse?> DeleteBulkInvoicesAsync(string ids, Employee employee, Partner partner)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ids))
                {
                    return new GeneralResponse(false, "Invoice IDs cannot be null or empty.");
                }

                if (employee == null)
                {
                    return new GeneralResponse(false, "Employee cannot be null.");
                }

                var idList = ids.Split(',')
                    .Select(id => int.TryParse(id.Trim(), out int parsedId) ? parsedId : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (!idList.Any())
                {
                    return new GeneralResponse(false, "No valid Invoice IDs provided.");
                }

                var invoicesToDelete = await _appContext.Invoices
                    .Where(o => idList.Contains(o.Id) && o.OwnerId == employee.Id)
                    .ToListAsync();

                if (!invoicesToDelete.Any())
                {
                    throw new KeyNotFoundException("No invoices found for deletion.");
                }

                var orderIdStrings = invoicesToDelete.Select(o => o.Id.ToString()).ToList();

                await DeleteBulkInvoiceDetailsAsync(orderIdStrings);

                _appContext.Invoices.RemoveRange(invoicesToDelete);
                await _appContext.SaveChangesAsync();

                return new GeneralResponse(true, "Remove invoices successfully");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete invoices: {ex.Message}");
            }
        }



        private async Task<bool> DeleteBulkInvoiceDetailsAsync(List<string> invoiceIds)
        {
            try
            {
                if (invoiceIds == null || !invoiceIds.Any())
                {
                    throw new ArgumentException("Invoice ID list cannot be null or empty.", nameof(invoiceIds));
                }

                var deleteResult = await _invoicesDetailsCollection.DeleteManyAsync(d => invoiceIds.Contains(d.InvoiceId.ToString()));

                return deleteResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete invoice details: {ex.Message}");
            }
        }

        public async Task<GeneralResponse?> BulkUpdateInvoicesAsync(List<int> invoiceIds, int? ContactId, int? CustomerId, Employee employee, Partner partner)
        {
            using var transaction = await _appContext.Database.BeginTransactionAsync();
            try
            {
                // Get all invoices that match the provided Order IDs and belong to the employee/partner
                var invoicesToUpdate = await _appContext.Invoices
                    .Where(o => invoiceIds.Contains(o.Id)
                                && (o.OwnerId == employee.Id
                                    || o.InvoiceEmployees.Any(oe => oe.EmployeeId == employee.Id
                                    && oe.AccessLevel == AccessLevel.Write))
                                && o.Partner == partner)
                    .ToListAsync();

                if (!invoicesToUpdate.Any())
                {
                    return new GeneralResponse(false, "Hoá đơn không được tìm thấy.");
                }

                foreach (var invoice in invoicesToUpdate)
                {
                    if (ContactId.HasValue)
                        if (ContactId.Value == 0)
                            invoice.BuyerId = null;
                        else
                            invoice.BuyerId = ContactId.Value;

                    if (CustomerId.HasValue)
                        if (CustomerId.Value == 0)
                            invoice.CustomerId = null;
                        else
                            invoice.CustomerId = CustomerId.Value;
                }

                _appContext.Invoices.UpdateRange(invoicesToUpdate);
                await _appContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return new GeneralResponse(true, $"Cập nhật thành công {invoicesToUpdate.Count} hoá đơn.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new GeneralResponse(false, $"Cập nhật hoá đơn không thành công: {ex.Message}");
            }
        }

        public async Task<List<OrderInvoiceDTO>> GetOrdersByInvoiceIdAsync(int invoiceId, Partner partner)
        {
            Console.WriteLine($"Starting GetOrdersByInvoiceIdAsync for Invoice ID: {invoiceId}, Partner ID: {partner?.Id}");
            var strategy = _appContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                Console.WriteLine("Execution strategy started.");
                try
                {
                    // Tìm Invoice và các OrderId liên quan qua InvoiceOrders
                    Console.WriteLine($"Fetching InvoiceOrders for Invoice ID {invoiceId}...");
                    var orderIds = await _appContext.InvoiceOrders
                        .Where(io => io.InvoiceId == invoiceId && io.PartnerId == partner.Id)
                        .Select(io => io.OrderId)
                        .ToListAsync();

                    if (!orderIds.Any())
                    {
                        Console.WriteLine($"No OrderIds found for Invoice ID {invoiceId}.");
                        return new List<OrderInvoiceDTO>(); // Trả về danh sách rỗng nếu không có OrderId
                    }
                    Console.WriteLine($"Found {orderIds.Count} OrderIds: [{string.Join(", ", orderIds)}]");

                    // Lấy thông tin Orders dựa trên OrderIds
                    Console.WriteLine($"Fetching Orders for OrderIds...");
                    var orders = await _appContext.Orders
                      .Where(o => orderIds.Contains(o.Id) && o.Partner.Id == partner.Id)
                      .ToListAsync();

                    Console.WriteLine($"Mapping {orders.Count} Orders to OrderInvoiceDTOs...");
                    var orderDtos = _mapper.Map<List<OrderInvoiceDTO>>(orders);

                    Console.WriteLine($"Retrieved {orderDtos.Count} OrderInvoiceDTOs.");
                    return orderDtos;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    return new List<OrderInvoiceDTO>();
                }
            });
        }

        public async Task<List<Activity>> GetAllActivitiesByIdAsync(int id, Employee employee, Partner partner)
        {
                if (employee == null || partner == null)
            {
                throw new ArgumentNullException(nameof(employee), "ID nhân viên và ID tổ chức không đuọc bỏ trống.");
            }
            var activities = await _appContext.Activities
            // .Include(c => c.ActivityEmployees)
            .Where(c => c.InvoiceId == id && c.TaskOwnerId == employee.Id && c.PartnerId == partner.Id)
            .ToListAsync();

            return activities.Any() ? activities : new List<Activity>();
        }
    }
}