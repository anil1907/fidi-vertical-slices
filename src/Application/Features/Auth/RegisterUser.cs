using ErrorOr;
using FluentValidation;
using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Common.Models;
using VerticalSliceArchitecture.Application.Domain.Users;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.Auth;

[Tags("Auth")]
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
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithMessage("E-posta adresi boş bırakılamaz.")
            .EmailAddress()
            .WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(c => c.Password)
            .NotEmpty()
            .WithMessage("Şifre boş bırakılamaz.")
            .MinimumLength(6)
            .WithMessage("Şifre en az 6 karakter olmalıdır.");

        RuleFor(c => c.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Şifre tekrarı boş bırakılamaz.")
            .Equal(c => c.Password)
            .WithMessage("Şifre tekrarı, şifre ile aynı olmalıdır.");
    }
}

internal sealed class RegisterUserCommandHandler(ApplicationDbContext context) : IRequestHandler<RegisterUserCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser)
        {
            throw new AppException("Bu e-posta adresi zaten sistemde kayıtlı.");
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
