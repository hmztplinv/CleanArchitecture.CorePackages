
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.MSSqlServer;

public class MsSqlLogger:LoggerServiceBase
{
    public MsSqlLogger(IConfiguration configuration)
    {
        MsSqlLogConfiguration msSqlLogConfiguration = configuration.GetSection("SerilogConfigurations:MsSqlLogConfiguration").Get<MsSqlLogConfiguration>() 
        ?? throw new Exception(SeriLogMessages.NullOptionsMessage);

        MSSqlServerSinkOptions sinkOptions = new()
        {
            TableName = msSqlLogConfiguration.TableName,
            AutoCreateSqlTable = msSqlLogConfiguration.AutoCreateSqlTable
        };

        ColumnOptions columnOptions = new();
        Logger seriLogConfiguration= new LoggerConfiguration().WriteTo.MSSqlServer(
            connectionString: msSqlLogConfiguration.ConnectionString,
            sinkOptions: sinkOptions,
            columnOptions: columnOptions)
            .CreateLogger();
        Logger = seriLogConfiguration;
    }
}