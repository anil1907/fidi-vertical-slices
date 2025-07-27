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
    public RegisterUserCommandValidator(ApplicationDbContext context)
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(c => c.Password)
            .MinimumLength(6);

        RuleFor(c => c.Email);

        RuleFor(c => c.ConfirmPassword)
            .Equal(c => c.Password)
            .WithMessage("Confirm password must match the password.");
    }
}

internal sealed class RegisterUserCommandHandler(ApplicationDbContext context) : IRequestHandler<RegisterUserCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser)
        {
            throw new ApplicationException("Email adresi sistemde mevcut");
        }

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
