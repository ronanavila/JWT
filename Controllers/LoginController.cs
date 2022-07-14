using Auth.Models;
using Auth.Repositories;
using Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers;

[ApiController]
[Route("v1")]
public class LoginController: ControllerBase
{
    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<dynamic>> AuthenticateAsync([FromBody] User model)
    {
        var user = UserRepository.Get(model.UserName, model.Password);

        if(user == null)
            return NotFound(new {message = "User or Password incorrect."});
        
        var token = TokenService.GenerateToken(user);

        user.Password = "";

        return new 
        {
            user = user,
            token = token
        };
    }
}