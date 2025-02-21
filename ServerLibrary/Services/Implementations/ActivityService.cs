


using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class ActivityService : IActivityService
    {
        private readonly AppDbContext _appDbContext;

        private readonly IMapper _mapper;

        public ActivityService(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task<List<Activity>> GetAllActivityAsync(Employee employee, Partner partner)
        {
            try
            {
                var activities = await _appDbContext.Activities
                                  .Where(a =>
                                      a.PartnerId == partner.Id && a.TaskOwnerId == employee.Id ||
                                      a.ActivityEmployees.Any(oe => oe.EmployeeId == employee.Id))
                                      .Include(oce => oce.ActivityEmployees)
                                  .ToListAsync();


                if (activities.Any())
                {
                    return new List<Activity>();
                }
                // var activitiesDto = _mapper.Map<ActivityDTO>(activities);
                return activities;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy dữ liệu tất cả hoạt động: {ex.Message}");

            }

        }
    }
}