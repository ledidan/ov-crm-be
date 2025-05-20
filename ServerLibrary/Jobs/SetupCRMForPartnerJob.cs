using Quartz;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Jobs
{
    public class SetupCRMForPartnerJob : IJob
    {
        private readonly ICRMService _crmSetupService;

        public SetupCRMForPartnerJob(ICRMService crmSetupService)
        {
            _crmSetupService = crmSetupService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var partnerId = context.MergedJobDataMap.GetInt("PartnerId");
            var userId = context.MergedJobDataMap.GetInt("UserId");
            var employeeId = context.MergedJobDataMap.GetInt("EmployeeId");

            Console.WriteLine($"[Quartz] Setup CRM cho partnerId: {partnerId}, userId: {userId}");

            var result = await _crmSetupService.FirstSetupCRMPartnerAsync(partnerId, userId, employeeId);

            if (!result.Flag)
            {   
                Console.WriteLine($"[Quartz] CRM setup failed: {result.Message}");
            }
            else
            {
                Console.WriteLine($"[Quartz] CRM setup done for partnerId: {partnerId}");
            }
        }
    }
}
