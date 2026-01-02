using System;
using System.Data;
using System.Data.SQLite;
using FivePD.Server.Database.Sql.Dialect;
using MySqlConnector;
using Serilog;

namespace FivePD.Server.Database;

#pragma warning disable

internal class ResilientConnection : IDbConnection
{
    private readonly IDbConnection _inner;
    private readonly ILogger _logger;

    public ResilientConnection(IDbConnection inner, ILogger logger)
    {
        this._inner = inner ?? throw new ArgumentNullException(nameof(inner));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private void Wrap(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception) when (this.IsDatabaseUnavailable(exception))
        {
            this.HandleGlobalFailure(exception);
            throw;
        }
    }

    private T Wrap<T>(Func<T> func)
    {
        try
        {
            return func();
        }
        catch (Exception exception) when (this.IsDatabaseUnavailable(exception))
        {
            this.HandleGlobalFailure(exception);
            throw;
        }
    }

    private bool IsDatabaseUnavailable(Exception exception)
    {
        return exception is MySqlException
               || exception.InnerException is MySqlException
               || exception is SQLiteException
               || exception.InnerException is SQLiteException;
    }

    private void HandleGlobalFailure(Exception exception)
    {
        var message = SqlDialectRegistry.Type == DatabaseType.SQLite
            ? "SQLite database error. Make sure the database file is accessible and not corrupted."
            : "Database connection error. Make sure your database is running and configured correctly.";
        this._logger.Fatal(message);
        this._logger.Fatal("Exception: {exception}", exception.InnerException ?? exception);
        LifecycleHandler.Stop();
    }

    public string ConnectionString
    {
        get => this._inner.ConnectionString;
        set => this._inner.ConnectionString = value;
    }

    public int ConnectionTimeout => this._inner.ConnectionTimeout;

    public string Database => this._inner.Database;

    public ConnectionState State => this._inner.State;

    public IDbTransaction BeginTransaction() => this.Wrap(() => this._inner.BeginTransaction());

    public IDbTransaction BeginTransaction(IsolationLevel il) => this.Wrap(() => this._inner.BeginTransaction(il));

    public void Open() => this.Wrap(() => this._inner.Open());

    public void Close() => this._inner.Close();

    public void ChangeDatabase(string databaseName) => this._inner.ChangeDatabase(databaseName);

    public IDbCommand CreateCommand()
    {
        return new ResilientCommand(this._inner.CreateCommand(), this.IsDatabaseUnavailable, this.HandleGlobalFailure);
    }

    public void Dispose() => this._inner.Dispose();
}

internal class ResilientCommand : IDbCommand
{
    private readonly IDbCommand _inner;
    private readonly Func<Exception, bool> _isDatabaseUnavailable;
    private readonly Action<Exception> _onFailure;

    public ResilientCommand(IDbCommand inner, Func<Exception, bool> isDatabaseUnavailable, Action<Exception> onFailure)
    {
        this._inner = inner;
        this._isDatabaseUnavailable = isDatabaseUnavailable;
        this._onFailure = onFailure;
    }

    private T Wrap<T>(Func<T> func)
    {
        try
        {
            return func();
        }
        catch (Exception exception) when (this._isDatabaseUnavailable(exception))
        {
            this._onFailure(exception);
            throw;
        }
    }

    public IDataReader ExecuteReader() => this.Wrap(() => this._inner.ExecuteReader());

    public IDataReader ExecuteReader(CommandBehavior behavior) => this.Wrap(() => this._inner.ExecuteReader(behavior));

    public object ExecuteScalar() => this.Wrap(() => this._inner.ExecuteScalar());

    public int ExecuteNonQuery() => this.Wrap(() => this._inner.ExecuteNonQuery());

    public IDbConnection Connection
    {
        get => this._inner.Connection;
        set => this._inner.Connection = value;
    }

    public IDbTransaction Transaction
    {
        get => this._inner.Transaction;
        set => this._inner.Transaction = value;
    }

    public string CommandText
    {
        get => this._inner.CommandText;
        set => this._inner.CommandText = value;
    }

    public int CommandTimeout
    {
        get => this._inner.CommandTimeout;
        set => this._inner.CommandTimeout = value;
    }

    public CommandType CommandType
    {
        get => this._inner.CommandType;
        set => this._inner.CommandType = value;
    }

    public IDataParameterCollection Parameters => this._inner.Parameters;

    public UpdateRowSource UpdatedRowSource
    {
        get => this._inner.UpdatedRowSource;
        set => this._inner.UpdatedRowSource = value;
    }

    public void Prepare() => this._inner.Prepare();

    public void Cancel() => this._inner.Cancel();

    public IDbDataParameter CreateParameter() => this._inner.CreateParameter();

    public void Dispose() => this._inner.Dispose();
}