using FivePD.Server.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FivePD.Server.Database;

/// <summary>
/// A factory for creating scoped database contexts.
/// Provides a way to resolve a <see cref="DatabaseContext"/> within a dedicated <see cref="IServiceScope"/>.
/// </summary>
public class DatabaseContextScopeFactory : IDatabaseContextFactory
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseContextScopeFactory"/> class.
    /// </summary>
    /// <param name="scopeFactory">The <see cref="IServiceScopeFactory"/> to use.</param>
    public DatabaseContextScopeFactory(IServiceScopeFactory scopeFactory)
    {
        this._scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public IDatabaseContextScope CreateScope()
    {
        return new DatabaseContextScope(this._scopeFactory.CreateScope());
    }

#pragma warning disable S3260
#pragma warning disable S3881
    private sealed class DatabaseContextScope : IDatabaseContextScope
#pragma warning restore S3881
#pragma warning restore S3260
    {
        private readonly IServiceScope _scope;

        public DatabaseContextScope(IServiceScope scope)
        {
            this._scope = scope;
            this.Context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        }

        public DatabaseContext Context { get; }

        public void Dispose()
        {
            this._scope.Dispose();
        }
    }
}
