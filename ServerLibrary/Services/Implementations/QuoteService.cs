using AutoMapper;
using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.Enums;
using Data.MongoModels;
using Data.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class QuoteService : BaseService, IQuoteService
    {
        private readonly AppDbContext _appContext;
        private readonly IMapper _mapper;
        private readonly IMongoCollection<QuoteDetails> _quotesDetailsCollection;

        private readonly ICustomerService _customerService;

        public QuoteService(
            MongoDbContext dbContext,
            AppDbContext appContext,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ICustomerService customerService
        )
            : base(appContext, httpContextAccessor)
        {
            _quotesDetailsCollection = dbContext.QuoteDetails;
            _appContext = appContext;
            _mapper = mapper;
            _customerService = customerService;
        }
        private async Task<OptionalQuoteDTO> GetQuoteByCode(string code, Partner partner)
        {
            var existingQuote = await _appDbContext.Quotes
                .FirstOrDefaultAsync(c => c.QuoteNo == code && c.PartnerId == partner.Id);
            if (existingQuote == null)
                return null;

            return new OptionalQuoteDTO
            {
                Id = existingQuote.Id,
                QuoteNo = existingQuote.QuoteNo,
                QuoteDate = existingQuote.QuoteDate,
            };
        }
        public async Task<DataObjectResponse?> CheckQuoteCodeAsync(string code, Employee employee, Partner partner)
        {
            var quoteDetail = await GetQuoteByCode(code, partner);

            if (quoteDetail == null)
            {
                return new DataObjectResponse(true, "Mã báo giá có thể sử dụng", null);
            }
            else
            {
                return new DataObjectResponse(false, "Mã báo giá đã tồn tại", new
                {
                    quoteDetail.QuoteNo,
                    quoteDetail.QuoteDate,
                    quoteDetail.Id
                });
            }
        }

        public async Task<GeneralResponse?> CreateQuoteAsync(CreateQuoteDTO quoteDto, Employee employee, Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);
            var strategy = _appContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    if (quoteDto == null)
                    {
                        return new GeneralResponse(false, "Báo giá không hợp lệ.");
                    }
                    if (quoteDto.QuoteDetails == null)
                    {
                        return new GeneralResponse(false, "Hàng hoá không được để trống.");
                    }
                    var checkCodeExisted = await CheckQuoteCodeAsync(quoteDto.QuoteNo, employee, partner);

                    if (checkCodeExisted != null && !checkCodeExisted.Flag)
                        return new GeneralResponse(false, "Mã Báo giá đã tồn tại !");

                    var quote = _mapper.Map<Quote>(quoteDto);
                    if (string.IsNullOrEmpty(quote.QuoteNo))
                    {
                        quote.QuoteNo = await codeGenerator.GenerateNextCodeAsync<Quote>(
                            "BG",
                            c => c.QuoteNo,
                            c => c.Partner.Id == partner.Id
                        );
                    }
                    quote.OwnerTaskExecuteId = employee.Id;
                    quote.OwnerTaskExecuteName = employee.FullName;
                    quote.Partner = partner;

                    _appContext.Quotes.Add(quote);
                    await _appContext.SaveChangesAsync();

                    var quoteDetails = quoteDto
                        .QuoteDetails.Select(detailDto => new QuoteDetails
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            QuoteId = quote.Id,
                            QuoteNo = quote.QuoteNo,
                            PartnerId = partner.Id,
                            PartnerName = partner.Name,
                            Avatar = detailDto.Avatar,
                            // ** Order Detail Manually
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
                            CustomerId = detailDto.CustomerId,
                            CustomerName = detailDto.CustomerName,
                            CreatedAt = DateTime.Now,
                        })
                        .ToList();

                    // có orders mới chạy code
                    if (quoteDetails.Any())
                    {
                        try
                        {
                            await _quotesDetailsCollection.InsertManyAsync(quoteDetails);
                        }
                        catch (MongoDB.Driver.MongoConnectionException ex)
                        {
                            throw new Exception($"MongoDB connection failed: {ex.Message}", ex);
                        }
                    }

                    await transaction.CommitAsync();
                    return new GeneralResponse(
                        true,
                        $"Tạo báo giá thành công. Quote ID: {quote.Id}"
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new GeneralResponse(
                        false,
                        $"Lỗi khi lấy thông tin đơn hàng: {ex.Message}"
                    );
                }
            });
        }

        private async Task<bool> DeleteBulkQuoteDetailsAsync(List<string> quoteIds)
        {
            try
            {
                if (quoteIds == null || !quoteIds.Any())
                {
                    throw new ArgumentException(
                        "Quote ID list cannot be null or empty.",
                        nameof(quoteIds)
                    );
                }

                var deleteResult = await _quotesDetailsCollection.DeleteManyAsync(d =>
                    quoteIds.Contains(d.QuoteId.ToString())
                );

                return deleteResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete quote details: {ex.Message}");
            }
        }
        public async Task<GeneralResponse?> DeleteBulkQuotesAsync(string ids, Employee employee, Partner partner)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ids))
                {
                    return new GeneralResponse(false, "Mã báo giá không được để trống.");
                }

                if (employee == null)
                {
                    return new GeneralResponse(false, "Không tìm thấy nhân viên.");
                }

                var idList = ids.Split(',')
                    .Select(id => int.TryParse(id.Trim(), out int parsedId) ? parsedId : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (!idList.Any())
                {
                    return new GeneralResponse(false, "Mã báo giá không hợp lệ.");
                }

                var quotesToDelete = await _appContext
                    .Quotes.Where(o => idList.Contains(o.Id) && o.OwnerTaskExecuteId == employee.Id)
                    .ToListAsync();

                if (!quotesToDelete.Any())
                {
                    throw new KeyNotFoundException("Không báo giá nào được xoá.");
                }

                var quoteIdStrings = quotesToDelete.Select(o => o.Id.ToString()).ToList();

                await DeleteBulkQuoteDetailsAsync(quoteIdStrings);

                _appContext.Quotes.RemoveRange(quotesToDelete);
                await _appContext.SaveChangesAsync();

                return new GeneralResponse(true, "Xoá báo giá thành công");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo báo giá: {ex.Message}");
            }
        }

        public async Task<DataObjectResponse?> GenerateQuoteCodeAsync(Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);

            var quoteCode = await codeGenerator
            .GenerateNextCodeAsync<Quote>(prefix: "BG",
                codeSelector: c => c.QuoteNo,
                filter: c => c.PartnerId == partner.Id);

            return new DataObjectResponse(true, "Tạo mã báo giá thành công", quoteCode);
        }

        public async Task<List<ActivityDTO?>> GetAllActivitiesByQuoteAsync(int id, Employee employee, Partner partner)
        {
            var doneStatus = ((char)ActivityStatus.Done).ToString();
            if (id == 0)
            {
                throw new NotImplementedException();
            }
            if (partner == null)
            {
                throw new Exception("Không tìm thấy đối tác");
            }
            var activities = await _appDbContext.Activities
            .Where(
                c => c.QuoteId == id
                && c.PartnerId == partner.Id
                && c.StatusID != doneStatus
                ).ToListAsync();
            var activitiesDtos = _mapper.Map<List<ActivityDTO>>(activities);
            return activitiesDtos;
        }

        public async Task<List<OptionalQuoteDTO>> GetAllQuotesAsync(Employee employee, Partner partner)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(
                    nameof(employee),
                    "Vui lòng không để trống ID Employee."
                );
            }
            if (partner == null)
            {
                throw new ArgumentNullException(
                    nameof(partner),
                    "Vui lòng không để trống đối tác."
                );
            }
            try
            {
                IQueryable<Quote> query = _appContext
                    .Quotes.Where(o => o.Partner == partner)
                    .AsNoTracking();
                if (!IsOwner)
                {
                    query = query
                        .Where(o =>
                            o.OwnerTaskExecuteId == employee.Id || o.PartnerId == partner.Id);
                }

                var quotes = await query.ToListAsync();
                if (quotes.Count == 0)
                    return new List<OptionalQuoteDTO>();

                var quoteIds = quotes.Select(o => o.Id).Where(id => id != null).ToList();

                // // 🔹 Truy vấn QuoteDetails từ MongoDB
                // var quoteDetailsList = await _quotesDetailsCollection
                //     .Find(d => quoteIds.Contains(d.QuoteId.Value))
                //     .ToListAsync();

                // var quoteDetailsDict = quoteDetailsList
                //     .GroupBy(d => d.QuoteId)
                //     .ToDictionary(g => g.Key, g => g.ToList());

                // // 🔹 Ánh xạ sang DTO
                // var quoteDtos = quotes
                //     .Select(quote =>
                //     {
                //         var dto = _mapper.Map<QuoteDTO>(quote);
                //         dto.QuoteDetails = quoteDetailsDict.TryGetValue(quote.Id, out var details)
                //             ? details
                //                 .Select(d => new QuoteDetailsDTO
                //                 {
                //                     Id = d.Id,
                //                     QuoteId = d.QuoteId,
                //                     PartnerId = d.PartnerId,
                //                     ProductId = d.ProductId,
                //                     CustomerId = d.CustomerId,
                //                     CustomerName = d.CustomerName,
                //                     PartnerName = d.PartnerName,
                //                     QuoteNo = d.QuoteNo,
                //                     ProductCode = d.ProductCode,
                //                     ProductName = d.ProductName,
                //                     TaxID = d.TaxID,
                //                     TaxAmount = d.TaxAmount,
                //                     Avatar = d.Avatar,
                //                     TaxIDText = d.TaxIDText,
                //                     DiscountRate = d.DiscountRate,
                //                     DiscountAmount = d.DiscountAmount,
                //                     UnitPrice = d.UnitPrice,
                //                     QuantityInstock = d.QuantityInstock,
                //                     Total = d.Total,
                //                     UsageUnitID = d.UsageUnitID,
                //                     UsageUnitIDText = d.UsageUnitIDText,
                //                     Quantity = d.Quantity,
                //                     AmountSummary = d.AmountSummary,
                //                     CreatedAt = d.CreatedAt,
                //                     UpdatedAt = d.UpdatedAt,
                //                 })
                //                 .ToList()
                //             : new List<QuoteDetailsDTO>();

                //         return dto;
                //     })
                //     .ToList();

                return quotes.Select(quote =>
                {
                    var dto = _mapper.Map<OptionalQuoteDTO>(quote);
                    return dto;
                }).ToList();
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentException($"Lỗi tham số đầu vào: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thông tin đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<QuoteDTO?> GetQuoteByIdAsync(int id, Employee employee, Partner partner)
        {
            try
            {
                if (employee == null)
                {
                    throw new ArgumentNullException(
                        nameof(employee),
                        "Vui lòng không để trống ID Nhân viên."
                    );
                }

                var quote = await _appContext
                    .Quotes.Where(o =>
                        o.Id == id
                        && (
                            o.Partner == partner && o.OwnerTaskExecuteId == employee.Id
                        // || o.OrderEmployees.Any(oe => oe.EmployeeId == employee.Id)
                        )
                    )
                    // .Include(o => o.OrderEmployees)
                    .FirstOrDefaultAsync();

                if (quote == null)
                {
                    throw new KeyNotFoundException(
                        $"Báo giá của ID {id} không được tìm thấy trên nhân viên này."
                    );
                }

                var quoteDetails = await _quotesDetailsCollection
                    .Find(d => d.QuoteId.Value.ToString() == id.ToString())
                    .ToListAsync();

                var quoteDto = _mapper.Map<QuoteDTO>(quote);
                quoteDto.QuoteDetails = quoteDetails
                    .Select(d => new QuoteDetailsDTO
                    {
                        Id = d.Id,
                        QuoteId = d.QuoteId,
                        PartnerId = d.PartnerId,
                        ProductId = d.ProductId,
                        Avatar = d.Avatar,
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
                        AmountSummary = d.AmountSummary,
                        CustomerId = d.CustomerId,
                        CustomerName = d.CustomerName,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt,
                    })
                    .ToList();

                return quoteDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thông tin báo giá: {ex.Message}");
            }
        }

        public Task<GeneralResponse?> UpdateFieldIdAsync(int id, UpdateQuoteDTO quotes, Employee employee, Partner partner)
        {
            throw new NotImplementedException();
        }

        public async Task<GeneralResponse?> UpdateQuoteAsync(int id, UpdateQuoteDTO quotes, Employee employee, Partner partner)
        {
            var strategy = _appContext.Database.CreateExecutionStrategy();
            Console.WriteLine(
                $"Starting UpdateQuoteAsync for Quote ID: {id}, Employee ID: {employee?.Id}, Partner ID: {partner?.Id}"
            );
            var codeGenerator = new GenerateNextCode(_appDbContext);

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Transaction started.");

                    Console.WriteLine("Fetching existing order from database...");
                    Quote? existingQuote = null;
                    existingQuote = await _appContext
                        .Quotes
                        .FirstOrDefaultAsync(o =>
                            o.Id == id && o.OwnerTaskExecuteId == employee.Id
                                && o.Partner == partner
                        );
                    if (existingQuote == null)
                    {
                        Console.WriteLine($"Quote with ID {id} not found.");
                        return new GeneralResponse(false, "Không tìm thấy báo giá");
                    }

                    if (string.IsNullOrEmpty(existingQuote.QuoteNo))
                    {
                        string originalCode = existingQuote.QuoteNo;
                        bool exists = await _appDbContext.Quotes.AnyAsync(c =>
             c.QuoteNo == existingQuote.QuoteNo &&
             c.PartnerId == partner.Id &&
             c.Id != id);
                        if (exists)
                        {
                            existingQuote.QuoteNo = await codeGenerator.GenerateNextCodeAsync<Quote>("BG", c => c.QuoteNo, c => c.PartnerId == partner.Id);
                            Console.WriteLine($"QuoteNo '{originalCode}' already existed. Replaced with '{existingQuote.QuoteNo}' for QuoteNo ID {id}.");
                        }
                    }
                    Console.WriteLine(
                        $"Quote found: ID {existingQuote.Id}, OwnerId {existingQuote.OwnerTaskExecuteId}, PartnerId {existingQuote.PartnerId}"
                    );
                    Console.WriteLine("Mapping orderDTO to existingOrder...");
                    _mapper.Map(quotes, existingQuote);
                    Console.WriteLine("Updating Quote in database...");
                    _appContext.Quotes.Update(existingQuote);
                    await _appContext.SaveChangesAsync();
                    Console.WriteLine("Quote updated in SQL database successfully.");

                    var existingQuoteDetails = await _quotesDetailsCollection
                        .Find(d => d.QuoteId == id)
                        .ToListAsync();
                    Console.WriteLine(
                        $"Found {existingQuoteDetails.Count} existing quote details."
                    );
                    //  Prepare QuoteDetails for MongoDB Update
                    var updatedQuoteDetails = new List<ReplaceOneModel<QuoteDetails>>();
                    var newQuoteDetails = new List<QuoteDetails>();
                    var detailIdsToKeep = new HashSet<string>();
                    Console.WriteLine("Processing quote details...");
                    foreach (var detailDto in quotes.QuoteDetails)
                    {
                        var detailId = string.IsNullOrEmpty(detailDto.Id)
                            ? ObjectId.GenerateNewId().ToString()
                            : detailDto.Id;
                        var existingDetail = existingQuoteDetails.FirstOrDefault(d =>
                            d.Id == detailId
                        );

                        if (existingDetail != null)
                        {
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
                            existingDetail.CustomerId = detailDto.CustomerId;
                            existingDetail.CustomerName = detailDto.CustomerName;
                            updatedQuoteDetails.Add(
                                new ReplaceOneModel<QuoteDetails>(
                                    Builders<QuoteDetails>.Filter.Eq(d => d.Id, detailId),
                                    existingDetail
                                )
                                {
                                    IsUpsert = true,
                                }
                            );
                        }
                        else
                        {
                            Console.WriteLine($"Creating new detail ID: {detailId}");
                            var newDetail = new QuoteDetails
                            {
                                Id = detailId,
                                QuoteId = existingQuote.Id,
                                QuoteNo = existingQuote.QuoteNo,
                                Avatar = detailDto.Avatar,
                                PartnerId = partner.Id,
                                PartnerName = existingQuote.Partner.Name,
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
                                CustomerId = detailDto.CustomerId,
                                CustomerName = detailDto.CustomerName,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                            };
                            newQuoteDetails.Add(newDetail);
                        }

                        detailIdsToKeep.Add(detailId);
                    }

                    var detailIdsToDelete = existingQuoteDetails
                        .Select(d => d.Id)
                        .Except(detailIdsToKeep)
                        .ToList();
                    if (detailIdsToDelete.Any())
                    {
                        Console.WriteLine(
                            $"Deleting {detailIdsToDelete.Count} obsolete quote details..."
                        );
                        await _quotesDetailsCollection.DeleteManyAsync(d =>
                            detailIdsToDelete.Contains(d.Id)
                        );
                        Console.WriteLine("Obsolete quote details deleted.");
                    }

                    // Perform Bulk Update in MongoDB
                    if (updatedQuoteDetails.Any())
                    {
                        Console.WriteLine(
                            $"Performing bulk update for {updatedQuoteDetails.Count} quote details..."
                        );
                        await _quotesDetailsCollection.BulkWriteAsync(updatedQuoteDetails);
                        Console.WriteLine("Bulk update completed.");
                    }
                    if (newQuoteDetails.Any())
                    {
                        Console.WriteLine(
                            $"Inserting {newQuoteDetails.Count} new quote details..."
                        );
                        await _quotesDetailsCollection.InsertManyAsync(newQuoteDetails);
                        Console.WriteLine("New quote details inserted.");
                    }
                    Console.WriteLine("Committing transaction...");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, "Cập nhật báo giá thành công");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(
                        false,
                        $"Lỗi khi lấy thông tin báo giá: {ex.Message}"
                    );
                }
            });
        }

        public async Task<List<OptionalOrderDTO>> GetAllOrdersByQuoteAsync(int id, Employee employee, Partner partner)
        {
            if (id == null)
            {
                throw new ArgumentException("ID báo giá không được để trống !");
            }
            if (partner == null)
            {
                throw new ArgumentException("Thông tin tổ chức không được bỏ trống");
            }

            var orders = await _appDbContext.Orders
            .Where(o => o.QuoteId == id && o.Partner.Id == partner.Id)
            .ToListAsync();

            if (!orders.Any())
            {
                return new List<OptionalOrderDTO>();
            }
            var orderDtos = orders.Select(order =>
            {
                var dto = _mapper.Map<OptionalOrderDTO>(order);
                return dto;
            }).ToList();

            return orderDtos;
        }

        public async Task<GeneralResponse> UnassignActivityFromId(int id, int activityId, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Execution strategy started.");

                    if (id == null)
                    {
                        Console.WriteLine($"No Quote found for ID {id}.");
                        return new GeneralResponse(true, $"ID {id} không liên kết với báo giá nào.");
                    }

                    var activity = await _appDbContext.Activities
                        .FirstOrDefaultAsync(a => a.Id == activityId && a.QuoteId == id && a.PartnerId == partner.Id);

                    if (activity == null)
                    {
                        Console.WriteLine($"No Activity found for ID {activityId}.");
                        return new GeneralResponse(true, $"ID {activityId} không liên kết với hoạt động nào.");
                    }

                    activity.QuoteId = null;
                    _appDbContext.Activities.Update(activity);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine("Activity removed successfully.");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Đã xóa hoạt động khỏi báo giá ID {id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(false, $"Lỗi khi xóa hoạt động khỏi báo giá ID {id}: {ex.Message}");
                }
            });
        }

        public async Task<List<ActivityDTO>> GetAllActivitiesDoneByQuoteAsync(int id, Employee employee, Partner partner)
        {
            var doneStatus = ((char)ActivityStatus.Done).ToString();

            if (id == 0)
            {
                throw new NotImplementedException();
            }
            if (partner == null)
            {
                throw new Exception("Không tìm thấy đối tác ");
            }
            var activities = await _appDbContext.Activities.Where(c => c.QuoteId == id
            && c.PartnerId == partner.Id
            && c.StatusID == doneStatus
            ).ToListAsync();
            if (activities.Count == 0)
            {
                return new List<ActivityDTO>();
            }
            return _mapper.Map<List<ActivityDTO>>(activities);
        }
    }
}