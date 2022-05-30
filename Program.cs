using slackBot;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
try
{
    IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddHostedService<Worker>();

        })
        .UseSerilog()
        .Build();

    await host.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}