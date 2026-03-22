namespace ASPNETCore.Application.Model
{
    public class RegisterModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string? FullName { get; set; }
        public string Email { get; set; }

        // для орг профиля
        public string? OrganizationName { get; set; }
        public string? Ogrn { get; set; }
    }      
}
