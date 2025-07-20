using VerticalSliceArchitecture.Application.Common;

namespace VerticalSliceArchitecture.Application.Domain.Patients;

public class Patient : AuditableEntity, IHasDomainEvent
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = default!;

    public int Age { get; set; }

    public string Phone { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string? Notes { get; set; }

    public DateTime? LastVisit { get; set; }

    public List<DomainEvent> DomainEvents { get; } = [];
}