using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Model;
using ASPNETCore.Domain.Entities;
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
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<IdentityResult> Register(RegisterModel model)
        {
            var user = new User
            {
                UserName = model.UserName,
                FullName = model.FullName
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
            var result = await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                false,
                false);

            if (!result.Succeeded)
                return null;

            var user = await _userManager.Users
                .Include(u => u.VolunteerProfile)
                .Include(u => u.OrganizerProfile)
                .FirstOrDefaultAsync(u => u.UserName == model.UserName);

            var token = await GenerateJwtToken(user);

            return new AuthResponse
            {
                Token = token,
                User = new UserDTO(user)
            };
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<UserDTO?> GetCurrentUser(ClaimsPrincipal principal)
        {
            var user = await _userManager.Users
                .Include(u => u.VolunteerProfile)
                .Include(u => u.OrganizerProfile)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(principal));

            if (user == null)
                return null;

            return new UserDTO(user);
        }

        public async Task<bool> UpdateProfile(ClaimsPrincipal principal, UpdateProfileModel model)
        {
            var user = await _userManager.Users
                .Include(u => u.OrganizerProfile)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(principal));

            if (user == null)
                return false;

            user.FullName = model.FullName ?? user.FullName;

            if (user.OrganizerProfile != null)
            {
                user.OrganizerProfile.OrganizationName =
                    model.OrganizationName ?? user.OrganizerProfile.OrganizationName;

                user.OrganizerProfile.Ogrn =
                    model.Ogrn ?? user.OrganizerProfile.Ogrn;
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
