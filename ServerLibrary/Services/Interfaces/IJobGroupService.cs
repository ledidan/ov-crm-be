



using Data.Entities;

namespace ServerLibrary.Services.Interfaces {
    public interface IJobGroupService 
    {
        Task<List<JobPositionGroup>> CreateDefaultJobPosition(Partner partner);
        Task<List<JobTitleGroup>> CreateDefaultJobTitle(Partner partner);
    }
}