using AdminService.Data;
using AdminService.Models.DTOs;
using AdminService.Models.Entities;
using AdminService.Models.Requests;
using AdminService.Models.Responses;
using AdminService.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AdminService.Services.Implementations
{
    public class AdminAuthService : IAdminAuthService
    {
        private readonly AdminDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;
        private readonly ILogger<AdminAuthService> _logger;

        public AdminAuthService(
            AdminDbContext context,
            IMapper mapper,
            IConfiguration configuration,
            IAuditService auditService,
            ILogger<AdminAuthService> logger)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<AdminLoginResponse> LoginAsync(HttpContext httpContext, AdminLoginRequest request)
        {
            var admin = await _context.AdminUsers
                .Include(a => a.Roles)
                .FirstOrDefaultAsync(a => a.Email == request.Email && a.IsActive);

            if (admin == null || !VerifyPassword(request.Password, admin.PasswordHash))
            {
                _logger.LogWarning("Login attempt failed for email: {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Update last login
            admin.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var adminDto = _mapper.Map<AdminUserDto>(admin);
            var token = GenerateJwtToken(adminDto);

            // Set secure httpOnly cookie (4 hours expiry)
            // Configure security settings based on environment
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Requires HTTPS
                SameSite = SameSiteMode.None, // Cross-site cookie for admin panel
                Expires = DateTime.UtcNow.AddHours(4), // Reduced from 24 to 4 hours
                Path = "/"
            };
            httpContext.Response.Cookies.Append("adminToken", token, cookieOptions);

            // Log successful login
            await _auditService.LogActionAsync(admin.Id, "ADMIN_LOGIN", "AdminUser", admin.Id, 
                new { Email = admin.Email, LoginTime = DateTime.UtcNow }, "", "");

            return new AdminLoginResponse
            {
                Token = string.Empty, 
                AdminUser = adminDto,
                ExpiresAt = DateTime.UtcNow.AddHours(4), 
                RequirePasswordChange = admin.RequirePasswordChange,
                RequireTwoFactor = admin.TwoFactorEnabled && string.IsNullOrEmpty(request.TwoFactorCode)
            };
        }

        public async Task<AdminUserDto> GetCurrentAdminAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                    ?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable is required");
                var key = Encoding.ASCII.GetBytes(secretKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "PancakesAdmin",
                    ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "PancakesAdminUsers",
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var idClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
                              ?? jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")
                              ?? jwtToken.Claims.FirstOrDefault(x => x.Type.EndsWith("/nameidentifier", StringComparison.OrdinalIgnoreCase));
                if (idClaim == null || string.IsNullOrEmpty(idClaim.Value))
                    throw new UnauthorizedAccessException("Token missing subject/identifier claim");
                var adminId = idClaim.Value;

                return await GetAdminByIdAsync(adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating admin token");
                throw new UnauthorizedAccessException("Invalid token");
            }
        }

        public async Task<AdminUserDto> GetAdminByIdAsync(string adminId)
        {
            var admin = await _context.AdminUsers
                .Include(a => a.Roles)
                .FirstOrDefaultAsync(a => a.Id == adminId && a.IsActive);

            if (admin == null)
                throw new UnauthorizedAccessException("Admin not found or inactive");

            return _mapper.Map<AdminUserDto>(admin);
        }



        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                await GetCurrentAdminAsync(token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GenerateJwtToken(AdminUserDto admin)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                ?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable is required");
            var key = Encoding.ASCII.GetBytes(secretKey);

            var permissions = admin.Roles.SelectMany(r => r.Permissions).Distinct().ToList();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, admin.Id),
                new Claim(ClaimTypes.Email, admin.Email),
                new Claim(ClaimTypes.Name, admin.Name),
                new Claim("AdminLevel", admin.AdminLevel.ToString()),
                new Claim("IsAdmin", "true")
            };

            // Add role claims
            foreach (var role in admin.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            // Add permission claims
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(4), 
                Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "PancakesAdmin",
                Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "PancakesAdminUsers",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                var parts = hashedPassword.Split(':');
                if (parts.Length != 2)
                    return false;

                var hash = parts[0];
                var salt = parts[1];

                using var sha256 = SHA256.Create();
                var saltedPassword = password + salt;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                var computedHash = Convert.ToBase64String(hashedBytes);

                return hash == computedHash;
            }
            catch
            {
                return false;
            }
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var salt = Guid.NewGuid().ToString();
            var saltedPassword = password + salt;
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            var hashedPassword = Convert.ToBase64String(hashedBytes);
            return $"{hashedPassword}:{salt}";
        }

        public async Task<bool> HasAdminUsersAsync()
        {
            return await _context.AdminUsers.AnyAsync();
        }

        public async Task<AdminUserDto> CreateBootstrapAdminAsync(CreateAdminUserRequest request)
        {
            // For bootstrap, we ensure this is the first admin user
            var existingAdmins = await _context.AdminUsers.AnyAsync();
            if (existingAdmins)
            {
                throw new InvalidOperationException("Admin users already exist. Use regular admin creation endpoint.");
            }

            // Get the SuperAdmin role
            var superAdminRole = await _context.AdminRoles
                .FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
            
            if (superAdminRole == null)
            {
                throw new InvalidOperationException("SuperAdmin role not found. Please ensure database is properly seeded.");
            }

            var adminUser = new AdminUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                Name = request.Name,
                PasswordHash = HashPassword(request.Password),
                AdminLevel = 4, // SuperAdmin level
                IsActive = true,
                RequirePasswordChange = false,
                TwoFactorEnabled = false,
                CreatedBy = "SYSTEM",
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                PasswordChangedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Roles = new List<AdminRole> { superAdminRole }
            };

            _context.AdminUsers.Add(adminUser);
            await _context.SaveChangesAsync();

            return _mapper.Map<AdminUserDto>(adminUser);
        }
    }
}