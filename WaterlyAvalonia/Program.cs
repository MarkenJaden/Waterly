using Avalonia;
using System;
using System.Threading.Tasks;
using Squirrel;

namespace Waterly
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static async Task Main(string[] args)
        {
            //Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            //    .CreateLogger();

            //Log.Information("--- Welcome to Waterly ---");

            SquirrelAwareApp.HandleEvents(
                onInitialInstall: OnAppInstall,
                onAppUninstall: OnAppUninstall,
                onEveryRun: OnAppRun);

            await UpdateMyApp();

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
        private static void OnAppInstall(SemanticVersion version, IAppTools tools)
        {
            tools.CreateShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop | ShortcutLocation.Startup);
        }

        private static void OnAppUninstall(SemanticVersion version, IAppTools tools)
        {
            tools.RemoveShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop | ShortcutLocation.Startup);
        }

        private static void OnAppRun(SemanticVersion version, IAppTools tools, bool firstRun)
        {
            tools.SetProcessAppUserModelId();
        }
        private static async Task UpdateMyApp()
        {
            using var mgr = new UpdateManager("https://waterly.nekos.lol/updates");
            
            if(!mgr.IsInstalledApp) return;

            var newVersion = await mgr.UpdateApp();

            //Log.Debug(mgr.AppDirectory);
            //Log.Debug(mgr.AppId);
            //Log.Debug(mgr.CheckForUpdate().Result.PackageDirectory);
            //Log.Debug(mgr.CurrentlyInstalledVersion().ToString());
            //Log.Debug(newVersion == null ? true.ToString() : false.ToString());

            // optionally restart the app automatically, or ask the user if/when they want to restart
            if (newVersion != null)
            {
                UpdateManager.RestartApp();
            }
        }
    }
}
