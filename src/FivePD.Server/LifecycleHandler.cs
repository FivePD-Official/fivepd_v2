using System;
using MySql.Data.MySqlClient;
using Serilog;

namespace FivePD.Server;

#pragma warning disable

internal static class LifecycleHandler
{
    private static ILogger logger;

    public static bool IsStopped;

    internal static void SetLogger(ILogger logger)
    {
        LifecycleHandler.logger = logger;
    }

    public static event EventHandler OnStopped;

    public static void HandleError(Exception exception)
    {
        if (exception is MySqlException || exception.InnerException is MySqlException)
        {
            logger.Fatal("Database connection error. Please make sure your database is running and configured correctly.");
            OnStopped?.Invoke(null, EventArgs.Empty);
            IsStopped = true;
        }
    }
}