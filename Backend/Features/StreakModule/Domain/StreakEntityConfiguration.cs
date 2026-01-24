using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.StreakModule.Domain;

public partial class StreakExpConfiguration : IEntityTypeConfiguration<StreakExp>
{
    public void Configure(EntityTypeBuilder<StreakExp> builder)
    {
        builder.ToTable("mt_streak_exp");

        builder.HasKey(e => e.Id)
            .HasName("pk_mt_streak_exp");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Ignore(e => e.CreateByStr);
        builder.Ignore(e => e.UpdateByStr);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(e => e.StreakDateFrom)
            .IsRequired()
            .HasColumnName("streak_date_from");

        builder.Property(e => e.Duration)
            .IsRequired()
            .HasColumnName("duration");

        builder.Property(e => e.Exp)
            .IsRequired()
            .HasColumnName("exp");

        builder.Property(e => e.Status)
            .HasDefaultValue(0)
            .HasColumnName("status");

        builder.Property(e => e.CreateBy).HasColumnName("create_by");
        builder.Property(e => e.CreateTime)
            .HasDefaultValueSql("(now())")
            .HasColumnType("timestamp with time zone")
            .HasColumnName("create_time");

        builder.Property(e => e.UpdateBy).HasColumnName("update_by");
        builder.Property(e => e.UpdateTime)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("update_time");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_streak_exp_user_id");

        OnConfigurePartial(builder);
    }

    partial void OnConfigurePartial(EntityTypeBuilder<StreakExp> builder);
}

public partial class StreakLogConfiguration : IEntityTypeConfiguration<StreakLog>
{
    public void Configure(EntityTypeBuilder<StreakLog> builder)
    {
        builder.ToTable("mt_streak_log");

        builder.HasKey(e => e.Id)
            .HasName("pk_mt_streak_log");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(e => e.StreakDate)
            .IsRequired()
            .HasColumnName("streak_date");

        builder.HasIndex(e => new { e.UserId, e.StreakDate })
            .IsUnique()
            .HasDatabaseName("ix_streak_log_user_date_unique");

        OnConfigurePartial(builder);
    }

    partial void OnConfigurePartial(EntityTypeBuilder<StreakLog> builder);
}
