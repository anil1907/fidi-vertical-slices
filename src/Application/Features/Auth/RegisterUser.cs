using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Domain.Users;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.Auth;

public class RegisterUserController : ApiControllerBase
{
    [HttpPost("/api/register")]
    public async Task<IActionResult> Register(RegisterUserCommand command)
    {
        var result = await Mediator.Send(command);

        return result.Match(id => Ok(id), Problem);
    }
}

public record RegisterUserCommand(string Email, string Password) : IRequest<ErrorOr<int>>;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private readonly ApplicationDbContext _context;

    public RegisterUserCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(c => c.Password)
            .MinimumLength(6);

        RuleFor(c => c.Email)
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists.");
    }

    private Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return _context.Users.AllAsync(u => u.Email != email, cancellationToken);
    }
}

internal sealed class RegisterUserCommandHandler(ApplicationDbContext context) : IRequestHandler<RegisterUserCommand, ErrorOr<int>>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ErrorOr<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
