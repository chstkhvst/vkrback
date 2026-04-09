using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Model;
using ASPNETCore.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _accountService.Register(model);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var loginResponse = await _accountService.Login(new LoginModel
        {
            UserName = model.UserName,
            Password = model.Password
        });

        return Ok(loginResponse);
    }
    //[HttpPost("login")]
    //public async Task<ActionResult<AuthResponse>> Login(LoginModel model)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest(ModelState);

    //    var response = await _accountService.Login(model);

    //    if (response == null)
    //        return Unauthorized();

    //    return Ok(response);
    //}
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var response = await _accountService.Login(model);

            if (response == null)
                return Unauthorized("Invalid credentials");

            return Ok(response);
        }
        catch (Exception ex) when (ex.Message == "User is banned")
        {
            return Unauthorized("User is banned");
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _accountService.Logout();
        return Ok();
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
    {
        var user = await _accountService.GetCurrentUser(User);

        if (user == null)
            return Unauthorized();

        return Ok(user);
    }
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _accountService.UpdateProfile(User, model);

        if (!success)
            return Unauthorized();

        return Ok();
    }
}