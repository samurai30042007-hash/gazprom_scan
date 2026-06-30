using AIReviewSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIReviewSystem.Infrastructure.Persistence;

public sealed class ReviewDbContext : DbContext
{
    public ReviewDbContext(DbContextOptions<ReviewDbContext> options)
        : base(options)
    {
    }

    public DbSet<AnalysisSession> AnalysisSessions => Set<AnalysisSession>();

    public DbSet<RepositorySnapshot> RepositorySnapshots => Set<RepositorySnapshot>();

    public DbSet<ChangedFile> ChangedFiles => Set<ChangedFile>();

    public DbSet<StaticFinding> StaticFindings => Set<StaticFinding>();

    public DbSet<ReportArtifact> ReportArtifacts => Set<ReportArtifact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnalysisSession>(builder =>
        {
            builder.ToTable("analysis_sessions");
            builder.HasKey(item => item.Id);
            builder.Property(item => item.RepositoryPath).HasMaxLength(1024).IsRequired();
            builder.Property(item => item.Status).IsRequired();
            builder.Property(item => item.Summary).HasMaxLength(4000);
            builder.HasOne(item => item.RepositorySnapshot)
                .WithOne(item => item!.AnalysisSession!)
                .HasForeignKey<RepositorySnapshot>(item => item.AnalysisSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RepositorySnapshot>(builder =>
        {
            builder.ToTable("repository_snapshots");
            builder.HasKey(item => item.Id);
            builder.Property(item => item.BaseCommit).HasMaxLength(64).IsRequired();
            builder.Property(item => item.TargetCommit).HasMaxLength(64).IsRequired();
            builder.Property(item => item.DiffMode).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<ChangedFile>(builder =>
        {
            builder.ToTable("changed_files");
            builder.HasKey(item => item.Id);
            builder.Property(item => item.FilePath).HasMaxLength(1024).IsRequired();
            builder.Property(item => item.ChangeType).IsRequired();
            builder.HasOne(item => item.AnalysisSession)
                .WithMany(item => item.ChangedFiles)
                .HasForeignKey(item => item.AnalysisSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StaticFinding>(builder =>
        {
            builder.ToTable("static_findings");
            builder.HasKey(item => item.Id);
            builder.Property(item => item.RuleId).HasMaxLength(128).IsRequired();
            builder.Property(item => item.Message).HasMaxLength(4000).IsRequired();
            builder.Property(item => item.FilePath).HasMaxLength(1024).IsRequired();
            builder.Property(item => item.AnalyzerName).HasMaxLength(128).IsRequired();
            builder.Property(item => item.Severity).IsRequired();
            builder.Property(item => item.Column);
            builder.HasOne(item => item.AnalysisSession)
                .WithMany(item => item.StaticFindings)
                .HasForeignKey(item => item.AnalysisSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReportArtifact>(builder =>
        {
            builder.ToTable("report_artifacts");
            builder.HasKey(item => item.Id);
            builder.Property(item => item.Format).HasMaxLength(64).IsRequired();
            builder.Property(item => item.Location).HasMaxLength(1024).IsRequired();
            builder.Property(item => item.ContentHash).HasMaxLength(128).IsRequired();
            builder.HasOne(item => item.AnalysisSession)
                .WithMany(item => item.ReportArtifacts)
                .HasForeignKey(item => item.AnalysisSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}