using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace CoreQuant.Infrastructure.Persistence.EF.Observability;
internal readonly record struct EfTarget(string Database, string Context)
{
    public static EfTarget From(DbConnection? connection, DbContext? context)
        => new(string.IsNullOrEmpty(connection?.Database) ? "unknown" : connection.Database, context?.GetType().Name ?? "unknown");

    public void AddTo(ref TagList tags)
    {
        tags.Add("db.namespace", Database);
        tags.Add("db.context", Context);
    }
}