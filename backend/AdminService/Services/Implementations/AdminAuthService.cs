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

        public async Task<AdminLoginResponse> LoginAsync(AdminLoginRequest request)
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

            // Log successful login
            await _auditService.LogActionAsync(admin.Id, "ADMIN_LOGIN", "AdminUser", admin.Id, 
                new { Email = admin.Email, LoginTime = DateTime.UtcNow }, "", "");

            return new AdminLoginResponse
            {
                Token = token,
                AdminUser = adminDto,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                RequirePasswordChange = admin.RequirePasswordChange,
                RequireTwoFactor = admin.TwoFactorEnabled && string.IsNullOrEmpty(request.TwoFactorCode)
            };
        }

        public async Task<AdminUserDto> GetCurrentAdminAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JWT_SECRET_KEY"]!);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["JWT_ISSUER"],
                    ValidAudience = _configuration["JWT_AUDIENCE"],
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var adminId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

                var admin = await _context.AdminUsers
                    .Include(a => a.Roles)
                    .FirstOrDefaultAsync(a => a.Id == adminId && a.IsActive);

                if (admin == null)
                    throw new UnauthorizedAccessException("Admin not found or inactive");

                return _mapper.Map<AdminUserDto>(admin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating admin token");
                throw new UnauthorizedAccessException("Invalid token");
            }
        }

        public async Task<AdminUserDto> CreateAdminUserAsync(CreateAdminUserRequest request, string createdBy)
        {
            if (await _context.AdminUsers.AnyAsync(a => a.Email == request.Email))
                throw new InvalidOperationException("Admin user with this email already exists");

            var roles = await _context.AdminRoles
                .Where(r => request.RoleIds.Contains(r.Id))
                .ToListAsync();

            var admin = new AdminUser
            {
                Email = request.Email,
                Name = request.Name,
                PasswordHash = HashPassword(request.Password),
                AdminLevel = request.AdminLevel,
                RequirePasswordChange = request.RequirePasswordChange,
                CreatedBy = createdBy,
                Roles = roles
            };

            _context.AdminUsers.Add(admin);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(createdBy, "ADMIN_USER_CREATED", "AdminUser", admin.Id,
                new { Email = admin.Email, Name = admin.Name, AdminLevel = admin.AdminLevel }, "", "");

            return _mapper.Map<AdminUserDto>(admin);
        }

        public async Task<AdminUserDto> UpdateAdminUserAsync(string adminId, UpdateAdminUserRequest request, string updatedBy)
        {
            var admin = await _context.AdminUsers
                .Include(a => a.Roles)
                .FirstOrDefaultAsync(a => a.Id == adminId);

            if (admin == null)
                throw new ArgumentException("Admin user not found");

            var oldData = new { admin.Name, admin.Email, admin.AdminLevel, admin.IsActive };

            admin.Name = request.Name;
            admin.Email = request.Email;
            admin.AdminLevel = request.AdminLevel;
            admin.IsActive = request.IsActive;
            admin.UpdatedAt = DateTime.UtcNow;

            // Update roles
            admin.Roles.Clear();
            var newRoles = await _context.AdminRoles
                .Where(r => request.RoleIds.Contains(r.Id))
                .ToListAsync();
            admin.Roles = newRoles;

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(updatedBy, "ADMIN_USER_UPDATED", "AdminUser", admin.Id,
                new { OldData = oldData, NewData = new { admin.Name, admin.Email, admin.AdminLevel, admin.IsActive } }, "", "");

            return _mapper.Map<AdminUserDto>(admin);
        }

        public async Task<bool> ChangePasswordAsync(string adminId, ChangePasswordRequest request)
        {
            var admin = await _context.AdminUsers.FindAsync(adminId);
            if (admin == null)
                throw new ArgumentException("Admin user not found");

            if (!VerifyPassword(request.CurrentPassword, admin.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect");

            admin.PasswordHash = HashPassword(request.NewPassword);
            admin.PasswordChangedAt = DateTime.UtcNow;
            admin.RequirePasswordChange = false;
            admin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(adminId, "PASSWORD_CHANGED", "AdminUser", adminId,
                new { PasswordChangedAt = DateTime.UtcNow }, "", "");

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, string resetBy)
        {
            var admin = await _context.AdminUsers.FindAsync(request.AdminId);
            if (admin == null)
                throw new ArgumentException("Admin user not found");

            admin.PasswordHash = HashPassword(request.NewPassword);
            admin.PasswordChangedAt = DateTime.UtcNow;
            admin.RequirePasswordChange = request.RequirePasswordChange;
            admin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(resetBy, "PASSWORD_RESET", "AdminUser", admin.Id,
                new { ResetBy = resetBy, RequirePasswordChange = request.RequirePasswordChange }, "", "");

            return true;
        }

        public async Task<bool> DeactivateAdminAsync(string adminId, string deactivatedBy)
        {
            var admin = await _context.AdminUsers.FindAsync(adminId);
            if (admin == null)
                throw new ArgumentException("Admin user not found");

            admin.IsActive = false;
            admin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(deactivatedBy, "ADMIN_USER_DEACTIVATED", "AdminUser", adminId,
                new { DeactivatedBy = deactivatedBy }, "", "");

            return true;
        }

        public async Task<bool> ActivateAdminAsync(string adminId, string activatedBy)
        {
            var admin = await _context.AdminUsers.FindAsync(adminId);
            if (admin == null)
                throw new ArgumentException("Admin user not found");

            admin.IsActive = true;
            admin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(activatedBy, "ADMIN_USER_ACTIVATED", "AdminUser", adminId,
                new { ActivatedBy = activatedBy }, "", "");

            return true;
        }

        public async Task<PagedResponse<AdminUserDto>> GetAdminUsersAsync(int page, int pageSize)
        {
            var query = _context.AdminUsers.Include(a => a.Roles);
            var totalCount = await query.CountAsync();

            var adminUsers = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var adminDtos = _mapper.Map<List<AdminUserDto>>(adminUsers);

            return new PagedResponse<AdminUserDto>
            {
                Data = adminDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNext = page * pageSize < totalCount,
                HasPrevious = page > 1
            };
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
            var key = Encoding.ASCII.GetBytes(_configuration["JWT_SECRET_KEY"]!);

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
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = _configuration["JWT_ISSUER"],
                Audience = _configuration["JWT_AUDIENCE"],
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
    }
}