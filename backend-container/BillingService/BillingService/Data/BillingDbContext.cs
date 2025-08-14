using Microsoft.EntityFrameworkCore;
using BillingService.Models;

namespace BillingService.Data;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<SubscriptionContract> SubscriptionContracts => Set<SubscriptionContract>();
    public DbSet<PaymentIntent> PaymentIntents => Set<PaymentIntent>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<AppointmentPayment> AppointmentPayments => Set<AppointmentPayment>();
    public DbSet<SubscriptionPayment> SubscriptionPayments => Set<SubscriptionPayment>();
    public DbSet<PspWebhookEvent> PspWebhookEvents => Set<PspWebhookEvent>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("billing");

        const string SqlGuid = "CONVERT(VARCHAR(36), NEWID())";
        const string SysUtc = "SYSUTCDATETIME()";

        modelBuilder.Entity<PaymentMethod>(e =>
        {
            e.Property(p => p.Id).HasDefaultValueSql(SqlGuid);
            e.HasIndex(p => new { p.PatientId, p.IsDefault });
            e.Property(p => p.CreatedAt).HasDefaultValueSql(SysUtc);
        });
        modelBuilder.Entity<SubscriptionContract>(e =>
        {
            e.Property(p => p.Id).HasDefaultValueSql(SqlGuid);
            e.HasOne(p => p.DefaultPaymentMethod).WithMany().HasForeignKey(p => p.DefaultPaymentMethodId).OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<PaymentIntent>(e =>
        {
            e.Property(p => p.Id).HasDefaultValueSql(SqlGuid);
            e.Property(p => p.CreatedAt).HasDefaultValueSql(SysUtc);
            e.HasIndex(p => new { p.PatientId, p.Kind, p.Status });
        });
        modelBuilder.Entity<PaymentTransaction>(e =>
        {
            e.Property(p => p.Id).HasDefaultValueSql(SqlGuid);
            e.Property(p => p.OccurredAt).HasDefaultValueSql(SysUtc);
            e.HasIndex(p => p.PaymentIntentId);
        });
        modelBuilder.Entity<AppointmentPayment>(e =>
        {
            e.Property(p => p.Id).HasDefaultValueSql(SqlGuid);
            e.HasIndex(p => new { p.AppointmentId });
        });
        modelBuilder.Entity<SubscriptionPayment>(e =>
        {
            e.Property(p => p.Id).HasDefaultValueSql(SqlGuid);
            e.HasIndex(p => new { p.SubscriptionContractId, p.PeriodStart });
        });
        modelBuilder.Entity<PspWebhookEvent>(e =>
        {
            e.HasKey(p => new { p.Id, p.Provider });
            e.Property(p => p.ReceivedAt).HasDefaultValueSql(SysUtc);
        });
        modelBuilder.Entity<OutboxEvent>(e =>
        {
            e.Property(p => p.Id).HasDefaultValueSql(SqlGuid);
            e.Property(p => p.OccurredAt).HasDefaultValueSql(SysUtc);
            e.HasIndex(p => p.PublishedAt);
        });

        // Views as keyless if not materialized
        modelBuilder.Entity<PatientBillingSummary>().HasNoKey().ToView("vw_Patient_Billing_Summary", "billing");
        modelBuilder.Entity<DoctorRevenueDashboard>().HasNoKey().ToView("vw_Doctor_Revenue_Dashboard", "billing");
    }
}
