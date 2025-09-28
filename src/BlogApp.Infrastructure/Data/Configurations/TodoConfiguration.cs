using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Data.Configurations;

public class TodoConfiguration : IEntityTypeConfiguration<Todo>
{
    public void Configure(EntityTypeBuilder<Todo> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.HasOne(e => e.User)
            .WithMany(e => e.Todos)
            .HasForeignKey(e => e.UserId);

        // Configure indexes
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.IsCompleted);
    }
}