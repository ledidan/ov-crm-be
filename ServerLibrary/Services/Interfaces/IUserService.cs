using Data.DTOs;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IUserService
    {
        Task<GeneralResponse> CreateAsync(Register user);
        Task<LoginResponse> SignInAsync(Login user);
        Task<LoginResponse> RefreshTokenAsync(RefreshToken token);
    }
}
