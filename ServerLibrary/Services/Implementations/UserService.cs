using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ServerLibrary.Services.Implementations
{
    public class UserService(IOptions<JwtSection> config, AppDbContext appDbContext) : IUserService
    {
        public async Task<GeneralResponse> CreateAsync(Register user)
        {
            if (user == null) return new GeneralResponse(false, "Model is empty");

            var checkingUser = await FindUserByEmail(user.Email);
            if (checkingUser != null) return new GeneralResponse(false, "User already exist");

            //add user
            var applicationUser = await AddToDatabase(new ApplicationUser()
            {
                Email = user.Email,
                Fullname = user.Fullname,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password)
            });

            //check, create and assign role
            var checkingRole = await CheckSystemRole(user.Role!);
            if (checkingRole == null) return new GeneralResponse(false, "Role not found");

            await AddToDatabase(new UserRole() { Role = checkingRole, User = applicationUser });

            return new GeneralResponse(true, "User created");
        }

        private async Task<SystemRole?> CheckSystemRole(string role)
        {
            return await appDbContext.SystemRoles.FirstOrDefaultAsync(r => r.Name!.ToLower().Equals(role.ToLower()));
        }

        private async Task<T> AddToDatabase<T>(T model)
        {
            var result = appDbContext.Add(model!);
            await appDbContext.SaveChangesAsync();
            return (T)result.Entity;
        }

        private async Task<ApplicationUser?> FindUserByEmail(string? email)
        {
            return await appDbContext.ApplicationUsers.FirstOrDefaultAsync(user => user.Email!.ToLower().Equals(email!.ToLower()));
        }

        private async Task<UserRole?> FindUserRole(int userId)
        {
            return await appDbContext.UserRoles.Include(_ => _.Role).FirstOrDefaultAsync(_ => _.User.Id == userId);
        }

        private async Task<SystemRole?> FindSystemRole(int roleId)
        {
            return await appDbContext.SystemRoles.FirstOrDefaultAsync(_ => _.Id == roleId);
        }

        public async Task<LoginResponse> SingInAsync(Login user)
        {
            if (user == null) return new LoginResponse(false, "Model is empty");

            var applicationUser = await FindUserByEmail(user.Email);
            if (applicationUser == null) return new LoginResponse(false, "User not found");

            //verify
            if (!BCrypt.Net.BCrypt.Verify(user.Password, applicationUser.Password))
                return new LoginResponse(false, "Email/Password not valid");

            var userRole = await FindUserRole(applicationUser.Id);
            if (userRole == null) return new LoginResponse(false, "User role not found");
            var systemRole = await FindSystemRole(userRole.Role.Id);

            string jwtToken = GenerateToken(applicationUser, systemRole!.Name);
            string refreshToken = GenerateRefreshToken();

            //save Refresh token
            var findingRefreshToken = await FindRefreshTokenByUserId(applicationUser.Id);
            if (findingRefreshToken != null)
            {
                findingRefreshToken!.Token = refreshToken;
                await appDbContext.SaveChangesAsync();
            }
            else
            {
                await AddToDatabase(new RefreshTokenInfo() { Token = refreshToken, UserId = applicationUser.Id });
            }    
            return new LoginResponse(true, "Login sucessfully", jwtToken, refreshToken);
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        private string GenerateToken(ApplicationUser applicationUser, string? role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            Claim[] userClaims;
            if (role != Constants.Role.SysAdmin)
            {
                var partnerUser = appDbContext.PartnerUsers.Include(_ => _.Partner).FirstOrDefault(x => x.User.Id == applicationUser.Id);
                userClaims = new[] 
                {
                    new Claim(ClaimTypes.NameIdentifier, applicationUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, applicationUser.Fullname!),
                    new Claim(ClaimTypes.Email, applicationUser.Email!),
                    new Claim(ClaimTypes.Role, role!),
                    new Claim("PartnerId", partnerUser.Partner.Id.ToString()),
                };
            }
            else
            {
                userClaims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, applicationUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, applicationUser.Fullname!),
                    new Claim(ClaimTypes.Email, applicationUser.Email!),
                    new Claim(ClaimTypes.Role, role!)
                };
            }
            var token = new JwtSecurityToken(
                issuer: config.Value.Issuer,
                audience: config.Value.Audience,
                claims: userClaims,
                expires: DateTime.Now.AddHours(4),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<RefreshTokenInfo?> FindRefreshTokenByUserId(int userId)
        {
            return await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.UserId == userId);
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshToken token)
        {
            if (token == null) return new LoginResponse(false, "Token is empty");

            var findingToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.Token!.Equals(token.Token));
            if (findingToken == null) return new LoginResponse(false, "Refresh token is required");

            //get user
            var user = await appDbContext.ApplicationUsers.FirstOrDefaultAsync(_ => _.Id == findingToken.UserId);
            if (user == null) return new LoginResponse(false, "Refresh token could not be generated because user not found");

            var userRole = await FindUserRole(user.Id);
            var systemRole = await FindSystemRole(userRole.Role.Id);
            string jwtToken = GenerateToken(user, systemRole?.Name);
            string refreshToken = GenerateRefreshToken();

            var updatingRefreshToken = await FindRefreshTokenByUserId(user.Id);
            if (updatingRefreshToken == null) return new LoginResponse(false, "Refresh token could not be generated because user has not signed in");

            updatingRefreshToken.Token = refreshToken;
            await appDbContext.SaveChangesAsync();
            return new LoginResponse(true, "Token refreshed successfully", jwtToken, refreshToken);
        }
    }
}
