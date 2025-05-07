using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ServerLibrary.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IOptions<JwtSection> config;
        private readonly AppDbContext appDbContext;
        private readonly IPartnerService partnerService;
        private readonly IEmployeeService employeeService;
        private readonly IEmailService emailService;
        private readonly IOptions<FrontendConfig> _frontendConfig;

        public UserService(IOptions<JwtSection> _config,
        AppDbContext _appDbContext,
        IPartnerService _partnerService,
        IEmployeeService _employeeService,
        IEmailService _emailService,
        IOptions<FrontendConfig> frontendConfig)
        {
            config = _config;
            appDbContext = _appDbContext;
            partnerService = _partnerService;
            employeeService = _employeeService;
            emailService = _emailService;
            _frontendConfig = frontendConfig;
        }
        public async Task<GeneralResponse> CreateUnverifiedAdminAsync(RegisterAdmin user)
        {
            var checkingUser = await FindUserByEmail(user.Email);
            if (checkingUser != null) return new GeneralResponse(false, "Email đã được sử dụng !");

            var adminRole = await CheckSystemRole(Constants.Role.Admin);
            if (adminRole == null) return new GeneralResponse(false, "Vai trò Admin không được tìm thấy");

            var partner = await partnerService.FindById(user.PartnerId);
            if (partner == null) return new GeneralResponse(false, "Không tìm thấy tổ chức");

            var strategy = appDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
    {
        await using var transaction = await appDbContext.Database.BeginTransactionAsync();
        try
        {
            var applicationUser = await appDbContext.InsertIntoDb(new ApplicationUser()
            {
                Email = user.Email,
                FullName = user.FullName,
                AccountStatus = AccountStatus.WaitingVerification,
            });

            await appDbContext.InsertIntoDb(new UserRole() { Role = adminRole, User = applicationUser });

            // Seed roles and create employee
            var employee = await SeedDefaultEmployeeRolesAndAdminAsync(partner.Id, applicationUser);

            await appDbContext.InsertIntoDb(new PartnerUser()
            {
                User = applicationUser,
                Partner = partner,
                EmployeeId = employee.Id,
            });

            await transaction.CommitAsync();
            return await SendVerificationEmailAsync(applicationUser);
        }
        catch (Exception ex)
        {
            if (transaction.GetDbTransaction().Connection != null)
            {
                await transaction.RollbackAsync();
            }
            return new GeneralResponse(false, $"Lỗi khi tạo tài khoản: {ex.Message}");
        }
    });
        }


        // public async Task<DataObjectResponse> AssignDefaultApplicationsToPartner(int partnerId, PartnerLicenseDTO partnerLicense)
        // {
        //     var now = DateTime.UtcNow;
        //     var defaultApps = await appDbContext.Applications
        //         .ToListAsync();

        //     foreach (var app in defaultApps)
        //     {
        //         await appDbContext.AddAsync(new PartnerLicenseDTO()
        //         {
        //             PartnerId = partnerId,
        //             ApplicationId = app.ApplicationId,
        //             StartDate = now,
        //             EndDate = now.AddDays(2),
        //             LicenceType = "Monthly",
        //             Status = "Active"
        //         });
        //     }
        //     await appDbContext.SaveChangesAsync();
        // }
        private async Task<SystemRole?> CheckSystemRole(string role)
        {
            return await appDbContext.SystemRoles.FirstOrDefaultAsync(r => r.Name!.ToLower().Equals(role.ToLower()));
        }

        private async Task<bool?> FindUserByEmployee(int? employeeId)
        {
            return await appDbContext.PartnerUsers.AnyAsync(user => user.EmployeeId == employeeId);
        }

        public async Task<ApplicationUser?> FindUserByEmail(string? email)
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

        public async Task<LoginResponse> SignInAsync(Login user)
        {
            var applicationUser = await FindUserByEmail(user.Email);
            if (applicationUser == null) return new LoginResponse(false, "Email không tìm thấy");

            //verify
            if (!BCrypt.Net.BCrypt.Verify(user.Password, applicationUser.Password))
                return new LoginResponse(false, "Mật khẩu không hợp lệ");
            var userRole = await FindUserRole(applicationUser.Id);
            if (userRole == null) return new LoginResponse(false, "Không tìm thấy quyền người dùng");
            var systemRole = await FindSystemRole(userRole.Role.Id);

            string jwtToken = await GenerateToken(applicationUser, systemRole!.Name);
            if (string.IsNullOrEmpty(jwtToken)) return new LoginResponse(false, "Không tìm thấy đối tác người dùng");
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
                await appDbContext.InsertIntoDb(new RefreshTokenInfo() { Token = refreshToken, UserId = applicationUser.Id });
            }
            return new LoginResponse(true, "Đăng nhập thành công", jwtToken, refreshToken);
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        private async Task<string> GenerateToken(ApplicationUser applicationUser, string? role)
        {
            if (applicationUser == null) throw new ArgumentNullException(nameof(applicationUser));
            if (string.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role));
            var userClaims = new List<Claim>();

            // Thêm basic claims
            userClaims.AddRange(BuildBasicClaims(applicationUser, role));

            // Thêm partner claims (nếu không phải SysAdmin)
            if (role != Constants.Role.SysAdmin)
            {
                var partnerClaims = await BuildPartnerClaimsAsync(applicationUser, role);
                if (!partnerClaims.Any()) // Nếu không tìm thấy partnerUser
                    return string.Empty;
                userClaims.AddRange(partnerClaims);
            }
            // Tạo JWT token
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config.Value.Issuer,
                audience: config.Value.Audience,
                claims: userClaims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<EmployeeDTO> SeedDefaultEmployeeRolesAndAdminAsync(int partnerId, ApplicationUser adminUser)
        {
            var allPermissions = await appDbContext.CRMPermissions.ToListAsync();

            var roles = new List<CRMRole>
    {
        new CRMRole { Name = "Admin", PartnerId = partnerId },
        new CRMRole { Name = "Employee", PartnerId = partnerId },
        new CRMRole { Name = "Shipper", PartnerId = partnerId }
    };
            appDbContext.CRMRoles.AddRange(roles);
            await appDbContext.SaveChangesAsync();

            var adminRole = roles.First(r => r.Name == "Admin");

            var rolePermissions = allPermissions.Select(p => new CRMRolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id
            }).ToList();

            appDbContext.CRMRolePermissions.AddRange(rolePermissions);
            await appDbContext.SaveChangesAsync();

            var employeeData = new CreateEmployee
            {
                // ** Manually Enter 
                FullName = adminUser.FullName,
                Email = adminUser.Email,
                // ** Auto-generated
                EmployeeCode = "NV0000001",
                CRMRoleId = adminRole.Id,
                PartnerId = partnerId
            };
            return await employeeService.CreateEmployeeAdminAsync(employeeData);
        }

        private async Task<RefreshTokenInfo?> FindRefreshTokenByUserId(int userId)
        {
            return await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.UserId == userId);
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshToken token)
        {
            var findingToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.Token!.Equals(token.Token));
            if (findingToken == null) return new LoginResponse(false, "Refresh token is required");

            //get user
            var user = await appDbContext.ApplicationUsers.FirstOrDefaultAsync(_ => _.Id == findingToken.UserId);
            if (user == null) return new LoginResponse(false, "Refresh token could not be generated because user not found");

            var userRole = await FindUserRole(user.Id);
            if (userRole == null) return new LoginResponse(false, "User role not found");
            var systemRole = await FindSystemRole(userRole.Role.Id);
            string jwtToken = await GenerateToken(user, systemRole?.Name);
            string refreshToken = GenerateRefreshToken();

            var updatingRefreshToken = await FindRefreshTokenByUserId(user.Id);
            if (updatingRefreshToken == null) return new LoginResponse(false, "Refresh token could not be generated because user has not signed in");

            updatingRefreshToken.Token = refreshToken;
            await appDbContext.SaveChangesAsync();
            return new LoginResponse(true, "Token refreshed successfully", jwtToken, refreshToken);
        }
        public async Task<GeneralResponse> VerifyAsync(string email, string token)
        {
            var checkUser = await FindUserByEmail(email);
            if (checkUser == null)
            {
                return new GeneralResponse(false, "Email không tồn tại, vui lòng đăng ký tài khoản !");
            }
            var verification = await appDbContext.EmailVerifications.FirstOrDefaultAsync(v => v.Email == email && v.Token == token && !v.IsVerified);


            if (verification == null)
                return new GeneralResponse(false, "Liên kết xác minh không hợp lệ hoặc đã hết hạn.");

            if (verification.ExpiresAt < DateTime.UtcNow)
                return new GeneralResponse(false, "Liên kết xác minh đã hết hạn.");

            var user = await appDbContext.ApplicationUsers
                .FirstOrDefaultAsync(u => u.Id == verification.UserId && u.IsActivateEmail == false);


            if (user == null)
                return new GeneralResponse(false, "Không tìm thấy người dùng hoặc đã được xác minh.");

            user.AccountStatus = AccountStatus.Verified;
            verification.IsVerified = true;
            appDbContext.Update(verification);
            await appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Đã xác minh email. Vui lòng đặt mật khẩu của bạn.");
        }

        public async Task<GeneralResponse> SetPasswordAsync(SetPasswordDTO newUser)
        {

            var isUserValid = await CheckVerifiedUser(newUser.Email) ?? false;
            if (!isUserValid)
            {
                return new GeneralResponse(false, "Tài khoản chưa được xác minh!");
            }
            var user = await appDbContext.ApplicationUsers
        .FirstOrDefaultAsync(u => u.Email == newUser.Email && u.IsActivateEmail == false);

            if (user == null)
                return new GeneralResponse(false, "Không tìm thấy người dùng hoặc đã kích hoạt.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
            user.IsActivateEmail = true;
            user.IsActive = true;
            appDbContext.Update(user);
            await appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Đã đặt mật khẩu và kích hoạt người dùng thành công!");
        }

        private async Task<bool?> CheckVerifiedUser(string email)
        {
            return await appDbContext.EmailVerifications.AnyAsync(u => u.Email == email && u.IsVerified == true);

        }

        public async Task<GeneralResponse> ResendVerificationAsync(string email)
        {

            var clientUrl = _frontendConfig.Value.BaseUrl;
            if (string.IsNullOrEmpty(clientUrl))
                return new GeneralResponse(false, "Client URL is not configured");

            var user = await appDbContext.ApplicationUsers
         .FirstOrDefaultAsync(u => u.Email == email && u.IsActivateEmail == false);

            if (user == null)
                return new GeneralResponse(false, "Người dùng không được tìm thấy hoặc đã được xác minh.");

            var existingVerification = await appDbContext.EmailVerifications
                .FirstOrDefaultAsync(v => v.UserId == user.Id && !v.IsVerified);

            string token = Guid.NewGuid().ToString();
            DateTime expiration = DateTime.UtcNow.AddHours(24);

            if (existingVerification != null)
            {
                existingVerification.Token = token;
                existingVerification.ExpiresAt = expiration;
                existingVerification.IsVerified = false;
                appDbContext.Update(existingVerification);
            }
            else
            {
                var emailVerification = new EmailVerification
                {
                    Email = email,
                    Token = token,
                    ExpiresAt = expiration,
                    IsVerified = false,
                    UserId = user.Id
                };
                await appDbContext.InsertIntoDb(emailVerification);
            }

            string verificationLink = $"{clientUrl}/vi/auth/activate-user?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

            string emailBody = $@"
         <h2>Xác thực Email - Gửi lại</h2>
         <p>Xin chào {user.FullName},</p>
        <p>Chúng tôi đã tạo một liên kết xác minh mới cho bạn. Vui lòng nhấn đường dẫn ở dưới xác thực email và tạo mật khẩu cho tài khoản:</p>
        <p><a href='{verificationLink}'>Xác minh email</a></p>
        <p>Liên kết này sẽ hết hạn sau 24 giờ. Nếu bạn không yêu cầu điều này, hãy bỏ qua email này.</p>
        <p>Trân trọng,<br/>Ovie System Service</p>";
            try
            {
                await emailService.SendEmailAsync(email, "Verify Your Email - Ovie System", emailBody);
                await appDbContext.SaveChangesAsync();
                return new GeneralResponse(true, "Liên kết xác minh mới được gửi tới email của bạn.");
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, $"Không gửi được email xác minh: {ex.Message}");
            }
        }


        public async Task<GeneralResponse> CreateUnverifiedUserAsync(RegisterUserDTO user)
        {
            var role = Constants.Role.User;
            var checkingUser = await FindUserByEmail(user.Email);
            if (checkingUser != null) return new GeneralResponse(false, "Tài khoản đã tồn tại !");

            var partner = await partnerService.FindById(user.PartnerId);
            if (partner == null) return new GeneralResponse(false, "Không tìm thấy tổ chức");

            var employee = await employeeService.FindByIdAsync(user.EmployeeId);
            if (employee == null) return new GeneralResponse(false, "Không tìm thấy thông tin nhân viên");

            var applicationUser = new ApplicationUser
            {
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Avatar = user.Avatar,
                Birthday = user.Birthday,
                AccountStatus = AccountStatus.NotSendingVerification,
                Password = null,
                IsActive = false,
                IsActivateEmail = false
            };

            var unverifiedUser = await appDbContext.InsertIntoDb(applicationUser);
            var checkingRole = await CheckSystemRole(role);

            await appDbContext.InsertIntoDb(new UserRole { Role = checkingRole, User = unverifiedUser });

            await appDbContext.InsertIntoDb(new PartnerUser
            {
                User = unverifiedUser,
                Partner = partner,
                EmployeeId = employee.Id
            });

            return new GeneralResponse(true, "User created successfully");
        }


        public async Task<GeneralResponse> SendVerificationEmailAsync(ApplicationUser user)
        {
            var clientUrl = _frontendConfig.Value.BaseUrl;
            if (string.IsNullOrEmpty(clientUrl))
                return new GeneralResponse(false, "Client URL is not configured");

            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            DateTime expiration = DateTime.UtcNow.AddMinutes(5);

            var emailVerification = new EmailVerification
            {
                Email = user.Email,
                Token = token,
                ExpiresAt = expiration,
                IsVerified = false,
                UserId = user.Id
            };
            await appDbContext.InsertIntoDb(emailVerification);

            user.AccountStatus = AccountStatus.WaitingVerification;
            await appDbContext.UpdateDb(user);

            string verificationLink = $"{clientUrl}/vi/auth/activate-user?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
            string templateName = "EmailVerificationTemplate.cshtml";
            string emailBody = await emailService.GetEmailTemplateAsync(user.FullName, verificationLink, templateName);
            try
            {
                await emailService.SendEmailAsync(user.Email, "Xác minh email - Autuna Software", emailBody);
                return new GeneralResponse(true, "Liên kết xác minh được gửi đến email của bạn. Vui lòng kiểm tra email");
            }
            catch (Exception ex)
            {
                user.AccountStatus = AccountStatus.NotSendingVerification;
                await appDbContext.UpdateDb(user);
                return new GeneralResponse(false, $"Lỗi khi gửi email: {ex.Message}");
            }
        }

        public async Task<List<ApplicationUser?>> GetAllMembersAsync(Partner partner)
        {
            if (partner == null)
            {
                throw new ArgumentException($"Partner {nameof(partner)} is null.");
            }

            var users = await appDbContext.PartnerUsers
                .Include(pu => pu.User)
                .Where(pu => pu.Partner.Id == partner.Id)
                .Select(pu => pu.User)
                .ToListAsync();

            return users;
        }
        public async Task<bool> IsValidResetTokenAsync(string? email, string? phoneNumber, string token)
        {
            return await appDbContext.PasswordResetTokens
                .AnyAsync(t => t.Email == email || t.PhoneNumber == phoneNumber && t.Token == token && !t.IsUsed);
        }
        public async Task<bool> ResetPasswordAsync(ResetPasswordDTO request)
        {
            var resetToken = await appDbContext.PasswordResetTokens
        .FirstOrDefaultAsync(t => t.Email == request.Email && t.Token == request.Token && !t.IsUsed);

            if (resetToken == null)
                return false;

            var user = await appDbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return false;

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            appDbContext.PasswordResetTokens.Remove(resetToken);
            await appDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string? email, string? phoneNumber = null)
        {
            var clientUrl = _frontendConfig.Value.BaseUrl;
            // Kiểm tra user có tồn tại không
            var user = await appDbContext.ApplicationUsers
                .FirstOrDefaultAsync(u => u.Email == email || u.Phone == phoneNumber);
            if (user == null) return null;

            var existingToken = await appDbContext.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Email == email || t.PhoneNumber == phoneNumber);
            if (existingToken != null)
            {
                appDbContext.PasswordResetTokens.Remove(existingToken);
                await appDbContext.SaveChangesAsync();
            }
            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var resetToken = new PasswordResetTokens
            {
                Email = email,
                PhoneNumber = phoneNumber,
                Token = token,
                IsUsed = false
            };
            string verificationLink = $"{clientUrl}/vi/auth/reset-password?" +
                 $"email={Uri.EscapeDataString(user.Email ?? "")}" +
                 $"&phoneNumber={Uri.EscapeDataString(user.Phone ?? "")}" +
                 $"&token={Uri.EscapeDataString(token)}";

            string templateName = "ResetPasswordTemplate.cshtml";

            string emailBody = await emailService.GetEmailTemplateAsync(user.FullName, verificationLink, templateName);

            await emailService.SendEmailAsync(user.Email, "Xác minh email - Autuna Software", emailBody);

            await appDbContext.PasswordResetTokens.AddAsync(resetToken);
            await appDbContext.SaveChangesAsync();
            return token;
        }

        public async Task<GeneralResponse> CreateSysAdminAsync(RegisterSysAdmin user, string? role)
        {
            var checkingUser = await FindUserByEmail(user.Email);
            if (checkingUser != null) return new GeneralResponse(false, "User already exists");
            var checkingRole = await CheckSystemRole(role);
            if (checkingRole == null) return new GeneralResponse(false, "Role not found");
            Partner? partner = null;
            Employee? employee = null;
            if (role != Constants.Role.SysAdmin)
            {
                // Check Partner
                partner = await partnerService.FindById(user.PartnerId);
                if (partner == null) return new GeneralResponse(false, "Partner not found");

                employee = await employeeService.FindByIdAsync(user.EmployeeId);
                if (employee == null)
                    return new GeneralResponse(false, "Employee not found");
            }
            var applicationUser = await appDbContext.InsertIntoDb(new ApplicationUser()
            {
                Email = user.Email,
                FullName = user.FullName,
                AccountStatus = AccountStatus.Verified,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
            });
            await appDbContext.InsertIntoDb(new UserRole() { Role = checkingRole, User = applicationUser });

            if (role != Constants.Role.SysAdmin)
            {
                await appDbContext.InsertIntoDb(new PartnerUser()
                {
                    User = applicationUser,
                    Partner = partner,
                    EmployeeId = user.EmployeeId
                });
            }
            return new GeneralResponse(true, $"{role} created");
        }



        // ** Claims Builder 

        private List<Claim> BuildBasicClaims(ApplicationUser user, string role)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
            if (!string.IsNullOrEmpty(user.FullName))
                claims.Add(new Claim(ClaimTypes.Name, user.FullName));
            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            if (!string.IsNullOrEmpty(user.Phone))
                claims.Add(new Claim("Phone", user.Phone));
            if (!string.IsNullOrEmpty(user.Avatar))
                claims.Add(new Claim("Avatar", user.Avatar));

            return claims;
        }

        private async Task<List<Claim>> BuildPartnerClaimsAsync(ApplicationUser user, string? role)
        {
            var claims = new List<Claim>();

            var partnerUser = await appDbContext.PartnerUsers
                .Include(p => p.Partner)
                .Include(pu => pu.Employee)
                .ThenInclude(r => r.CRMRole)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(x => x.User.Id == user.Id);

            if (partnerUser == null)
                return claims;

            claims.Add(new Claim("CompanyName", partnerUser.Partner.Name));
            claims.Add(new Claim("PartnerId", partnerUser.Partner.Id.ToString()));

            if (partnerUser.Employee?.CRMRole != null)
            {
                claims.Add(new Claim("CRMRoleId", partnerUser.Employee.CRMRole.Id.ToString()));
                claims.Add(new Claim("CRMRoleName", partnerUser.Employee.CRMRole.Name));
            }

            if (partnerUser.EmployeeId != null)
            {
                claims.Add(new Claim("EmployeeId", partnerUser.EmployeeId.ToString()));
            }

            if (role == Constants.Role.Admin)
            {
                claims.Add(new Claim("Owner", "true"));
            }

            var licenses = await appDbContext.PartnerLicenses
         .Where(l => l.PartnerId == partnerUser.Partner.Id
                  && l.Status == "Active").Select(l => new { l.ApplicationId, l.EndDate, l.Application.Name })
         .ToListAsync();

            if (licenses.Any())
            {
                var apps = licenses.Select(l => new
                {   
                    AppName = l.Name, 
                    AppId = l.ApplicationId.ToString(),
                    AppExpiredAt = l.EndDate.ToString("o")
                }).ToList();
                claims.Add(new Claim("Apps", JsonSerializer.Serialize(apps)));
            }
            return claims;
        }
    }
}
