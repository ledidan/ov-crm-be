using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        private readonly ILogger<UserService> _logger;

        private readonly IConfiguration _configuration;
        public UserService(IOptions<JwtSection> _config,
        AppDbContext _appDbContext,
        IPartnerService _partnerService,
        IEmployeeService _employeeService,
        IEmailService _emailService,
        IConfiguration configuration,
        ILogger<UserService> logger,
        IOptions<FrontendConfig> frontendConfig)
        {
            config = _config;
            appDbContext = _appDbContext;
            partnerService = _partnerService;
            employeeService = _employeeService;
            emailService = _emailService;
            _frontendConfig = frontendConfig;
            _configuration = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<GeneralResponse> CreateUnverifiedAdminAsync(RegisterAdmin user, CreatePartner partner)
        {
            var checkingUser = await FindUserByEmail(user.Email);
            if (checkingUser != null)
            {
                var isLinkPartner = await partnerService.FindUserOfPartner(checkingUser.Id);
                if (isLinkPartner == true)
                    return new GeneralResponse(false, "Email đã được sử dụng !");
            }
            try
            {
                if (checkingUser != null && checkingUser.AccountStatus == AccountStatus.Verified)
                {
                    var hasLicense = await appDbContext.PartnerLicenses.AnyAsync(l => l.UserId == checkingUser.Id);
                    if (!hasLicense)
                    {
                        _logger.LogInformation("User {Email} đã verify nhưng chưa có license, tạo FreeTrial.", user.Email);
                        return await HandleVerifiedUserWithoutPartnerAsync(checkingUser, partner);
                    }
                    _logger.LogWarning("User {Email} có tài khoản nhưng không hợp lệ", user.Email);
                    return new GeneralResponse(false, "Tài khoản đã có license, vui lòng xử lý qua kênh riêng!");
                }

                _logger.LogInformation("User {Email} chưa có tài khoản, tạo mới luôn!", user.Email);
                return await HandleNewUserRegistrationAsync(user, partner);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, $"Lỗi khi tạo tài khoản: {ex.Message}");
            }
        }

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

        public async Task<LoginResponse> SignInAppAsync(Login user)
        {
            var applicationUser = await FindUserByEmail(user.Email);
            if (applicationUser == null) return new LoginResponse(false, "Email không tìm thấy");
            var licenseCheck = await CheckActiveLicenseForRedirectAsync(applicationUser);
            if (licenseCheck.Flag)
            {
                return new LoginResponse(false, licenseCheck.Message, "RedirectRegisterPartner");
            }
            var isLinkPartner = await partnerService.FindUserOfPartner(applicationUser.Id);
            if (isLinkPartner == false)
            {
                _logger.LogInformation("User {Email} is not linked to any partner", user.Email);
                return new LoginResponse(false, "Tài khoản không liên kết với tổ chức nào");
            }

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

            // Thêm partner claims (nếu không phải SysAdmin) và không phải là guest
            if (role != Constants.Role.SysAdmin && applicationUser.IsGuestAccount == false)
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

        // ** Verify email for user registered - Exclude user paid for app !
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
        <p>Trân trọng,<br/>Autuna System Service</p>";
            try
            {
                await emailService.SendEmailAsync(email, "Verify Your Email - Autuna System", emailBody);
                await appDbContext.SaveChangesAsync();
                return new GeneralResponse(true, "Liên kết xác minh mới được gửi tới email của bạn.");
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, $"Không gửi được email xác minh: {ex.Message}");
            }
        }

        public async Task<GeneralResponse> CreateUnverifiedUserByPartnerAsync(RegisterUserDTO user)
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
            DateTime expiration = DateTime.UtcNow.AddHours(24);

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
       .AnyAsync(t =>
           (t.Email == email || t.PhoneNumber == phoneNumber)
           && t.Token == token
           && !t.IsUsed
       );
        }
        // ** popapop use it.
        public async Task<GeneralResponse> ResetPasswordAsync(ResetPasswordDTO request)
        {
            var resetToken = await appDbContext.PasswordResetTokens
        .FirstOrDefaultAsync(t => t.Email == request.Email || t.PhoneNumber == request.PhoneNumber && t.Token == request.Token && !t.IsUsed);

            if (resetToken == null)
                return new GeneralResponse(false, "Token không hợp lệ hoặc đã sử dụng");

            var user = await FindUserByEmail(request.Email);
            if (user == null) return new GeneralResponse(false, "Người dùng không tồn tại");

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            appDbContext.PasswordResetTokens.Remove(resetToken);
            await appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Mật khẩu đã được đặt lại thành công");
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string? email, string? phoneNumber = null)
        {
            var clientUrl = _frontendConfig.Value.BaseUrl;
            // Kiểm tra user có tồn tại không
            var user = await FindUserByEmail(email);
            if (user == null) return null;

            var existingToken = await appDbContext.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Email == email || t.PhoneNumber == phoneNumber);
            if (existingToken != null)
            {
                appDbContext.PasswordResetTokens.Remove(existingToken);
                await appDbContext.SaveChangesAsync();
            }
            int tokenConvert = RandomNumberGenerator.GetInt32(1000, 9999);
            // Tạo token mới
            string token = tokenConvert.ToString();
            var resetToken = new PasswordResetTokens
            {
                Email = email,
                PhoneNumber = phoneNumber,
                Token = token,
                IsUsed = false
            };

            string verificationLink = $"{clientUrl}/vi/auth/forgot-password?" +
                 $"email={Uri.EscapeDataString(user.Email ?? "")}" +
                 $"&phonenumber={Uri.EscapeDataString(user.Phone ?? "")}" +
                 $"&code={Uri.EscapeDataString(token)}";

            var model = new ResetPasswordModel
            {
                VerificationLink = verificationLink,
                Email = email,
                Token = token,
                PhoneNumber = phoneNumber,
            };
            string templateName = "ResetPasswordTemplate.cshtml";

            string emailBody = await emailService.GetResetPasswordTemplateAsync(model, templateName);

            await emailService.SendEmailAsync(user.Email, "Reset your password - Autuna Software", emailBody);

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
         .Where(l => l.PartnerId == partnerUser.Partner.Id)
         .Select(l => new { l.Id, l.ApplicationId, l.EndDate, l.Application.Name })
         .ToListAsync();

            if (licenses.Any())
            {
                var apps = licenses.Select(l => new
                {
                    AppName = l.Name,
                    PartnerLicenseId = l.Id.ToString(),
                    AppId = l.ApplicationId.ToString(),
                    AppExpiredAt = l.EndDate.ToString("o")
                }).ToList();
                claims.Add(new Claim("Apps", JsonSerializer.Serialize(apps)));
            }
            return claims;
        }


        public async Task<EmployeeDTO> SeedDefaultEmployeeRolesAndAdminAsync(int partnerId, ApplicationUser adminUser)
        {
            var allPermissions = await appDbContext.CRMPermissions.ToListAsync();

            var roles = new List<CRMRole>
    {
        new CRMRole { Name = "Admin", PartnerId = partnerId, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
        new CRMRole { Name = "Employee", PartnerId = partnerId, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
        new CRMRole { Name = "Shipper", PartnerId = partnerId, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow }
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

            var systemAdminRole = await CheckSystemRole(Constants.Role.Admin);
            await appDbContext.InsertIntoDb(new UserRole() { Role = systemAdminRole, User = adminUser });

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

        public async Task<LoginResponse> SignInGuestAsync(Login user)
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
            if (string.IsNullOrEmpty(jwtToken)) return new LoginResponse(false, "Không tìm thấy người dùng");
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

        public async Task<GeneralResponse> RegisterForGuestAsync(RegisterGuestDTO guest)
        {
            var checkingUser = await FindUserByEmail(guest.Email);
            if (checkingUser != null) return new GeneralResponse(false, "Tài khoản đã tồn tại !");
            var role = Constants.Role.User;
            var applicationUser = new ApplicationUser
            {
                Email = guest.Email,
                Phone = guest.Phone,
                // Password = BCrypt.Net.BCrypt.HashPassword(guest.Password),
                FullName = guest.FullName,
                IsGuestAccount = true,
                AccountStatus = AccountStatus.WaitingVerification,
                IsActive = false,
                IsActivateEmail = false
            };
            var unverifiedUser = await appDbContext.InsertIntoDb(applicationUser);
            var checkingRole = await CheckSystemRole(role);

            await appDbContext.InsertIntoDb(new UserRole { Role = checkingRole, User = unverifiedUser });

            return await SendVerificationEmailForGuestAsync(applicationUser);
        }

        public async Task<ApplicationUser> GetApplicationUserByIdAsync(int id)
        {
            return await appDbContext.ApplicationUsers
                .FindAsync(id);
        }

        public async Task<GeneralResponse> HandleVerifiedUserWithoutPartnerAsync(ApplicationUser user, CreatePartner partner)
        {
            try
            {
                var partnerData = new CreatePartner
                {
                    ShortName = partner.ShortName,
                    Name = partner.Name,
                    TaxIdentificationNumber = partner.TaxIdentificationNumber,
                    LogoUrl = partner.LogoUrl,
                    EmailContact = partner.EmailContact,
                    TotalEmployees = partner.TotalEmployees,
                    IsOrganization = partner.IsOrganization,
                    OwnerFullName = partner.OwnerFullName,
                    PhoneNumber = partner.PhoneNumber,
                };
                var newPartner = await partnerService.CreatePartnerAsync(partnerData);
                _logger.LogInformation("Tạo partner mới cho user {UserId}, tên {PartnerName}", user.Id, newPartner.Name);

                // Tạo PartnerLicense FreeTrial, status Pending
                var now = DateTime.UtcNow;
                var defaultApps = await appDbContext.Applications.ToListAsync();
                var licenses = new List<PartnerLicense>();
                var activationCode = Guid.NewGuid().ToString().Substring(0, 8);

                foreach (var app in defaultApps)
                {
                    licenses.Add(new PartnerLicense
                    {
                        PartnerId = newPartner.Id,
                        ApplicationId = app.ApplicationId,
                        UserId = user.Id,
                        StartDate = now,
                        EndDate = now.AddDays(15), // 15 ngày trial
                        LicenceType = "FreeTrial",
                        Status = "Pending",
                        CreatedAt = now,
                        ActivationCode = activationCode
                    });
                }
                appDbContext.PartnerLicenses.AddRange(licenses);
                await appDbContext.SaveChangesAsync();
                _logger.LogInformation("Tạo {Count} license FreeTrial cho user {UserId}, chờ kích hoạt nha! 🔑", licenses.Count, user.Id);

                // Liên kết user với partner

                var employee = await SeedDefaultEmployeeRolesAndAdminAsync(newPartner.Id, user);
                await appDbContext.InsertIntoDb(new PartnerUser
                {
                    User = user,
                    Partner = newPartner,
                    EmployeeId = employee.Id
                });
                // Cập nhật trạng thái tài khoản
                user.IsGuestAccount = false;
                await appDbContext.SaveChangesAsync();

                // Gửi email ActivationCode
                var appNames = string.Join(",", defaultApps.Select(a => a.Name ?? "Unknown App"));
                await SendActivationEmailAsync(user.Email, user.FullName ?? "User", activationCode, appNames);
                _logger.LogInformation("Gửi email ActivationCode {Code} cho user {UserId}, check inbox ngay! ", activationCode, user.Id);

                return new GeneralResponse(true, "Vui lòng kích hoạt license qua email để đăng nhập!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý user {UserId} chưa có partner", user.Id);
                return new GeneralResponse(false, "Lỗi hệ thống, thử lại sau nha!");
            }
        }

        public async Task<GeneralResponse> HandleUserWithActiveLicenseAsync(string email, CreatePartner partner)
        {
            var checkingUser = await FindUserByEmail(email);
            // Case 2 User has active license, not FreeTrial.
            var activeLicenses = await appDbContext.PartnerLicenses
       .Where(l => l.UserId == checkingUser.Id && l.Status == "Active" && l.LicenceType != "FreeTrial")
       .ToListAsync();
            if (activeLicenses == null || !activeLicenses.Any())
            {
                _logger.LogWarning("User {Email} không có license active", checkingUser.Email);
                return new GeneralResponse(false, "Tài khoản không có license active");
            }
            try
            {
                // Tạo Partner
                var newPartner = await partnerService.CreatePartnerAsync(partner);
                _logger.LogInformation("Tạo partner mới cho user {UserId}, tên {PartnerName},", checkingUser.Id, newPartner.Name);

                // Cập nhật PartnerId trong PartnerLicense hiện có
                foreach (var license in activeLicenses)
                {
                    license.PartnerId = newPartner.Id;
                    appDbContext.PartnerLicenses.Update(license);
                }
                await appDbContext.SaveChangesAsync();
                _logger.LogInformation("Liên kết {Count} license active với partner {PartnerId} cho user {UserId}", activeLicenses.Count, newPartner.Id, checkingUser.Id);

                // Liên kết user với partner
                var employee = await SeedDefaultEmployeeRolesAndAdminAsync(newPartner.Id, checkingUser);
                await appDbContext.InsertIntoDb(new PartnerUser
                {
                    User = checkingUser,
                    Partner = newPartner,
                    EmployeeId = employee.Id
                });
                checkingUser.IsGuestAccount = false;
                await appDbContext.SaveChangesAsync();

                _logger.LogInformation("User {UserId} đã có partner và license, sẵn sàng login", checkingUser.Id);
                return new GeneralResponse(true, "Tạo tài khoản doanh nghiệp thành công !");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý user {UserId} có license active", checkingUser.Id);
                return new GeneralResponse(false, "Lỗi hệ thống, thử lại sau nha!");
            }
        }

        public async Task<GeneralResponse> HandleNewUserRegistrationAsync(RegisterAdmin user, CreatePartner partner)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.FullName))
            {
                _logger?.LogWarning("Input user không hợp lệ: {User}", user?.ToString() ?? "null");
                return new GeneralResponse(false, "Thông tin user không hợp lệ!");
            }
            if (partner == null)
            {
                _logger?.LogWarning("Input partner không hợp lệ: {Partner}", partner?.ToString() ?? "null");
                return new GeneralResponse(false, "Thông tin partner không hợp lệ!");
            }
            var strategy = appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await appDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Tạo user mới
                    var applicationUser = await appDbContext.InsertIntoDb(new ApplicationUser
                    {
                        Email = user.Email,
                        FullName = user.FullName,
                        AccountStatus = AccountStatus.WaitingVerification,
                        IsGuestAccount = false
                    });
                    if (applicationUser == null)
                    {
                        await transaction.RollbackAsync();
                        _logger?.LogError("Tạo user thất bại cho email {Email}", user.Email);
                        return new GeneralResponse(false, "Không thể tạo tài khoản user!");
                    }
                    _logger.LogInformation("Tạo user mới {Email}", user.Email);

                    // Tạo Partner
                    var newPartner = await partnerService.CreatePartnerAsync(partner);
                    if (newPartner == null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("Tạo partner thất bại cho user {Email}", user.Email);
                        return new GeneralResponse(false, "Không thể tạo tổ chức");
                    }
                    _logger.LogInformation("Tạo partner {PartnerName} cho user {UserId}", newPartner.Name, applicationUser.Id);

                    var now = DateTime.UtcNow;

                    var defaultApps = await appDbContext.Applications.ToListAsync();
                    if (defaultApps == null || !defaultApps.Any())
                    {
                        await transaction.RollbackAsync();
                        _logger?.LogError("Không tìm thấy ứng dụng mặc định cho user {Email}", user.Email);
                        return new GeneralResponse(false, "Không tìm thấy ứng dụng để tạo license!");
                    }
                    var licenses = new List<PartnerLicense>();
                    foreach (var app in defaultApps)
                    {
                        licenses.Add(new PartnerLicense
                        {
                            PartnerId = newPartner.Id,
                            ApplicationId = app.ApplicationId,
                            UserId = applicationUser.Id,
                            StartDate = now,
                            EndDate = now.AddDays(15), // 15 ngày trial
                            LicenceType = "FreeTrial",
                            Status = "Active",
                            CreatedAt = now,
                            ActivationCode = null // Active ngay, không cần mã
                        });
                    }

                    appDbContext.PartnerLicenses.AddRange(licenses);
                    await appDbContext.SaveChangesAsync();
                    _logger.LogInformation("Tạo {Count} license FreeTrial active cho user {UserId}", licenses.Count, applicationUser.Id);

                    var employee = await SeedDefaultEmployeeRolesAndAdminAsync(newPartner.Id, applicationUser);
                    if (employee == null)
                    {
                        await transaction.RollbackAsync();
                        _logger?.LogError("Tạo employee thất bại cho user {Email}", user.Email);
                        return new GeneralResponse(false, "Không thể tạo employee!");
                    }
                    await appDbContext.InsertIntoDb(new PartnerUser
                    {
                        User = applicationUser,
                        Partner = newPartner,
                        EmployeeId = employee.Id
                    });
                    await appDbContext.SaveChangesAsync();

                    // Gửi email xác nhận
                    var emailResponse = await SendVerificationEmailAsync(applicationUser);
                    if (emailResponse == null || !emailResponse.Flag)
                    {
                        await transaction.RollbackAsync();
                        _logger?.LogError("Gửi email xác nhận cho {Email} thất bại", user.Email);
                        return new GeneralResponse(false, "Không thể gửi email xác nhận!");
                    }

                    await transaction.CommitAsync();
                    _logger.LogInformation("Tạo admin {Email} thành công, check email để set password nha", user.Email);
                    return new GeneralResponse(true, "Đăng ký thành công, vui lòng kiểm tra email để đặt mật khẩu!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Lỗi khi tạo user mới {Email}", user.Email);
                    return new GeneralResponse(false, $"Lỗi khi tạo tài khoản: {ex.Message}");
                }
            });
        }
        public async Task<DataObjectResponse> CheckActiveLicenseForRedirectAsync(ApplicationUser user)
        {
            try
            {
                // Kiểm tra liên kết partner
                var isLinkPartner = await partnerService.FindUserOfPartner(user.Id);

                // Kiểm tra license active
                var activeLicenses = await appDbContext.PartnerLicenses
                    .Where(l => l.UserId == user.Id && l.Status == "Active")
                    .ToListAsync();

                if (!activeLicenses.Any())
                {
                    _logger.LogWarning("User {UserId} không có license active, cần kích hoạt.", user.Id);
                    return new DataObjectResponse(false, "Tài khoản chưa có license active, vui lòng kích hoạt hoặc đăng ký!");
                }

                // Case 2: Có license active, type != FreeTrial, chưa có partner
                var nonFreeTrialLicenses = activeLicenses.Where(l => l.LicenceType != "FreeTrial").ToList();
                if (nonFreeTrialLicenses.Any() && isLinkPartner == false)
                {
                    _logger.LogInformation("User {UserId} có {Count} license active (non-FreeTrial), redirect tạo doanh nghiệp!",
                    user.Id, nonFreeTrialLicenses.Count);
                    return new DataObjectResponse(true, "Vui lòng tạo doanh nghiệp để tiếp tục!", new
                    {
                        RedirectToRegisterPartner = true,
                    });
                }
                _logger.LogInformation("User {UserId} có license active và partner, sẵn sàng login!", user.Id);
                return new DataObjectResponse(false, "Đủ điều kiện login !"); // Force false to be processing login page, relfect this state.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra license active cho user {UserId}", user.Id);
                return new DataObjectResponse(false, "Lỗi hệ thống, thử lại sau nha!");
            }
        }


        private async Task SendActivationEmailAsync(string email, string fullName, string activationCode, string appName)
        {
            try
            {
                // Validate configuration
                var baseUrl = _configuration["Frontend:BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new InvalidOperationException("Frontend:BaseUrl is not configured.");
                }

                // Construct activation link
                var activationLink = $"{baseUrl}/vi/auth/activate-license?code={Uri.EscapeDataString(activationCode)}&email={Uri.EscapeDataString(email)}&appname={Uri.EscapeDataString(appName)}";

                // Create the model for the email template
                var model = new ActivationEmailModel
                {
                    FullName = fullName,
                    VerificationLink = activationLink,
                    Email = email,
                    ActivationCode = activationCode,
                    AppName = appName
                };

                var templateName = "ActivationEmail.cshtml";
                var emailBody = await emailService.GetActivateEmailTemplateAsync(model, templateName);

                await emailService.SendEmailAsync(email, $"Kích hoạt tài khoản {appName}", emailBody);

                _logger.LogInformation($"Activation email sent to {email}");
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, $"Failed to send activation email to {email}");
                throw new Exception($"Failed to send activation email to {email}", ex);
            }
        }

        public async Task<GeneralResponse> SendVerificationEmailForGuestAsync(ApplicationUser user)
        {
            var clientUrl = _frontendConfig.Value.SubscriptionUrl;
            if (string.IsNullOrEmpty(clientUrl))
                return new GeneralResponse(false, "Client URL is not configured");

            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            DateTime expiration = DateTime.UtcNow.AddHours(24);

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

            string verificationLink = $"{clientUrl}/activate-user?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
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
    }
}
