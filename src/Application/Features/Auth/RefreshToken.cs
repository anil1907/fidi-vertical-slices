using System.Security.Cryptography;

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
public class RefreshTokenController : ApiControllerBase
{
    [HttpPost("/api/refresh-token")]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command)
    {
        return ApiResult(await Mediator.Send(command));
    }
}

public record RefreshTokenCommand(string RefreshToken) : IRequest<ApiResponse<LoginUserResponse>>;

internal sealed class RefreshTokenCommandHandler(
    ApplicationDbContext context,
    IJwtTokenGenerator tokenGenerator,
    IOptions<JwtSettings> options) : IRequestHandler<RefreshTokenCommand, ApiResponse<LoginUserResponse>>
{
    public async Task<ApiResponse<LoginUserResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);
        if (user is null)
        {
            throw new AppException("Geçersiz refresh token.");
        }

        if (user.RefreshTokenExpires is null || user.RefreshTokenExpires <= DateTime.UtcNow)
        {
            throw new AppException("Refresh token süresi dolmuş.");
        }

        var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7);

        await context.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(user.Email, user.FirstName, user.LastName);

        var accessToken = tokenGenerator.GenerateToken(user);
        var expiresIn = options.Value.ExpiryMinutes * 60;

        return ApiResponse<LoginUserResponse>.Success(new LoginUserResponse(accessToken, newRefreshToken, expiresIn, userDto));
    }
}