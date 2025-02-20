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

            using var transaction = await _appContext.Database.BeginTransactionAsync();
            try
            {
                if (invoiceDto == null || invoiceDto.InvoiceDetails.Count == 0)
                {
                    return new GeneralResponse(false, "Invalid invoice or invoice details.");
                }
                var invoice = _mapper.Map<Invoice>(invoiceDto);

                invoice.OwnerId = employee.Id;
                invoice.Partner = partner;
                // ** Store Order IDs
                invoice.Orders = invoiceDto.Orders != null ? await _appContext.Orders
        .Where(o => invoiceDto.Orders.Contains(o.Id))
        .ToListAsync() : new List<Order>();

                _appContext.Invoices.Add(invoice);
                await _appContext.SaveChangesAsync();
                // Create OrderDetails in MongoDB
                var invoiceDetails = invoiceDto.InvoiceDetails.Select(detailDto => new InvoiceDetails
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    InvoiceId = invoice.Id,
                    PartnerId = partner.Id,
                    OrderId = detailDto.OrderId,
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
                }).ToList();
                await _invoicesDetailsCollection.InsertManyAsync(invoiceDetails);

                await transaction.CommitAsync();
                return new GeneralResponse(true, $"Invoice created successfully. Invoice ID: {invoice.Id}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new GeneralResponse(false, $"Failed to create invoice: {ex.Message}");
            }
        }

        public async Task<GeneralResponse?> UpdateInvoiceAsync(int id, InvoiceDTO invoiceDTO,
         Employee employee, Partner partner)
        {
            using var transaction = await _appContext.Database.BeginTransactionAsync();
            try
            {
                Invoice? existingInvoice = null;
                existingInvoice = await _appContext.Invoices.Include(c => c.InvoiceEmployees).FirstOrDefaultAsync(o => o.Id == id
              && o.OwnerId == employee.Id || o.InvoiceEmployees.Any(oe => oe.EmployeeId == employee.Id && oe.AccessLevel == AccessLevel.Write)
              && o.Partner == partner);

                if (existingInvoice == null)
                {
                    return new GeneralResponse(false, "Invoice not found.");
                }

                _mapper.Map(invoiceDTO, existingInvoice);


                existingInvoice.OwnerId = employee.Id;
                existingInvoice.Partner = partner;

                _appContext.Invoices.Update(existingInvoice);
                await _appContext.SaveChangesAsync();


                var existingInvoiceDetails = await _invoicesDetailsCollection.Find(d => d.InvoiceId == id).ToListAsync();

                //  Prepare OrderDetails for MongoDB Update
                var updatedInvoiceDetails = new List<ReplaceOneModel<InvoiceDetails>>();
                var newInvoiceDetails = new List<InvoiceDetails>();
                var detailIdsToKeep = new HashSet<string>();

                foreach (var detailDto in invoiceDTO.InvoiceDetails)
                {
                    var detailId = string.IsNullOrEmpty(detailDto.Id) ? ObjectId.GenerateNewId().ToString() : detailDto.Id;
                    var existingDetail = existingInvoiceDetails.FirstOrDefault(d => d.Id == detailId);

                    if (existingDetail != null)
                    {
                        existingDetail.OrderId = detailDto.OrderId;
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

                        updatedInvoiceDetails.Add(new ReplaceOneModel<InvoiceDetails>(
                            Builders<InvoiceDetails>.Filter.Eq(d => d.Id, detailId),
                            existingDetail
                        )
                        { IsUpsert = true });
                    }
                    else
                    {
                        var newDetail = new InvoiceDetails
                        {
                            Id = detailId,
                            PartnerId = partner.Id,
                            InvoiceId = existingInvoice.Id,
                            OrderId = detailDto.OrderId,
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
                            AmountSummary = detailDto.AmountSummary
                        };
                        newInvoiceDetails.Add(newDetail);
                    }

                    detailIdsToKeep.Add(detailId);
                }


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
                    return new GeneralResponse(false, "Invalid invoice data provided.");
                }

                Invoice? existingInvoice = await _appContext.Invoices
                    .Include(c => c.InvoiceEmployees)
                    .FirstOrDefaultAsync(o => o.Id == id
                        && (o.OwnerId == employee.Id || o.InvoiceEmployees.Any(ie => ie.EmployeeId == employee.Id && ie.AccessLevel == AccessLevel.Write))
                        && o.Partner == partner);

                if (existingInvoice == null)
                {
                    return new GeneralResponse(false, "Invoice not found.");
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
    }
}