using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogApp.Infrastructure.Data.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Content)
            .IsRequired();

        builder.Property(e => e.AuthorId)
            .IsRequired();

        builder.HasOne(e => e.Author)
            .WithMany(e => e.Posts)
            .HasForeignKey(e => e.AuthorId);

        builder.HasOne(e => e.BannerImageFile)
            .WithMany()
            .HasForeignKey(e => e.BannerImageFileId);

        // Configure indexes
        builder.HasIndex(e => e.AuthorId);
        builder.HasIndex(e => e.BannerImageFileId);
    }
}