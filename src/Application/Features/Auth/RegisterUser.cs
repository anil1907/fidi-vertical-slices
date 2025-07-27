using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Common.Models;
using VerticalSliceArchitecture.Application.Domain.Users;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.Auth;

public class RegisterUserController : ApiControllerBase
{
    [HttpPost("/api/register")]
    public async Task<IActionResult> Register(RegisterUserCommand command)
    {
        return ApiResult(await Mediator.Send(command));
    }
}

public sealed record RegisterUserCommand(string Email, string Password, string FirstName, string LastName, string ConfirmPassword) : IRequest<ApiResponse<Guid>>;

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

        RuleFor(c => c.ConfirmPassword)
            .Equal(c => c.Password)
            .WithMessage("Confirm password must match the password.");
    }

    private Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return _context.Users.AllAsync(u => u.Email != email, cancellationToken);
    }
}

internal sealed class RegisterUserCommandHandler(ApplicationDbContext context) : IRequestHandler<RegisterUserCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.Success(user.Id);
    }
}
