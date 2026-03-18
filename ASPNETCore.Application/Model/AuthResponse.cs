using ASPNETCore.Application.DTO;

namespace ASPNETCore.Application.Model
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public UserDTO User { get; set; }
    }
}
