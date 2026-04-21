using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Model;
using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using ASPNETCore.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace ASPNETCore.Application.Services
{
    public class AccountService
    {
        private readonly BanService _banService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserRepository _userRepository; 
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public AccountService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUserRepository userRepository,
            IConfiguration configuration,
            BanService banService,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _banService = banService;
            _env = env;
            _userRepository = userRepository;
        }

        public async Task<IdentityResult> Register(RegisterModel model)
        {
            var user = new User
            {
                UserName = model.UserName,
                FullName = model.FullName,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return result;

            await _userManager.AddToRoleAsync(user, "volunteer");

            user.VolunteerProfile = new VolunteerProfile
            {
                UserId = user.Id,
                Points = 0,
                RankId = 1 
            };

            if (!string.IsNullOrEmpty(model.OrganizationName))
            {
                user.OrganizerProfile = new OrganizerProfile
                {
                    UserId = user.Id,
                    OrganizationName = model.OrganizationName,
                    Ogrn = model.Ogrn
                };
            }

            await _userManager.UpdateAsync(user);

            return result;
        }
        public async Task<AuthResponse?> Login(LoginModel model)
        {
            var user = await _userRepository.GetByUserNameWithProfilesAsync(model.UserName);

            if (user == null)
                return null;
            var isBanned = await _banService.IsUserBannedAsync(user.Id);
            if (isBanned)
                throw new Exception("User is banned");
            var result = await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                false,
                false
            );

            if (!result.Succeeded)
                return null;

            var token = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponse
            {
                Token = token,
                User = new UserDTO(user),
                Role = roles.FirstOrDefault()
            };
        }
        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<UserDTO?> GetCurrentUser(ClaimsPrincipal principal)
        {
            var userId = _userManager.GetUserId(principal);
            var user = await _userRepository.GetByIdWithProfilesAsync(userId);

            if (user == null)
                return null;

            return new UserDTO(user);
        }
        public async Task<UserForModerDTO?> GetUserById(string id)
        {
            var user = await _userRepository.GetByIdWithFullDetailsAsync(id);

            if (user == null)
                return null;

            return new UserForModerDTO(user);
        }
        public async Task<PaginatedResponse<UserDTO>> GetAllUsers(int page = 1, int pageSize = 10, string? search = null)
        {
            var (users, totalCount) = await _userRepository.GetPagedAsync(page, pageSize, search);

            return new PaginatedResponse<UserDTO>
            {
                Items = users.Select(u => new UserDTO(u)).ToList(),
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
        public async Task<List<UserDTO>> GetForRating()
        {
            var users = await _userRepository.GetForRatingAsync();

            return users.Select(u => new UserDTO(u)).ToList();
        }
        public async Task<bool> UpdateProfile(ClaimsPrincipal principal, UpdateProfileModel model)
        {
            var userId = _userManager.GetUserId(principal);
            var user = await _userRepository.GetByIdWithProfilesAsync(userId);

            if (user == null)
                return false;

            user.FullName = model.FullName ?? user.FullName;
            if (model.UserName !=  null /*&& (user.VolunteerProfile?.Coins >= 20 || user.VolunteerProfile == null)*/)
            {
                user.UserName = model.UserName ?? user.UserName;
                //user.VolunteerProfile.Coins -= 20;
            }     

            if (user.OrganizerProfile != null)
            {
                user.OrganizerProfile.OrganizationName =
                    model.OrganizationName ?? user.OrganizerProfile.OrganizationName;

                user.OrganizerProfile.Ogrn =
                    model.Ogrn ?? user.OrganizerProfile.Ogrn;
            }

            if (model.Image != null && model.Image.Length > 0 /*&& (user.VolunteerProfile?.Coins >= 40 || user.VolunteerProfile == null)*/)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "profilepics");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                user.ProfileImagePath = $"/profilepics/{uniqueFileName}";
                //user.VolunteerProfile.Coins -= 40;
            }
            if (model.BackgroundImage != null && model.BackgroundImage.Length > 0 /*&& user.VolunteerProfile?.Coins >= 100 || user.OrganizerProfile != null*/)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "backgroundpics");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(model.BackgroundImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.BackgroundImage.CopyToAsync(stream);
                }

                user.BackgroundImagePath = $"/backgroundpics/{uniqueFileName}";
                //user.VolunteerProfile.Coins -= 100;
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

            var roles = await _userManager.GetRolesAsync(user);

            claims.AddRange(roles.Select(r =>
                new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.Now.AddDays(
                Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
