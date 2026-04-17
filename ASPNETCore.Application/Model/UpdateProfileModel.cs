using Microsoft.AspNetCore.Http;

namespace ASPNETCore.Application.Model
{
    public class UpdateProfileModel
    {
        public string? FullName { get; set; }       
        public string? UserName { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? OrganizationName { get; set; }
        public string? Ogrn { get; set; }
        public IFormFile? Image { get; set; }
    }
}
