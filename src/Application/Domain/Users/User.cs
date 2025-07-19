using VerticalSliceArchitecture.Application.Common;

namespace VerticalSliceArchitecture.Application.Domain.Users;

public class User : AuditableEntity, IHasDomainEvent
{
    public int Id { get; set; }

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public List<DomainEvent> DomainEvents { get; } = new();
}
