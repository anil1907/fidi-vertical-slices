using System.Security.Cryptography;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Common.Interfaces;
using VerticalSliceArchitecture.Application.Common.Models;
using VerticalSliceArchitecture.Application.Infrastructure.Authentication;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.Auth;

[Tags("Auth")]
public class LoginUserController : ApiControllerBase
{
    [HttpPost("/api/login")]
    public async Task<IActionResult> Login(LoginUserCommand command)
    {
        return ApiResult(await Mediator.Send(command));
    }
}

public record LoginUserCommand(string Email, string Password) : IRequest<ApiResponse<LoginUserResponse>>;

public record LoginUserResponse(string AccessToken, string RefreshToken, int ExpiresIn, UserDto User);
public sealed record UserDto(string Email, string? FirstName, string? LastName);

internal sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithMessage("Email boş olamaz")
            .EmailAddress()
            .WithMessage("Uygun bir email adresi giriniz");
        RuleFor(c => c.Password)
            .NotEmpty()
            .WithMessage("Şifre boş olamaz");
    }
}

internal sealed class LoginUserCommandHandler(
    ApplicationDbContext context,
    IJwtTokenGenerator tokenGenerator,
    IOptions<JwtSettings> options) : IRequestHandler<LoginUserCommand, ApiResponse<LoginUserResponse>>
{
    public async Task<ApiResponse<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            throw new AppException("E-posta adresi ile eşleşen bir kullanıcı bulunamadı.");
        }

        var valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!valid)
        {
            throw new AppException("Şifre geçersiz.");
        }

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);

        await context.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(user.Email, user.FirstName, user.LastName);

        var accessToken = tokenGenerator.GenerateToken(user);
        var expiresIn = options.Value.ExpiryMinutes * 60;

        return ApiResponse<LoginUserResponse>.Success(new LoginUserResponse(accessToken, refreshToken, expiresIn, userDto));
    }
}