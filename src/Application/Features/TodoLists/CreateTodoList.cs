﻿using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Domain.Todos;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.TodoLists;

public class CreateTodoListController : ApiControllerBase
{
    [HttpPost("/api/todo-lists")]
    public async Task<IActionResult> Create(CreateTodoListCommand command)
    {
        var result = await Mediator.Send(command);

        return result.Match(
            id => Ok(id),
            Problem);
    }
}

public record CreateTodoListCommand(string? Title) : IRequest<ErrorOr<Guid>>;

internal sealed class CreateTodoListCommandValidator : AbstractValidator<CreateTodoListCommand>
{
    private readonly ApplicationDbContext _context;

    public CreateTodoListCommandValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .MustAsync(BeUniqueTitle).WithMessage("The specified title already exists.");
    }

    private Task<bool> BeUniqueTitle(string title, CancellationToken cancellationToken)
    {
        return _context.TodoLists
            .AllAsync(l => l.Title != title, cancellationToken);
    }
}

internal sealed class CreateTodoListCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateTodoListCommand, ErrorOr<Guid>>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ErrorOr<Guid>> Handle(CreateTodoListCommand request, CancellationToken cancellationToken)
    {
        var todoList = new TodoList { Title = request.Title };

        _context.TodoLists.Add(todoList);

        await _context.SaveChangesAsync(cancellationToken);

        return todoList.Id;
    }
}