using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.Enums;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IActivityService
    {
        public Task<List<Activity>> GetAllActivityAsync(Employee employee, Partner partner);
    }
}