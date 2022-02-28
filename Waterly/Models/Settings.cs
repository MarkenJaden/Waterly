using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Waterly.Models
{
    class Settings
    {
        // Delegates declaration
        public delegate void NotificationsSettingChangedHandler(Settings settings, EventArgs args);
        public delegate void AutoStartupSettingChangedHandler(bool autoStartupEnabled, EventArgs args);
        public delegate void ColorThemeChangedHandler(ApplicationTheme theme, EventArgs args);

        // Events declaration
        public event NotificationsSettingChangedHandler NotificationsSettingChanged;
        public event AutoStartupSettingChangedHandler AutoStartupSettingChanged;
        public event ColorThemeChangedHandler ColorThemeChanged;


        private StartupTask startupTask = null;

        /// <summary>
        /// Represents whether the user has control over the StartupTaskState setting,
        /// or if the system has control over it and prevents it to be changed
        /// </summary>
        public bool CanToggleAutoStartup =>
            startupTask != null &&
            (startupTask.State == StartupTaskState.Enabled ||
             startupTask.State == StartupTaskState.Disabled);

        /// <summary>
        /// Provides additional information about the app's AutoStartup state
        /// </summary>
        public string AutoStartupStateDescription
        {
            get
            {
                var resources = ResourceLoader.GetForCurrentView();

                return startupTask == null
                    ? resources.GetString("ErrorString")
                    : startupTask.State switch
                    {
                        StartupTaskState.Enabled => string.Empty,
                        StartupTaskState.Disabled => string.Empty,
                        StartupTaskState.EnabledByPolicy => resources.GetString("StartupPolicyControlledString"),
                        StartupTaskState.DisabledByPolicy => resources.GetString("StartupPolicyControlledString"),
                        StartupTaskState.DisabledByUser => resources.GetString("NoStartupPermissionString"),
                        _ => throw new ApplicationException("Invalid startup task state")
                    };
            }
        }

        /// <summary>
        /// Check if the application has been set up to start automatically with Windows (on user logon)
        /// </summary>
        public bool AutoStartupEnabled => startupTask != null && startupTask.State.ToString().Contains("Enabled");

        /// <summary>
        /// Attempts to apply the requested AutoStartup setting to the StartupTask object,
        /// this operation may fail due to Windows policies and permission settings
        /// </summary>
        /// <param name="autoStartupEnabled">Specifies whether the AutoStartup setting has to be enabled or disabled</param>
        /// <returns>The AutoStartup setting value after the operation,
        /// it may differ from the autoStartupEnabled parameter if the operation has not been successful</returns>
        public void TryChangeAutoStartupSetting(bool autoStartupEnabled)
        {
            if (startupTask == null) return;
            if (autoStartupEnabled)             // Enable automatic startup with Windows
                TryEnableStartupTask();
            else TryDisableStartupTask();       // Disable scheduled execution at Windows startup
        }


        public enum NotificationLevel
        {
            Disabled,
            Standard,
            Alarm
        }

        private NotificationLevel notificationSetting;
        /// <summary>
        /// User setting to specify which type of desktop toast notifications, if any,
        /// the application is allowed to send as drink or sleep reminders
        /// </summary>
        public NotificationLevel NotificationSetting
        {
            get => notificationSetting;
            set
            {
                // Update setting only if different from the current value
                if (notificationSetting == value) return;
                ApplicationData.Current.LocalSettings.Values["NotificationsLevel"] = (int)value;
                notificationSetting = value;

                NotificationsSettingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Specifies whether toast notification reminders are enabled for the application
        /// </summary>
        public bool NotificationsEnabled => notificationSetting != NotificationLevel.Disabled;


        private readonly UISettings uiSettings = new();

        public enum ColorTheme
        {
            Light,
            Dark,
            System
        }

        private ColorTheme colorThemeSetting;
        /// <summary>
        /// User setting to specify whether the light or dark application theme has to be used,
        /// or if the app will simply follow the system theme (set by the user in Windows settings)
        /// </summary>
        public ColorTheme ColorThemeSetting
        {
            get => colorThemeSetting;
            set
            {
                // Update setting only if different from the current value
                if (colorThemeSetting == value) return;
                ApplicationData.Current.LocalSettings.Values["ColorTheme"] = (int)value;
                colorThemeSetting = value;

                ColorThemeChanged?.Invoke(RequestedApplicationTheme, EventArgs.Empty);
            }
        }

        private ApplicationTheme systemTheme;
        /// <summary>
        /// The requested ApplicationTheme (Light or Dark) which has been selected by the
        /// user (either in the app or via Windows settings)
        /// </summary>
        public ApplicationTheme RequestedApplicationTheme =>
            colorThemeSetting switch
            {
                ColorTheme.Light => ApplicationTheme.Light,
                ColorTheme.Dark => ApplicationTheme.Dark,
                ColorTheme.System => systemTheme,
                _ => throw new ApplicationException("Invalid color theme setting")
            };

        /// <summary>
        /// Handles color theme changes applied outside of the application (e.g. Windows settings)
        /// </summary>
        private async void SystemColorSettingsChanged(UISettings sender, object args)
        {
            var backgroundColor = sender.GetColorValue(UIColorType.Background);
            systemTheme = (backgroundColor == Colors.Black) ? 
                ApplicationTheme.Dark : ApplicationTheme.Light;

            // Update the current application colors if it's using the system theme
            if (ColorThemeSetting == ColorTheme.System)
                await CoreApplication.MainView.CoreWindow.Dispatcher
                    .RunAsync(CoreDispatcherPriority.Normal, InvokeColorThemeChangedHandlers);
        }

        /// <summary>
        /// Utility method for invoking the ColorThemeChanged event handler as a dispatched call
        /// </summary>
        private void InvokeColorThemeChangedHandlers()
        {
            ColorThemeChanged?.Invoke(RequestedApplicationTheme, EventArgs.Empty);
        }


        /// <summary>
        /// Load previously saved application settings locally from the device
        /// If one or more settings are not found, the default value is loaded
        /// </summary>
        public void LoadSettings()
        {
            // Get the application's StartupTask object
            try
            {
                startupTask = StartupTask.GetAsync("WaterDropsStartupId").AsTask().Result;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }

            // Store the initial application theme (which reflects the system theme)
            systemTheme = Application.Current.RequestedTheme;

            try
            {
                var LocalSettings = ApplicationData.Current.LocalSettings;

                // Load other settings from local application settings storage
                notificationSetting = LocalSettings.Values.TryGetValue("NotificationsLevel", out var value)
                    ? (NotificationLevel)value : NotificationLevel.Standard;

                colorThemeSetting = LocalSettings.Values.TryGetValue("ColorTheme", out value)
                    ? (ColorTheme)value : ColorTheme.System;
            }
            catch (Exception e)
            {
                // Default settings
                notificationSetting = NotificationLevel.Standard;
                colorThemeSetting = ColorTheme.System;

                Console.Error.WriteLine(e.Message);
            }

            // Attach SystemColorSettingsChanged handler to the UISettings event
            uiSettings.ColorValuesChanged += SystemColorSettingsChanged;
        }


        /// <summary>
        /// Write application settings to Windows.Storage.ApplicationData.Current.LocalSettings
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                var LocalSettings = ApplicationData.Current.LocalSettings;

                LocalSettings.Values["NotificationsLevel"] = notificationSetting;
                LocalSettings.Values["ColorTheme"] = colorThemeSetting;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }


        private async void TryEnableStartupTask()
        {
            if (startupTask.State != StartupTaskState.Disabled) return;
            // Task is disabled but can be enabled.
            await startupTask.RequestEnableAsync();

            AutoStartupSettingChanged?.Invoke(AutoStartupEnabled, EventArgs.Empty);
        }

        private void TryDisableStartupTask()
        {
            if (startupTask.State != StartupTaskState.Enabled) return;
            // Task is enabled but can be disabled.
            startupTask.Disable();

            AutoStartupSettingChanged?.Invoke(false, EventArgs.Empty);
        }
    }
}
