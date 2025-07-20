using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Common.Interfaces;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.Auth;

public class LoginUserController : ApiControllerBase
{
    [HttpPost("/api/login")]
    public async Task<IActionResult> Login(LoginUserCommand command)
    {
        var result = await Mediator.Send(command);
        return result.Match(token => Ok(token), Problem);
    }
}

public record LoginUserCommand(string Email, string Password) : IRequest<ErrorOr<string>>;

internal sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(c => c.Password)
            .NotEmpty();
    }
}

internal sealed class LoginUserCommandHandler(
    ApplicationDbContext context,
    IJwtTokenGenerator tokenGenerator) : IRequestHandler<LoginUserCommand, ErrorOr<string>>
{
    public async Task<ErrorOr<string>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            return Error.NotFound(description: "User not found.");
        }

        var valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!valid)
        {
            return Error.Validation(description: "Invalid credentials.");
        }

        var token = tokenGenerator.GenerateToken(user);
        return token;
    }
}
