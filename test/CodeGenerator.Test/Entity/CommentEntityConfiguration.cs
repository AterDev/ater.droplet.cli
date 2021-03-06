using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CodeGenerator.Test.Entity;

public class CommentEntityConfiguration : IEntityTypeConfiguration<Comments>
{
    public void Configure(EntityTypeBuilder<Comments> builder)
    {
        builder.Property(b => b.Content)
            .HasMaxLength(1000)
            .IsRequired();
    }
}
