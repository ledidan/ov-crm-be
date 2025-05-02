using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class CustomerCareService : ICustomerCareService
    {

        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;

        public CustomerCareService(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task<GeneralResponse> BulkAddSupportTicketFromId(List<int> supportTicketIds, int id, Employee employee, Partner partner)
        {
            if (supportTicketIds == null || !supportTicketIds.Any())
                return new GeneralResponse(false, "Danh sách thẻ tư vấn không được để trống!");

            var customerCare = await GetCustomerCareTicketById(id, partner);

            if (customerCare == null)
                return new GeneralResponse(false, "Không tìm thấy thẻ chăm sóc!");

            var supportTickets = await _appDbContext.SupportTickets
                .Where(c => supportTicketIds.Contains(c.Id) && c.PartnerId == partner.Id)
                .ToListAsync();

            if (!supportTickets.Any())
                return new GeneralResponse(false, "Không tìm thấy thẻ tư vấn!");

            // var existingTicketIds = customerCare..Select(oc => oc.ContactId).ToHashSet();
            // var newCustomerContacts = contacts
            //     .Where(c => !existingContactIds.Contains(c.Id))
            //     .Select(c => new CustomerContacts
            //     {
            //         CustomerId = customer.Id,
            //         ContactId = c.Id,
            //         PartnerId = partner.Id
            //     })
            //     .ToList();

            // if (!newCustomerContacts.Any())
            //     return new GeneralResponse(false, "Liên hệ đã liên kết với khách hàng !");

            // _appDbContext.CustomerContacts.AddRange(newCustomerContacts);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Thêm liên hệ vào thông tin khách hàng thành công!");
        }

        public async Task<DataObjectResponse?> CheckCustomerCareCodeAsync(string code, Employee employee, Partner partner)
        {
            var customerTicketDetail = await GetCustomerTicketByCode(code, partner);

            if (customerTicketDetail == null)
            {
                return new DataObjectResponse(true, "Mã chăm sóc có thể sử dụng", null);
            }
            else
            {
                return new DataObjectResponse(false, "Mã chăm sóc đã tồn tại", new
                {
                    customerTicketDetail.AccountName,
                    customerTicketDetail.CustomerCareNumber,
                    customerTicketDetail.Id
                });
            }
        }

        public async Task<GeneralResponse> CreateCustomerCareTicket(CustomerCareTicketDTO customerCareTicketDTO, Employee employee, Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);
            var checkCodeExisted = await CheckCustomerCareCodeAsync(customerCareTicketDTO.CustomerCareNumber, employee, partner);
            if (checkCodeExisted != null && !checkCodeExisted.Flag)
                return new GeneralResponse(false, "Mã tư vấn đã tồn tại !");

            if (customerCareTicketDTO == null)
            {
                return new GeneralResponse(false, "Dữ liệu không hợp lệ");
            }
            if (employee == null)
            {
                return new GeneralResponse(false, "Nhân viên không tồn tại");
            }
            if (partner == null)
            {
                return new GeneralResponse(false, "Đối tác không tồn tại");
            }
            if (string.IsNullOrEmpty(customerCareTicketDTO.CustomerCareNumber))
            {
                customerCareTicketDTO.CustomerCareNumber = await codeGenerator.GenerateNextCodeAsync<CustomerCare>(
            "CS",
            c => c.CustomerCareNumber,
            c => c.PartnerId == partner.Id
        );
            }
            var customerCareTicket = _mapper.Map<CustomerCare>(customerCareTicketDTO);
            customerCareTicket.Partner = partner;
            customerCareTicket.PartnerName = partner.Name;
            customerCareTicket.CreatedBy = employee.Id;
            customerCareTicket.CreatedByName = employee.FullName;


            await _appDbContext.CustomerCares.AddAsync(customerCareTicket);
            await _appDbContext.SaveChangesAsync();
            return new GeneralResponse(true, "Tạo yêu cầu hỗ trợ chăm sóc khách hàng thành công");
        }

        public async Task<GeneralResponse?> DeleteBulkCustomerTicketsAsync(string ids, Employee employee, Partner partner)
        {
            if (partner == null)
                return new GeneralResponse(false, "Đối tác không tồn tại");

            var idList = ids.Split(',')
                .Select(id => int.TryParse(id.Trim(), out int parsedId) ? parsedId : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            if (!idList.Any())
                return new GeneralResponse(false, "Không có ID hợp lệ nào được cung cấp");

            var customerCares = await _appDbContext
                .CustomerCares.Where(p => idList.Contains(p.Id) && p.Partner.Id == partner.Id)
                .ToListAsync();

            if (!customerCares.Any())
                return new GeneralResponse(false, "Không tìm thấy yêu cầu hỗ trợ chăm sóc khách hàng nào với ID đã cho");

            _appDbContext.CustomerCares.RemoveRange(customerCares);
            await _appDbContext.SaveChangesAsync();
            return new GeneralResponse(true, $"{customerCares.Count} yêu cầu hỗ trợ chăm sóc khách hàng đã được xóa thành công");
        }

        public async Task<GeneralResponse> DeleteCustomerCareTicket(int id, Partner partner)
        {
            var customerCareTicket = await _appDbContext.CustomerCares.Where(c => c.Partner.Id == partner.Id).FirstOrDefaultAsync(c => c.Id == id);
            if (customerCareTicket == null)
            {
                return new GeneralResponse(false, "Yêu cầu hỗ trợ chăm sóc khách hàng không tồn tại");
            }
            _appDbContext.CustomerCares.Remove(customerCareTicket);
            await _appDbContext.SaveChangesAsync();
            return new GeneralResponse(true, "Xóa yêu cầu hỗ trợ chăm sóc khách hàng thành công");
        }

        public async Task<DataObjectResponse?> GenerateCustomerCareCodeAsync(Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);

            var customerCareCode = await codeGenerator
            .GenerateNextCodeAsync<CustomerCare>(prefix: "CS",
                codeSelector: c => c.CustomerCareNumber,
                filter: c => c.PartnerId == partner.Id);

            return new DataObjectResponse(true, "Tạo mã chăm sóc thành công", customerCareCode);
        }

        public async Task<List<ActivityDTO>> GetAllActivitiesByCustomerCareTickets(int id, Partner partner)
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
                c => c.CustomerCareTicketID == id
                && c.PartnerId == partner.Id
                && c.StatusID != doneStatus
                ).ToListAsync();
            var activitiesDtos = _mapper.Map<List<ActivityDTO>>(activities);
            return activitiesDtos;
        }

        public async Task<List<ActivityDTO>> GetAllActivitiesDoneByCustomerCareTickets(int id, Partner partner)
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
            var activities = await _appDbContext.Activities.Where(c => c.CustomerCareTicketID == id
            && c.PartnerId == partner.Id
            && c.StatusID == doneStatus
            ).ToListAsync();
            if (activities.Count == 0)
            {
                return new List<ActivityDTO>();
            }
            return _mapper.Map<List<ActivityDTO>>(activities);
        }

        public async Task<PagedResponse<List<CustomerCareTicketDTO>>> GetAllCustomerCareTickets(Partner partner, int pageNumber, int pageSize)
        {
            // Validate input
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10; // Default page size

            // Query with pagination
            var query = _appDbContext.CustomerCares.AsQueryable();
            // Optional: Filter by partner if needed
            if (partner != null)
            {
                query = query.Where(cc => cc.PartnerId == partner.Id);
            }
            var totalRecords = await query.CountAsync();

            var customerCareTickets = await query
                .OrderBy(cc => cc.Id) // Optional: Add sorting, adjust as needed
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to DTOs
            var customerCareTicketDTOs = _mapper.Map<List<CustomerCareTicketDTO>>(customerCareTickets);

            // Return paged response
            return new PagedResponse<List<CustomerCareTicketDTO>>(
                data: customerCareTicketDTOs,
                pageNumber: pageNumber,
                pageSize: pageSize,
                totalRecords: totalRecords
            );
        }

        public Task<List<SupportTicketDTO>> GetAllSupportTicketsByCustomerCareTickets(int id, Partner partner)
        {
            throw new NotImplementedException();
        }

        public async Task<CustomerCareTicketDTO> GetCustomerCareTicketById(int id, Partner partner)
        {
            var customerCareTicket = await _appDbContext.CustomerCares.Where(c => c.Partner.Id == partner.Id).FirstOrDefaultAsync(c => c.Id == id);
            if (customerCareTicket == null)
            {
                throw new Exception("Yêu cầu hỗ trợ chăm sóc khách hàng không tồn tại");
            }
            return _mapper.Map<CustomerCareTicketDTO>(customerCareTicket);
        }

        public async Task<GeneralResponse> UnassignActivityFromId(int id, int activityId, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Transaction started.");
                    var activity = await _appDbContext.Activities.FirstOrDefaultAsync(a =>
                        a.Id == activityId && a.CustomerCareTicketID == id && a.PartnerId == partner.Id
                    );
                    if (activity == null)
                    {
                        Console.WriteLine($"No activity found for ID {activityId}.");
                        return new GeneralResponse(
                            false,
                            $"Không tìm thấy hoạt động ID {activityId}."
                        );
                    }

                    activity.CustomerCareTicketID = null;
                    _appDbContext.Activities.Update(activity);
                    await _appDbContext.SaveChangesAsync();
                    Console.WriteLine("Activity unassigned from order successfully.");
                    await transaction.CommitAsync();
                    return new GeneralResponse(
                        true,
                        $"Đã gỡ bỏ liên kết hoạt động với thẻ chăm sóc ID {id}."
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new GeneralResponse(
                        false,
                        $"Không thể gỡ bỏ liên kết hoạt động với thẻ chăm sóc: {ex.Message}"
                    );
                }
            });
        }

        public Task<GeneralResponse> UnassignSupportTicketFromId(int id, int supportTicketId, Partner partner)
        {
            throw new NotImplementedException();
        }

        public async Task<GeneralResponse> UpdateCustomerCareTicket(int id,
        CustomerCareTicketDTO customerCareTicketDTO, Employee employee, Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);

            var customerCareTicket = await _appDbContext.CustomerCares.Where(c => c.Partner.Id == partner.Id).FirstOrDefaultAsync(c => c.Id == id);
            if (customerCareTicket == null)
            {
                return new GeneralResponse(false, "Yêu cầu hỗ trợ chăm sóc khách hàng không tồn tại");
            }

            if (string.IsNullOrEmpty(customerCareTicketDTO.CustomerCareNumber))
            {
                string originalCode = customerCareTicketDTO.CustomerCareNumber;
                bool exists = await _appDbContext.CustomerCares.AnyAsync(c =>
     c.CustomerCareNumber == customerCareTicketDTO.CustomerCareNumber &&
     c.PartnerId == partner.Id &&
     c.Id != id);
                if (exists)
                {
                    customerCareTicketDTO.CustomerCareNumber = await codeGenerator.GenerateNextCodeAsync<CustomerCare>("CS", c => c.CustomerCareNumber, c => c.PartnerId == partner.Id);
                    Console.WriteLine($"CustomerCareNumber '{originalCode}' already existed. Replaced with '{customerCareTicketDTO.CustomerCareNumber}' for CustomerCareNumber ID {id}.");
                }
            }
            _mapper.Map(customerCareTicketDTO, customerCareTicket);
            customerCareTicket.Partner = partner;
            await _appDbContext.SaveChangesAsync();
            return new GeneralResponse(true, "Cập nhật yêu cầu hỗ trợ chăm sóc khách hàng thành công");
        }


        private async Task<CustomerCareTicketDTO> GetCustomerTicketByCode(string code, Partner partner)
        {
            var existingCustomerTicket = await _appDbContext.CustomerCares
                .FirstOrDefaultAsync(c => c.CustomerCareNumber == code && c.PartnerId == partner.Id);
            if (existingCustomerTicket == null)
                return null;

            return new CustomerCareTicketDTO
            {
                Id = existingCustomerTicket.Id,
                CustomerCareNumber = existingCustomerTicket.CustomerCareNumber,
                AccountName = existingCustomerTicket.AccountName,
            };
        }
    }
}