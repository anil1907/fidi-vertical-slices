using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.Auth;

public class LoginUserController : ApiControllerBase
{
    [HttpPost("/api/login")]
    public async Task<IActionResult> Login(LoginUserCommand command)
    {
        var result = await Mediator.Send(command);
        return result.Match(_ => Ok(), Problem);
    }
}

public record LoginUserCommand(string Email, string Password) : IRequest<ErrorOr<Success>>;

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

internal sealed class LoginUserCommandHandler(ApplicationDbContext context) : IRequestHandler<LoginUserCommand, ErrorOr<Success>>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ErrorOr<Success>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
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

        return Result.Success;
    }
}
