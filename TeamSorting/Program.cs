using Avalonia;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace TeamSorting;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        const string outputTemplate = "[{ProcessId}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
        var logPath = string.Empty;
        LoggerConfiguration loggerConfig = new LoggerConfiguration()
                                           .MinimumLevel.Debug()
                                           .WriteTo.Console(outputTemplate: outputTemplate)
                                           .Enrich.WithProcessId();

#if !DEBUG
        logPath = Path.Combine(Constants.LogDirectory, "TeamSorting.log");
        loggerConfig
            .WriteTo.File(path: logPath,
                restrictedToMinimumLevel: LogEventLevel.Information,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 1,
                shared: true,
                outputTemplate: outputTemplate);
#endif

        Log.Logger = loggerConfig.CreateLogger();

        Log.Information("Application starting");
        Log.Debug("Writing log to folder {path}", Path.GetDirectoryName(logPath));
        Log.Information("Version: {versionNumber}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown");
        Log.Information("System: {architecture} - {os}",
            System.Runtime.InteropServices.RuntimeInformation.OSArchitecture,
            System.Runtime.InteropServices.RuntimeInformation.OSDescription
        );

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Application terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current.Register<MaterialDesignIconProvider>();

        return AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .WithInterFont()
                         .LogToTrace();
    }
}