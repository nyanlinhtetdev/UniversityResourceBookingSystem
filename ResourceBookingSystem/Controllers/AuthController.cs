using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResourceBookingSystem.WebApp.Auth.DTOs;
using ResourceBookingSystem.WebApp.Auth.Services;
using ResourceBookingSystem.WebApp.Auth.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;

namespace ResourceBookingSystem.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto registerRequestDto)
        {
            var result = await authService.Register(registerRequestDto);
            if(result is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Already Existed",
                    detail: $"User with this email has already existed.");
            }
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            var result = await authService.Login(loginRequestDto);
            if (result is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid email or password",
                    detail: "Invalid email or password");
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            };

            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
            return Ok(new LoginResponseDto
            {
                User = result.User,
                AccessToken = result.AccessToken
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Ok();

            var result = await authService.Refresh(refreshToken);
            if (result is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid refresh token",
                    detail: "Invalid refresh token");
            }
            return Ok(new LoginResponseDto
            {
                User = result.User,
                AccessToken = result.AccessToken
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Ok();

            var result = await authService.RevokeRefreshToken(refreshToken);
            if (result is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid refresh token",
                    detail: "Invalid refresh token");
            }
            return Ok(new
            {
                message = "Successfully logout"
            }); 
        }
    }
}
