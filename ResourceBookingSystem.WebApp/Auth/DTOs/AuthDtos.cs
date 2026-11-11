using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceBookingSystem.WebApp.Auth.DTOs
{
    public class RegisterRequestDto
    {
        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string ContactNumber { get; set; } = null!;

        public string StudentId { get; set; } = null!;
    }

    public class LoginRequestDto
    {
        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;
    }

    public class UserDto
    {
        public Guid UserId { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? ContactNumber { get; set; }

        public string? StudentId { get; set; }

        public string Role { get; set; } = null!;

        public bool IsActive { get; set; }
    }

    public class LoginServiceResponseDto
    {
        public UserDto User { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class LoginResponseDto
    {
        public UserDto User { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
    }
    public class RefreshTokenResponseDto
    {
        public UserDto User { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
    }
}
