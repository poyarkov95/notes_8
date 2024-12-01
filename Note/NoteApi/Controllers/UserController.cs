using BusinessLogic.Model;
using BusinessLogic.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NoteApi.Controllers;

[Microsoft.AspNetCore.Components.Route("api/[controller]")]
public class UserController : BaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
        
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task Register(UserRegisterModel filter)
    {
        await _userService.Register(filter);
    }
        
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<TokenPair> Login(UserRegisterModel filter)
    {
        return await _userService.Login(filter);
    }
}