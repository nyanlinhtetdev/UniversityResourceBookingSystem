using ResourceBookingSystem.Database.AppDbContextModels;
using ResourceBookingSystem.WebApp.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceBookingSystem.WebApp.Auth.ServiceInterfaces
{
    public interface IAuthService
    {
        Task<UserDto?> Register(RegisterRequestDto registerRequestDto);
        Task<LoginServiceResponseDto?> Login(LoginRequestDto request);
        Task<LoginServiceResponseDto?> Refresh(string refreshToken);
        Task<int?> RevokeRefreshToken(string refreshToken);
    }
}
