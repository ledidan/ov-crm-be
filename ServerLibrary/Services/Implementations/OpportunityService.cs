using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.MongoModels;
using Data.Responses;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class OpportunityService : BaseService, IOpportunityService
    {
        private readonly AppDbContext _appContext;
        private readonly IMapper _mapper;
        private readonly IMongoCollection<OpportunityProductDetails> _opportunityProductDetails;

        private readonly ICustomerService _customerService;

        public OpportunityService(
            MongoDbContext dbContext,
            AppDbContext appContext,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ICustomerService customerService
        )
            : base(appContext, httpContextAccessor)
        {
            _opportunityProductDetails = dbContext.OpportunityProductDetails;
            _appContext = appContext;
            _mapper = mapper;
            _customerService = customerService;
        }
        private async Task<OptionalOpportunityDTO> GetOpportunityByCode(string code,
        Partner partner)
        {
            var existingOpportunity = await _appDbContext.Opportunities
                .FirstOrDefaultAsync(c => c.OpportunityNo == code && c.PartnerId == partner.Id);
            if (existingOpportunity == null)
                return null;

            return new OptionalOpportunityDTO
            {
                Id = existingOpportunity.Id,
                OpportunityNo = existingOpportunity.OpportunityNo,
                OpportunityName = existingOpportunity.OpportunityName,
            };
        }

        public async Task<DataObjectResponse?> CheckOpportunityCodeAsync(string code,
        Employee employee,
        Partner partner)
        {
            var opportunityDetail = await GetOpportunityByCode(code, partner);

            if (opportunityDetail == null)
            {
                return new DataObjectResponse(true, "Mã cơ hội có thể sử dụng", null);
            }
            else
            {
                return new DataObjectResponse(false, "Mã cơ hội đã tồn tại", new
                {
                    opportunityDetail.OpportunityNo,
                    opportunityDetail.OpportunityName,
                    opportunityDetail.Id
                });
            }
        }

        public async Task<GeneralResponse?> CreateOpportunityAsync(CreateOpportunityDTO opportunityDto, Employee employee, Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);
            var strategy = _appContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    if (opportunityDto == null)
                    {
                        return new GeneralResponse(false, "Cơ hội không hợp lệ.");
                    }
                    if (opportunityDto.OpportunityProductDetails == null)
                    {
                        return new GeneralResponse(false, "Hàng hoá không được để trống.");
                    }
                    var checkCodeExisted = await CheckOpportunityCodeAsync(opportunityDto.OpportunityNo, employee, partner);

                    if (checkCodeExisted != null && !checkCodeExisted.Flag)
                        return new GeneralResponse(false, "Mã Cơ hội đã tồn tại !");

                    var opportunity = _mapper.Map<Opportunity>(opportunityDto);
                    if (string.IsNullOrEmpty(opportunity.OpportunityNo))
                    {
                        opportunity.OpportunityNo = await codeGenerator.GenerateNextCodeAsync<Opportunity>(
                            "CH",
                            c => c.OpportunityNo,
                            c => c.Partner.Id == partner.Id
                        );
                    }
                    opportunity.OwnerTaskExecuteId = employee.Id;
                    opportunity.OwnerTaskExecuteName = employee.FullName;
                    opportunity.Partner = partner;

                    _appContext.Opportunities.Add(opportunity);
                    await _appContext.SaveChangesAsync();

                    var opportunityDetails = opportunityDto
                    .OpportunityProductDetails
                        .Select(detailDto => new OpportunityProductDetails
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            OpportunityId = opportunity.Id,
                            OpportunityName = opportunity.OpportunityName,
                            OpportunityNo = opportunity.OpportunityNo,
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
                    if (opportunityDetails.Any())
                    {
                        try
                        {
                            await _opportunityProductDetails.InsertManyAsync(opportunityDetails);
                        }
                        catch (MongoDB.Driver.MongoConnectionException ex)
                        {
                            throw new Exception($"MongoDB connection failed: {ex.Message}", ex);
                        }
                    }

                    await transaction.CommitAsync();
                    return new GeneralResponse(
                        true,
                        $"Tạo cơ hội thành công ID Cơ hội: {opportunity.OpportunityNo}"
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
        private async Task<bool> DeleteBulkOpportunitiesDetailsAsync(List<string> opportunityIds)
        {
            try
            {
                if (opportunityIds == null || !opportunityIds.Any())
                {
                    throw new ArgumentException(
                        "Opportunity ID list cannot be null or empty.",
                        nameof(opportunityIds)
                    );
                }

                var deleteResult = await _opportunityProductDetails.DeleteManyAsync(d =>
                    opportunityIds.Contains(d.OpportunityId.ToString())
                );

                return deleteResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete opportunity details: {ex.Message}");
            }
        }

        public async Task<GeneralResponse?> DeleteBulkOpportunitiesAsync(string ids, Employee employee, Partner partner)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ids))
                {
                    return new GeneralResponse(false, "Mã cơ hội không được để trống.");
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
                    return new GeneralResponse(false, "Mã cơ hội không hợp lệ.");
                }

                var opportunitiesToDelete = await _appContext
                    .Opportunities.Where(o => idList.Contains(o.Id) && o.OwnerTaskExecuteId == employee.Id)
                    .ToListAsync();

                if (!opportunitiesToDelete.Any())
                {
                    throw new KeyNotFoundException("Không cơ hội nào được xoá.");
                }

                var opportunityIdStrings = opportunitiesToDelete.Select(o => o.Id.ToString()).ToList();

                await DeleteBulkOpportunitiesDetailsAsync(opportunityIdStrings);

                _appContext.Opportunities.RemoveRange(opportunitiesToDelete);
                await _appContext.SaveChangesAsync();

                return new GeneralResponse(true, "Xoá cơ hội thành công");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo cơ hội: {ex.Message}");
            }
        }

        public async Task<DataObjectResponse?> GenerateOpportunityCodeAsync(Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);

            var orderCode = await codeGenerator
            .GenerateNextCodeAsync<Opportunity>(prefix: "CH",
                codeSelector: c => c.OpportunityNo,
                filter: c => c.PartnerId == partner.Id);

            return new DataObjectResponse(true, "Tạo mã cơ hội thành công", orderCode);
        }

        public Task<List<Activity?>> GetAllActivitiesByOpportunityAsync(int opportunityId, Employee employee, Partner partner)
        {
            throw new NotImplementedException();
        }

        public Task<List<ContactDTO>> GetAllContactsAvailableByIdAsync(int opportunityId, Employee employee, Partner partner)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResponse<List<OptionalOpportunityDTO>>> GetAllOpportunitiesAsync(Employee employee, Partner partner, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Check null inputs
                if (employee == null)
                {
                    throw new ArgumentNullException(nameof(employee), "Employee null là toang đó nha! 😜");
                }
                if (partner == null)
                {
                    throw new ArgumentNullException(nameof(partner), "Partner null cũng không ổn đâu! 😎");
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10; // Default page size

                // Build query
                var query = _appContext.Opportunities
                    .Where(o => o.Partner.Id == partner.Id)
                    .AsNoTracking();

                if (!IsOwner)
                {
                    query = query.Where(o => o.OwnerTaskExecuteId == employee.Id);
                }

                var totalRecords = await query.CountAsync();

                var opportunities = await query
                    .OrderBy(o => o.Id) 
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to DTOs
                var opportunityDtos = _mapper.Map<List<OptionalOpportunityDTO>>(opportunities);

                // Return paged response
                return new PagedResponse<List<OptionalOpportunityDTO>>(
                    data: opportunityDtos ?? new List<OptionalOpportunityDTO>(),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    totalRecords: totalRecords
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Lấy danh sách opportunities fail: {ex.Message}", ex);
            }
        }

        public async Task<OpportunityDTO?> GetOpportunityByIdAsync(int id, Employee employee, Partner partner)
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

                var opportunity = await _appContext
                    .Opportunities.Where(o =>
                        o.Id == id
                        && (
                            o.Partner == partner && o.OwnerTaskExecuteId == employee.Id
                        // || o.OrderEmployees.Any(oe => oe.EmployeeId == employee.Id)
                        )
                    )
                    // .Include(o => o.OrderEmployees)
                    .FirstOrDefaultAsync();

                if (opportunity == null)
                {
                    throw new KeyNotFoundException(
                        $"Cơ hội của ID {id} không được tìm thấy trên nhân viên này."
                    );
                }

                var opportunityDetails = await _opportunityProductDetails
                    .Find(d => d.OpportunityId.Value.ToString() == id.ToString())
                    .ToListAsync();

                var opportunityDto = _mapper.Map<OpportunityDTO>(opportunity);
                opportunityDto.OpportunityProductDetails = opportunityDetails
                    .Select(d => new OpportunityProductDetails
                    {
                        Id = d.Id,
                        OpportunityId = d.OpportunityId,
                        OpportunityNo = d.OpportunityNo,
                        OpportunityName = d.OpportunityName,
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

                return opportunityDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thông tin đơn hàng: {ex.Message}");
            }
        }

        public Task<GeneralResponse?> UpdateFieldIdAsync(int id, UpdateOpportunityDTO opportunities, Employee employee, Partner partner)
        {
            throw new NotImplementedException();
        }

        public async Task<GeneralResponse?> UpdateOpportunityAsync(int id, UpdateOpportunityDTO opportunities, Employee employee, Partner partner)
        {
            var strategy = _appContext.Database.CreateExecutionStrategy();
            Console.WriteLine(
                $"Starting UpdateOrderAsync for Order ID: {id}, Employee ID: {employee?.Id}, Partner ID: {partner?.Id}"
            );
            var codeGenerator = new GenerateNextCode(_appDbContext);

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Transaction started.");

                    Console.WriteLine("Fetching existing opportunity from database...");
                    Opportunity? existingOpportunity = null;
                    existingOpportunity = await _appContext
                        .Opportunities
                        .FirstOrDefaultAsync(o =>
                            o.Id == id && o.OwnerTaskExecuteId == employee.Id
                                // || o.OrderEmployees.Any(oe =>
                                //     oe.EmployeeId == employee.Id && oe.AccessLevel == AccessLevel.Write
                                // )
                                && o.Partner == partner
                        );
                    if (existingOpportunity == null)
                    {
                        Console.WriteLine($"Opportunity with ID {id} not found.");
                        return new GeneralResponse(false, "Không tìm thấy cơ hội");
                    }

                    if (string.IsNullOrEmpty(existingOpportunity.OpportunityNo))
                    {
                        string originalCode = existingOpportunity.OpportunityNo;
                        bool exists = await _appDbContext.Opportunities.AnyAsync(c =>
             c.OpportunityNo == existingOpportunity.OpportunityNo &&
             c.PartnerId == partner.Id &&
             c.Id != id);
                        if (exists)
                        {
                            existingOpportunity.OpportunityNo = await codeGenerator.GenerateNextCodeAsync<Opportunity>("CH", c => c.OpportunityNo, c => c.PartnerId == partner.Id);
                            Console.WriteLine($"OpportunityNo '{originalCode}' already existed. Replaced with '{existingOpportunity.OpportunityNo}' for OpportunityNo ID {id}.");
                        }
                    }
                    Console.WriteLine(
                        $"Opportunity found: ID {existingOpportunity.Id}, OwnerId {existingOpportunity.OwnerTaskExecuteId}, Partner ID {existingOpportunity.Partner.Id}"
                    );
                    Console.WriteLine("Mapping opportunities to existingOpportunity...");
                    _mapper.Map(opportunities, existingOpportunity);

                    Console.WriteLine("Updating opportunity in database...");
                    _appContext.Opportunities.Update(existingOpportunity);
                    await _appContext.SaveChangesAsync();
                    Console.WriteLine("Order updated in SQL database successfully.");

                    var existingOpportunityDetails = await _opportunityProductDetails
                        .Find(d => d.OpportunityId == id)
                        .ToListAsync();
                    Console.WriteLine(
                        $"Found {existingOpportunityDetails.Count} existing opportunity details."
                    );
                    //  Prepare OpportunityDetails for MongoDB Update
                    var updatedOpportunityDetails = new List<ReplaceOneModel<OpportunityProductDetails>>();
                    var newOpportunityDetails = new List<OpportunityProductDetails>();
                    var detailIdsToKeep = new HashSet<string>();
                    Console.WriteLine("Processing opportunity details...");
                    foreach (var detailDto in opportunities.OpportunityProductDetails)
                    {
                        var detailId = string.IsNullOrEmpty(detailDto.Id)
                            ? ObjectId.GenerateNewId().ToString()
                            : detailDto.Id;
                        var existingDetail = existingOpportunityDetails.FirstOrDefault(d =>
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
                            updatedOpportunityDetails.Add(
                                new ReplaceOneModel<OpportunityProductDetails>(
                                    Builders<OpportunityProductDetails>.Filter.Eq(d => d.Id, detailId),
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
                            var newDetail = new OpportunityProductDetails
                            {
                                Id = detailId,
                                OpportunityId = existingOpportunity.Id,
                                OpportunityNo = existingOpportunity.OpportunityNo,
                                OpportunityName = existingOpportunity.OpportunityName,
                                Avatar = detailDto.Avatar,
                                PartnerId = partner.Id,
                                PartnerName = existingOpportunity.Partner.Name,
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
                            newOpportunityDetails.Add(newDetail);
                        }

                        detailIdsToKeep.Add(detailId);
                    }

                    var detailIdsToDelete = existingOpportunityDetails
                        .Select(d => d.Id)
                        .Except(detailIdsToKeep)
                        .ToList();
                    if (detailIdsToDelete.Any())
                    {
                        Console.WriteLine(
                            $"Deleting {detailIdsToDelete.Count} obsolete opportunity details..."
                        );
                        await _opportunityProductDetails.DeleteManyAsync(d =>
                            detailIdsToDelete.Contains(d.Id)
                        );
                        Console.WriteLine("Obsolete opportunity details deleted.");
                    }

                    // Perform Bulk Update in MongoDB
                    if (updatedOpportunityDetails.Any())
                    {
                        Console.WriteLine(
                            $"Performing bulk update for {updatedOpportunityDetails.Count} opportunity details..."
                        );
                        await _opportunityProductDetails.BulkWriteAsync(updatedOpportunityDetails);
                        Console.WriteLine("Bulk update completed.");
                    }
                    if (newOpportunityDetails.Any())
                    {
                        Console.WriteLine(
                            $"Inserting {newOpportunityDetails.Count} new opportunity details..."
                        );
                        await _opportunityProductDetails.InsertManyAsync(newOpportunityDetails);
                        Console.WriteLine("New opportunity details inserted.");
                    }
                    Console.WriteLine("Committing transaction...");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, "Cập nhật cơ hội thành công");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(
                        false,
                        $"Lỗi khi lấy thông tin cơ hội: {ex.Message}"
                    );
                }
            });
        }

        public async Task<List<ContactDTO>> GetAllContactsByIdAsync(int id, Partner partner)
        {
            if (id == null)
            {
                throw new ArgumentException("ID khách hàng không được để trống !");
            }
            if (partner == null)
            {
                throw new ArgumentException("Thông tin tổ chức không được bỏ trống");
            }

            var result = await _appDbContext.Contacts
            .Where(c => c.OpportunityContacts.Any(cc => cc.OpportunityId == id && cc.PartnerId == partner.Id))
            .Select(c => new ContactDTO
            {
                Id = c.Id,
                ContactCode = c.ContactCode,
                ContactName = c.ContactName,
                FullName = $"{c.LastName} {c.FirstName}",
                SalutationID = c.SalutationID,
                OfficeEmail = c.OfficeEmail,
                TitleID = c.TitleID,
                Mobile = c.Mobile,
                Email = c.Email,
            }).ToListAsync();
            return result;
        }

        public Task<List<ContactDTO>> GetAllContactAvailableById(int id, Partner partner)
        {
            throw new NotImplementedException();
        }

        public Task<List<Activity>> GetAllActivitiesByIdAsync(int id, Partner partner)
        {
            throw new NotImplementedException();
        }

        public Task<List<OptionalOrderDTO>> GetAllOrdersByIdAsync(int id, Partner partner)
        {
            throw new NotImplementedException();
        }

        public async Task<GeneralResponse?> BulkAddContactsIntoId(List<int> contactIds, int customerId, Employee employee, Partner partner)
        {
            if (contactIds == null || !contactIds.Any())
                return new GeneralResponse(false, "Danh sách liên hệ không được để trống!");

            var opportunity = await GetOpportunityByIdAsync(customerId, employee, partner);

            if (opportunity == null)
                return new GeneralResponse(false, "Không tìm thấy cơ hội!");

            var contacts = await _appDbContext.Contacts
                .Where(c => contactIds.Contains(c.Id) && c.PartnerId == partner.Id)
                .ToListAsync();

            if (!contacts.Any())
                return new GeneralResponse(false, "Không tìm thấy liên hệ !");

            var existingContactIds = opportunity.OpportunityContacts.Select(oc => oc.ContactId).ToHashSet();
            var newOpportunityContacts = contacts
                .Where(c => !existingContactIds.Contains(c.Id))
                .Select(c => new OpportunityContacts
                {
                    OpportunityId = opportunity.Id,
                    ContactId = c.Id,
                    PartnerId = partner.Id
                })
                .ToList();

            if (!newOpportunityContacts.Any())
                return new GeneralResponse(false, "Liên hệ đã liên kết với cơ hội !");

            _appDbContext.OpportunityContacts.AddRange(newOpportunityContacts);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Thêm liên hệ vào thông tin cơ hội thành công!");
        }

        public async Task<GeneralResponse?> RemoveContactFromId(int id, int contactId, Partner partner)
        {
            if (id == null)
            {
                return new GeneralResponse(false, "ID khách hàng không được để trống !");
            }
            if (contactId == null)
            {
                return new GeneralResponse(false, "Thông tin liên không được bỏ trống");
            }
            if (partner == null)
            {
                return new GeneralResponse(false, "Thông tin tổ chức không được bỏ trống");
            }
            var opportunityContact = await _appDbContext.OpportunityContacts
         .FirstOrDefaultAsync(cc => cc.OpportunityId == id
                                 && cc.ContactId == contactId
                                 && cc.PartnerId == partner.Id);
            if (opportunityContact == null)
            {
                return new GeneralResponse(false, "Không tìm thấy bản ghi cần xóa!");
            }
            _appDbContext.OpportunityContacts.Remove(opportunityContact);

            await _appDbContext.SaveChangesAsync();


            return new GeneralResponse(true, "Xóa thành công!");
        }

        public async Task<GeneralResponse?> UnassignActivityFromId(int id, int activityId, Partner partner)
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
                        Console.WriteLine($"No Customer found for ID {id}.");
                        return new GeneralResponse(true, $"ID {id} không liên kết với cơ hội nào.");
                    }

                    var activity = await _appDbContext.Activities
                        .FirstOrDefaultAsync(a => a.Id == activityId && a.OpportunityId == id && a.PartnerId == partner.Id);

                    if (activity == null)
                    {
                        Console.WriteLine($"No Activity found for ID {activityId}.");
                        return new GeneralResponse(true, $"ID {activityId} không liên kết với hoạt động nào.");
                    }

                    activity.OpportunityId = null;
                    _appDbContext.Activities.Update(activity);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine("Activity removed successfully.");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Đã xóa hoạt động khỏi cơ hội ID {id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(false, $"Lỗi khi xóa hoạt động khỏi cơ hội ID {id}: {ex.Message}");
                }
            });
        }

        public async Task<GeneralResponse?> UnassignOrderFromId(int id, int orderId, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Execution strategy started.");
                    var opportunity = await _appDbContext.Opportunities
                        .FirstOrDefaultAsync(c => c.Id == id && c.Partner.Id == partner.Id);

                    if (opportunity == null)
                    {
                        Console.WriteLine($"No Customer found for ID {id}.");
                        return new GeneralResponse(true, $"ID {id} không liên kết với cơ hội nào.");
                    }

                    var order = await _appDbContext.Orders
                        .FirstOrDefaultAsync(o => o.Id == orderId && o.OpportunityId == id && o.Partner.Id == partner.Id);

                    if (order == null)
                    {
                        Console.WriteLine($"No Order found for ID {orderId}.");
                        return new GeneralResponse(true, $"ID {orderId} không liên kết với đơn hàng nào.");
                    }
                    order.OpportunityId = null;
                    _appDbContext.Orders.Update(order);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine("Order removed successfully.");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Đã xóa đơn hàng khỏi cơ hội ID {id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(false, $"Lỗi khi xóa đơn hàng khỏi cơ hội ID {id}: {ex.Message}");
                }
            });
        }

        public async Task<GeneralResponse?> UnassignQuoteFromId(int id, int quoteId, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Execution strategy started.");
                    var opportunity = await _appDbContext.Opportunities
                        .FirstOrDefaultAsync(c => c.Id == id && c.Partner.Id == partner.Id);

                    if (opportunity == null)
                    {
                        Console.WriteLine($"No Customer found for ID {id}.");
                        return new GeneralResponse(true, $"ID {id} không liên kết với cơ hội nào.");
                    }

                    var quote = await _appDbContext.Quotes
                        .FirstOrDefaultAsync(o => o.Id == quoteId && o.OpportunityID == id && o.Partner.Id == partner.Id);

                    if (quote == null)
                    {
                        Console.WriteLine($"No Quote found for ID {quoteId}.");
                        return new GeneralResponse(true, $"ID {quoteId} không liên kết với cơ hội nào.");
                    }
                    quote.OpportunityID = null;
                    _appDbContext.Quotes.Update(quote);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine("Quote removed successfully.");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Đã xóa báo giá khỏi cơ hội ID {id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(false, $"Lỗi khi xóa đơn hàng khỏi cơ hội ID {id}: {ex.Message}");
                }
            });
        }

        public Task<List<QuoteDTO>> GetAllQuotesByIdAsync(int id, Partner partner)
        {
            throw new NotImplementedException();

        }
    }
}