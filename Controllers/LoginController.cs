using Auth.Models;
using Auth.Repositories;
using Auth.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
        
        var token =  TokenService.GenerateToken(user);
        var refreshToken = TokenService.GenerateRefreshToken();
        TokenService.SaveRefreshToken(user.UserName, refreshToken);

        user.Password = "";

        return  new 
        {
            user = user,
            token = token,
            refreshToken = refreshToken
        };
    }

    [HttpPost]
    [Route("refresh")]
    public async Task<ActionResult<dynamic>> Refresh([FromBody]RefreshToken refreshToken)
    {
        var principal = TokenService.GetPrincipalFromExpiredToken(refreshToken.Token);
        var userName = principal.Identity.Name;
        var savedRefreshToken = TokenService.GetRefreshToken(userName);

        if(savedRefreshToken != refreshToken.RefreshTk)
            throw new SecurityTokenException("Invalid Refresh Token");

        var newJwtToken = TokenService.GenerateToken(principal.Claims);
        var newRefreshToken = TokenService.GenerateRefreshToken();

        TokenService.DeleteRefreshToken(userName, refreshToken.RefreshTk);
        TokenService.SaveRefreshToken(userName, newRefreshToken);
        return  new 
        {           
            token = newJwtToken,
            refreshToken = newRefreshToken
        };
    }
}