using ResourceBookingSystem.Database.AppDbContextModels;
using ResourceBookingSystem.Domain.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceBookingSystem.Domain.Auth.ServiceInterfaces
{
    public interface IAuthService
    {
        Task<User?> Register(RegisterRequestDto registerRequestDto);
        Task<LoginServiceResponseDto?> Login(LoginRequestDto request);
        Task<LoginServiceResponseDto?> Refresh(string refreshToken);
        Task<int?> RevokeRefreshToken(string refreshToken);
    }
}
