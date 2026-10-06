namespace ChatApp.Data;

using ChatApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<PrekeyBundle> PrekeyBundles => Set<PrekeyBundle>();
    public DbSet<OneTimePrekey> OneTimePrekeys => Set<OneTimePrekey>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<CallSession> CallSessions => Set<CallSession>();
    public DbSet<CallParticipant> CallParticipants => Set<CallParticipant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("chatapp");

        // ===== User =====
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Email).HasColumnName("email").HasMaxLength(255);
            e.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(150);
            e.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            e.Property(u => u.ProfilePic).HasColumnName("profile_pic");
            e.Property(u => u.CreatedAt).HasColumnName("created_at");
            e.Property(u => u.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(u => u.Email).IsUnique();
        });

        // ===== Message =====
        modelBuilder.Entity<Message>(e =>
        {
            e.ToTable("messages");
            e.Property(m => m.Id).HasColumnName("id");
            e.Property(m => m.SenderId).HasColumnName("sender_id");
            e.Property(m => m.ReceiverId).HasColumnName("receiver_id");
            e.Property(m => m.Text).HasColumnName("text");
            e.Property(m => m.ImageUrl).HasColumnName("image_url");
            e.Property(m => m.CreatedAt).HasColumnName("created_at");
            e.Property(m => m.UpdatedAt).HasColumnName("updated_at");

            e.HasOne(m => m.Sender)
             .WithMany(u => u.SentMessages)
             .HasForeignKey(m => m.SenderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Receiver)
             .WithMany(u => u.ReceivedMessages)
             .HasForeignKey(m => m.ReceiverId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(m => new { m.SenderId, m.ReceiverId, m.CreatedAt })
             .IsDescending(false, false, true);
            e.HasIndex(m => new { m.ReceiverId, m.SenderId, m.CreatedAt })
             .IsDescending(false, false, true);
        });

        // ===== PrekeyBundle =====
        modelBuilder.Entity<PrekeyBundle>(e =>
        {
            e.ToTable("prekey_bundles");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.UserId).HasColumnName("user_id");
            e.Property(p => p.IdentityPublicKey).HasColumnName("identity_public_key");
            e.Property(p => p.SignedPrekey).HasColumnName("signed_prekey");
            e.Property(p => p.SignedPrekeySignature).HasColumnName("signed_prekey_signature");
            e.Property(p => p.SignedPrekeyId).HasColumnName("signed_prekey_id");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");

            e.HasOne(p => p.User)
             .WithMany()
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(p => p.UserId).IsUnique();
        });

        // ===== OneTimePrekey =====
        modelBuilder.Entity<OneTimePrekey>(e =>
        {
            e.ToTable("one_time_prekeys");
            e.Property(o => o.Id).HasColumnName("id");
            e.Property(o => o.BundleId).HasColumnName("bundle_id");
            e.Property(o => o.KeyId).HasColumnName("key_id");
            e.Property(o => o.PublicKey).HasColumnName("public_key");
            e.Property(o => o.IsConsumed).HasColumnName("is_consumed").HasDefaultValue(false);
            e.Property(o => o.ConsumedAt).HasColumnName("consumed_at");

            e.HasOne(o => o.Bundle)
             .WithMany(b => b.OneTimePrekeys)
             .HasForeignKey(o => o.BundleId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(o => o.BundleId);
            e.HasIndex(o => new { o.BundleId, o.IsConsumed })
             .HasFilter("NOT is_consumed");
        });

        // ===== DeviceToken =====
        modelBuilder.Entity<DeviceToken>(e =>
        {
            e.ToTable("device_tokens");
            e.Property(d => d.Id).HasColumnName("id");
            e.Property(d => d.UserId).HasColumnName("user_id");
            e.Property(d => d.Token).HasColumnName("token");
            e.Property(d => d.Platform).HasColumnName("platform").HasMaxLength(20);
            e.Property(d => d.LastActive).HasColumnName("last_active");

            e.HasOne(d => d.User)
             .WithMany()
             .HasForeignKey(d => d.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(d => d.UserId);
            e.HasIndex(d => new { d.UserId, d.Token }).IsUnique();
        });

        // ===== CallSession =====
        modelBuilder.Entity<CallSession>(e =>
        {
            e.ToTable("call_sessions");
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.CallerId).HasColumnName("caller_id");
            e.Property(c => c.CallType).HasColumnName("call_type").HasMaxLength(20);
            e.Property(c => c.Status).HasColumnName("status").HasMaxLength(30);
            e.Property(c => c.StartedAt).HasColumnName("started_at");
            e.Property(c => c.EndedAt).HasColumnName("ended_at");

            e.HasOne(c => c.Caller)
             .WithMany()
             .HasForeignKey(c => c.CallerId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(c => new { c.CallerId, c.StartedAt })
             .IsDescending(false, true);
        });

        // ===== CallParticipant =====
        modelBuilder.Entity<CallParticipant>(e =>
        {
            e.ToTable("call_participants");
            e.Property(cp => cp.Id).HasColumnName("id");
            e.Property(cp => cp.CallId).HasColumnName("call_id");
            e.Property(cp => cp.UserId).HasColumnName("user_id");
            e.Property(cp => cp.ResponseStatus).HasColumnName("response_status").HasMaxLength(20);
            e.Property(cp => cp.JoinedAt).HasColumnName("joined_at");

            e.HasOne(cp => cp.Call)
             .WithMany(c => c.Participants)
             .HasForeignKey(cp => cp.CallId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(cp => cp.User)
             .WithMany()
             .HasForeignKey(cp => cp.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(cp => cp.UserId);
        });
    }
}
