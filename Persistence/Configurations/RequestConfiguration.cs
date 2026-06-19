using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.HasKey(request => request.Id);

        builder.Property(request => request.Name)
            .IsRequired();

        builder.Property(request => request.Description)
            .IsRequired();
    }
}
