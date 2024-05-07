using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly DataContext _context;

    public AccountController(DataContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Account already exists");

        var user = new User
        {
            Username = registerDto.Username.ToLower(),
            Password = registerDto.Password,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Username = user.Username,
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _context.Users.FindAsync(loginDto.Username);

        if (user == null) return Unauthorized("Invalid Username");

        if (user.Password != loginDto.Password) return Unauthorized("Invalid Password");

        return new UserDto
        {
            Username = user.Username,
        };
    }

    private async Task<bool> UserExists(string Username)
    {
        return await _context.Users.AnyAsync(u => u.Username == Username.ToLower());
    }
}
