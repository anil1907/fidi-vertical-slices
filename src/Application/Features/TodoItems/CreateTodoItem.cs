﻿using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Domain.Todos;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.TodoItems;

public class CreateTodoItemController : ApiControllerBase
{
    [HttpPost("/api/todo-items")]
    public async Task<IActionResult> Create(CreateTodoItemCommand command)
    {
        var result = await Mediator.Send(command);

        return result.Match(
            id => Ok(id),
            Problem);
    }
}

public record CreateTodoItemCommand(int ListId, string? Title) : IRequest<ErrorOr<Guid>>;

internal sealed class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
{
    public CreateTodoItemCommandValidator()
    {
        RuleFor(v => v.Title)
            .MaximumLength(200)
            .NotEmpty();
    }
}

internal sealed class CreateTodoItemCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateTodoItemCommand, ErrorOr<Guid>>
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ErrorOr<Guid>> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = new TodoItem
        {
            ListId = request.ListId,
            Title = request.Title,
            Done = false,
        };

        entity.DomainEvents.Add(new TodoItemCreatedEvent(entity));

        _context.TodoItems.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}