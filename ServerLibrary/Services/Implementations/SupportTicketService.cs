



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
    public class SupportTicketService : ISupportTicketService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        public SupportTicketService(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task<DataObjectResponse?> CheckSupportCodeAsync(string code, Employee employee, Partner partner)
        {
            var ticketDetail = await GetSupportByCode(code, partner);

            if (ticketDetail == null)
            {
                return new DataObjectResponse(true, "Mã hỗ trợ có thể sử dụng", null);
            }
            else
            {
                return new DataObjectResponse(false, "Mã hỗ trợ đã tồn tại", new
                {
                    ticketDetail.TicketNumber,
                    ticketDetail.AccountName,
                    ticketDetail.Id
                });
            }
        }

        public async Task<GeneralResponse> CreateSupportTicket(SupportTicketDTO supportTicketDTO, Employee employee, Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);
            var supportTicket = _mapper.Map<SupportTicket>(supportTicketDTO);
            supportTicket.Partner = partner;
            supportTicket.CreatedBy = employee.Id;
            supportTicket.CreatedByName = employee.FullName;
            var checkCodeExisted = await CheckSupportCodeAsync(supportTicket.TicketNumber, employee, partner);

            if (checkCodeExisted != null && !checkCodeExisted.Flag)
                return new GeneralResponse(false, "Mã hỗ trợ đã tồn tại !");

            if (string.IsNullOrEmpty(supportTicketDTO.TicketNumber))
            {
                supportTicket.TicketNumber = await codeGenerator.GenerateNextCodeAsync<SupportTicket>(
            "LH",
            c => c.TicketNumber,
            c => c.PartnerId == partner.Id
        );
            }
            await _appDbContext.SupportTickets.AddAsync(supportTicket);
            await _appDbContext.SaveChangesAsync();
            return new GeneralResponse(true, "Tạo yêu cầu hỗ trợ thành công");
        }

        public async Task<GeneralResponse?> DeleteBulkTicketsAsync(string ids, Employee employee, Partner partner)
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

            var tickets = await _appDbContext
                .SupportTickets.Where(p => idList.Contains(p.Id) && p.Partner.Id == partner.Id)
                .ToListAsync();

            if (!tickets.Any())
                return new GeneralResponse(false, "Không tìm thấy yêu cầu tư vấn với ID đã cho");

            _appDbContext.SupportTickets.RemoveRange(tickets);
            await _appDbContext.SaveChangesAsync();
            return new GeneralResponse(true, $"{tickets.Count} yêu cầu hỗ trợ đã được xóa thành công");
        }

        public async Task<GeneralResponse> DeleteSupportTicket(int id, Partner partner)
        {
            var supportTicket = await _appDbContext.SupportTickets.Where(s => s.Partner.Id == partner.Id).FirstOrDefaultAsync(s => s.Id == id);
            if (supportTicket == null)
            {
                return new GeneralResponse(false, "Yêu cầu hỗ trợ không tồn tại");
            }
            _appDbContext.SupportTickets.Remove(supportTicket);
            await _appDbContext.SaveChangesAsync();
            return new GeneralResponse(true, "Xóa yêu cầu hỗ trợ thành công");
        }

        private async Task<SupportTicketDTO> GetSupportByCode(string code, Partner partner)
        {
            var existingSupport = await _appDbContext.SupportTickets
                .FirstOrDefaultAsync(c => c.TicketNumber == code && c.PartnerId == partner.Id);
            if (existingSupport == null)
                return null;

            return new SupportTicketDTO
            {
                Id = existingSupport.Id,
                TicketNumber = existingSupport.TicketNumber,
                AccountName = existingSupport.AccountName,
            };
        }

        public async Task<DataObjectResponse?> GenerateSupportCodeAsync(Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);

            var supportCode = await codeGenerator
            .GenerateNextCodeAsync<SupportTicket>(prefix: "HT",
                codeSelector: c => c.TicketNumber,
                filter: c => c.PartnerId == partner.Id);

            return new DataObjectResponse(true, "Tạo mã hỗ trợ thành công", supportCode);
        }

        public async Task<List<ActivityDTO>> GetAllActivitiesBySupportTickets(int id, Partner partner)
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
                c => c.SupportTicketID == id
                && c.PartnerId == partner.Id
                && c.StatusID != doneStatus
                ).ToListAsync();
            var activitiesDtos = _mapper.Map<List<ActivityDTO>>(activities);
            return activitiesDtos;
        }

        public async Task<List<ActivityDTO>> GetAllActivitiesDoneBySupportTickets(int id, Partner partner)
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
            var activities = await _appDbContext.Activities.Where(c => c.SupportTicketID == id
            && c.PartnerId == partner.Id
            && c.StatusID == doneStatus
            ).ToListAsync();
            if (activities.Count == 0)
            {
                return new List<ActivityDTO>();
            }
            return _mapper.Map<List<ActivityDTO>>(activities);
        }

        public async Task<List<SupportTicketDTO>> GetAllSupportTickets(Partner partner)
        {
            var supportTickets = await _appDbContext.SupportTickets.Where(s => s.Partner.Id == partner.Id).ToListAsync();
            var supportTicketDTOs = _mapper.Map<List<SupportTicketDTO>>(supportTickets);
            return supportTicketDTOs;
        }

        public async Task<SupportTicketDTO> GetSupportTicketById(int id, Partner partner)
        {
            var supportTicket = await _appDbContext.SupportTickets.Where(s => s.Partner.Id == partner.Id).FirstOrDefaultAsync(s => s.Id == id);
            if (supportTicket == null)
            {
                throw new Exception("Yêu cầu hỗ trợ không tồn tại");
            }
            return _mapper.Map<SupportTicketDTO>(supportTicket);
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
                        a.Id == activityId && a.SupportTicketID == id && a.PartnerId == partner.Id
                    );
                    if (activity == null)
                    {
                        Console.WriteLine($"No activity found for ID {activityId}.");
                        return new GeneralResponse(
                            false,
                            $"Không tìm thấy hoạt động ID {activityId}."
                        );
                    }

                    activity.SupportTicketID = null;
                    _appDbContext.Activities.Update(activity);
                    await _appDbContext.SaveChangesAsync();
                    Console.WriteLine("Activity unassigned from order successfully.");
                    await transaction.CommitAsync();
                    return new GeneralResponse(
                        true,
                        $"Đã gỡ bỏ liên kết hoạt động với thẻ tư vấn ID {id}."
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new GeneralResponse(
                        false,
                        $"Không thể gỡ bỏ liên kết hoạt động với thẻ tư vấn: {ex.Message}"
                    );
                }
            });
        }

        public async Task<GeneralResponse> UpdateSupportTicket(int id, SupportTicketDTO supportTicketDTO, Employee employee, Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);
            var supportTicket = await _appDbContext.SupportTickets.Where(s => s.Partner.Id == partner.Id).FirstOrDefaultAsync(s => s.Id == id);
            if (supportTicket == null)
            {
                return new GeneralResponse(false, "Yêu cầu hỗ trợ không tồn tại");
            }
            if (string.IsNullOrEmpty(supportTicket.TicketNumber))
            {
                string originalCode = supportTicket.TicketNumber;
                bool exists = await _appDbContext.SupportTickets.AnyAsync(c =>
     c.TicketNumber == supportTicket.TicketNumber &&
     c.PartnerId == partner.Id &&
     c.Id != id);
                if (exists)
                {
                    supportTicket.TicketNumber = await codeGenerator.GenerateNextCodeAsync<SupportTicket>("HT", c => c.TicketNumber, c => c.PartnerId == partner.Id);
                    Console.WriteLine($"TicketNumber '{originalCode}' already existed. Replaced with '{supportTicketDTO.TicketNumber}' for TicketNumber ID {id}.");
                }
            }
            supportTicketDTO.ModifiedBy = employee.Id;
            supportTicketDTO.ModifiedByName = employee.FullName;
            _mapper.Map(supportTicketDTO, supportTicket);
            await _appDbContext.SaveChangesAsync();
            return new GeneralResponse(true, "Cập nhật yêu cầu hỗ trợ thành công");
        }

    }
}