using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabit.Api.Database.Configurations;

public sealed class HabitConfiguration : IEntityTypeConfiguration<Habit>
{
    public void Configure(EntityTypeBuilder<Habit> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(500);
        
        builder.Property(x => x.Name)
            .HasMaxLength(100);

        builder.OwnsOne(x => x.Frequency);

        builder.OwnsOne(x => x.Target, mb =>
        {
            mb.Property(x => x.Unit)
                .HasMaxLength(100);
        });

        builder.OwnsOne(x => x.Milestone);
    }
}
