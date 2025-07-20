using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using VerticalSliceArchitecture.Application.Domain.Patients;

namespace VerticalSliceArchitecture.Application.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients", "patient");

        builder.Ignore(p => p.DomainEvents);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Phone)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Email)
            .HasMaxLength(256)
            .IsRequired();
    }
}