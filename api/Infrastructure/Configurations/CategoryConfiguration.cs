using Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Icon).IsRequired().HasMaxLength(50);
        builder.Property(c => c.HexColor).IsRequired().HasMaxLength(7);
        builder.HasMany(c => c.Transactions).WithOne(t => t.Category).HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.Restrict); // On delete restrict for future implementations of hard delete categories without transactions

        builder.HasIndex(c => c.Name).IsUnique();


        builder.HasData(
            new Category
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"),
                Name = "Jedzenie",
                HexColor = "#34C759",
                Icon = "cart.fill",
                IsDefault = true,
                IsArchived = false
            },
            new Category
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000002"),
                Name = "Restauracje",
                HexColor = "#FF9500",
                Icon = "fork.knife",
                IsDefault = true,
                IsArchived = false
            },
            new Category
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000003"),
                Name = "Transport",
                HexColor = "#007AFF",
                Icon = "car.fill",
                IsDefault = true,
                IsArchived = false
            },
            new Category
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000004"),
                Name = "Mieszkanie i rachunki",
                HexColor = "#5856D6",
                Icon = "house.fill",
                IsDefault = true,
                IsArchived = false
            },
            new Category
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000005"),
                Name = "Zdrowie",
                HexColor = "#FF2D55",
                Icon = "cross.case.fill",
                IsDefault = true,
                IsArchived = false
            },
            new Category
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000006"),
                Name = "Rozrywka",
                HexColor = "#AF52DE",
                Icon = "gamecontroller.fill",
                IsDefault = true,
                IsArchived = false
            },
            new Category
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000007"),
                Name = "Zakupy",
                HexColor = "#FF3B30",
                Icon = "bag.fill",
                IsDefault = true,
                IsArchived = false
            },
            new Category
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000008"),
                Name = "Subskrypcje",
                HexColor = "#5AC8FA",
                Icon = "repeat.circle.fill",
                IsDefault = true,
                IsArchived = false
            },
            new Category
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000009"),
                Name = "Edukacja",
                HexColor = "#32ADE6",
                Icon = "book.fill",
                IsDefault = true,
                IsArchived = false
            },
            new Category
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000010"),
                Name = "Podróże",
                HexColor = "#32ADE6",
                Icon = "airplane",
                IsDefault = true,
                IsArchived = false
            },
            new Category
            {
                Id = Guid.Parse("a1000000-0000-0000-0000-000000000011"),
                Name = "Inne",
                HexColor = "#8E8E93",
                Icon = "tag.fill",
                IsDefault = true,
                IsArchived = false
            }
        );
    }
}
