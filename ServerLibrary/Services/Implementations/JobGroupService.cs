





using Data.Entities;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{

    public class JobGroupService : IJobGroupService
    {

        private readonly AppDbContext _appDbContext;

        public JobGroupService(AppDbContext appDbContext)
        {

            _appDbContext = appDbContext;
        }

        public async Task<List<JobPositionGroup>> CreateDefaultJobPosition(Partner partner)
        {
            if (partner == null) throw new ArgumentNullException(nameof(partner));

            bool existingJobPositions = await _appDbContext.JobPositionGroups.AnyAsync(jpg => jpg.PartnerId == partner.Id);
            if (existingJobPositions)
                return new List<JobPositionGroup>();

            var jobPositionGroups = Constants.DefaultJobPositionGroups.JobPositionGroups
                .Select(jpg => new JobPositionGroup
                {
                    JobPositionGroupCode = jpg.JobPositionGroupCode,
                    JobPositionGroupName = jpg.JobPositionGroupName,
                    PartnerId = partner.Id
                }).ToList();

            await _appDbContext.JobPositionGroups.AddRangeAsync(jobPositionGroups);
            await _appDbContext.SaveChangesAsync();
            return jobPositionGroups;
        }

        public async Task<List<JobTitleGroup>> CreateDefaultJobTitle(Partner partner)
        {
            if (partner == null) throw new ArgumentNullException(nameof(partner));

            bool existingJobTitles = await _appDbContext.JobTitleGroups.AnyAsync(jtg => jtg.PartnerId == partner.Id);
            if (existingJobTitles)
                return new List<JobTitleGroup>();

            var jobTitleGroups = Constants.DefaultJobTitleGroups.JobTitleGroups
                .Select(jtg => new JobTitleGroup
                {
                    JobTitleGroupCode = jtg.JobTitleGroupCode,
                    JobTitleGroupName = jtg.JobTitleGroupName,
                    PartnerId = partner.Id
                }).ToList();

            await _appDbContext.JobTitleGroups.AddRangeAsync(jobTitleGroups);
            await _appDbContext.SaveChangesAsync();
            return jobTitleGroups;
        }
    }
}