using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using CoreQuant.Infrastructure.Persistence.EF.Interceptors;

namespace CoreQuant.Infrastructure.Persistence.EF.UnitTests;

/// <summary>
/// Proves physical deletes are impossible through the write registration interceptors:
/// <see cref="WriteDbContext"/> composed with <see cref="RejectPhysicalDeleteInterceptor"/>
/// rejects them, and the interceptor also guards plain contexts. Uses Sqlite as a stand-in
/// for wiring; MySQL behavior is proven separately.
/// </summary>
public sealed class PhysicalDeleteGuardTests
{
    [Fact]
    public async Task WriteDbContext_With_RejectInterceptor_Rejects_Physical_Delete()
    {
        await using var connection = OpenSharedConnection();
        var options = new DbContextOptionsBuilder<GuardWriteContext>()
            .UseSqlite(connection)
            .AddInterceptors(new RejectPhysicalDeleteInterceptor())
            .Options;
        await using var context = new GuardWriteContext(options);
        await context.Database.EnsureCreatedAsync();

        var record = new PlainRecord { Id = 1 };
        context.Records.Add(record);
        await context.SaveChangesAsync();

        context.Records.Remove(record);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Physical delete is forbidden", ex.Message);
    }

    [Fact]
    public async Task RejectInterceptor_Rejects_Physical_Delete_In_Pipeline()
    {
        await using var connection = OpenSharedConnection();
        var options = new DbContextOptionsBuilder<GuardPlainContext>()
            .UseSqlite(connection)
            .AddInterceptors(new RejectPhysicalDeleteInterceptor())
            .Options;
        await using var context = new GuardPlainContext(options);
        await context.Database.EnsureCreatedAsync();

        var record = new PlainRecord { Id = 1 };
        context.Records.Add(record);
        await context.SaveChangesAsync();

        context.Records.Remove(record);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Physical delete is forbidden", ex.Message);
    }

    private static SqliteConnection OpenSharedConnection()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }

    public sealed class PlainRecord
    {
        public int Id { get; set; }
    }

    private sealed class GuardWriteContext : WriteDbContext
    {
        public GuardWriteContext(DbContextOptions<GuardWriteContext> options)
            : base(options)
        {
        }

        public DbSet<PlainRecord> Records => Set<PlainRecord>();

        protected override System.Reflection.Assembly ConfigurationsAssembly =>
            typeof(GuardWriteContext).Assembly;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            modelBuilder.Entity<PlainRecord>(builder =>
            {
                builder.ToTable("PlainRecords");
                builder.HasKey(x => x.Id);
            });
        }
    }

    private sealed class GuardPlainContext : DbContext
    {
        public GuardPlainContext(DbContextOptions<GuardPlainContext> options)
            : base(options)
        {
        }

        public DbSet<PlainRecord> Records => Set<PlainRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            modelBuilder.Entity<PlainRecord>(builder =>
            {
                builder.ToTable("PlainRecords");
                builder.HasKey(x => x.Id);
            });
        }
    }
}
