namespace EF10_NewFeaturesDbLibrary;

using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class LoggingCommandInterceptor : DbCommandInterceptor
{
    private readonly ILogger _logger;

    public LoggingCommandInterceptor(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("EFCustomInterceptor");
    }

    public override InterceptionResult<DbCommand> CommandCreating(
        CommandCorrelatedEventData eventData,
        InterceptionResult<DbCommand> result)
    {
        _logger.LogInformation("Creating command for context {Context}", eventData.Context?.GetType().Name);
        return base.CommandCreating(eventData, result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        _logger.LogInformation("Executing NonQuery: {CommandText}", command.CommandText);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing NonQueryAsync: {CommandText}", command.CommandText);
        return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        _logger.LogInformation("Executing Query: {CommandText}", command.CommandText);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing Query (Async): {CommandText}", command.CommandText);
        return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}

