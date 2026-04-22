using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Model;
using ASPNETCore.Application.Services;
using ASPNETCore.Domain.Interfaces;
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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _accountService.UpdateProfile(User, model);

        if (!success)
            return Unauthorized();

        return Ok();
    }
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserForModerDTO>> GetUserById(string id)
    {
        var user = await _accountService.GetUserById(id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<ActionResult<PaginatedResponse<UserDTO>>> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var users = await _accountService.GetAllUsers(page, pageSize, search);

        return Ok(users);
    }
    [Authorize]
    [HttpGet("ratingmonthly")]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetForRatingMonthly()
    {
        var users = await _accountService.GetForRatingMonthly();

        return Ok(users);
    }
    [Authorize]
    [HttpGet("rating")]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetForRatingAll()
    {
        var users = await _accountService.GetForRatingAll();

        return Ok(users);
    }
}