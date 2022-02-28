﻿using System;
using System.Linq;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Waterly
{
    public sealed partial class MainPage : Page
    {
        // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
        private readonly Dictionary<string, Type> pages = new()
        {
            { "water", typeof(WaterPage) },
            { "person", typeof(PersonPage) },
            { "settings", typeof(SettingsPage) }
        };


        public MainPage()
        {
            InitializeComponent();

            ApplyRequestedColorTheme();

            Loaded += (_, _) =>
            {
                // Select the first page to be loaded in the content frame
                NavigationBar.SelectedItem = NavigationBar.MenuItems[1];

                // Attach handlers to application events
                App.Settings.ColorThemeChanged += OnColorThemeChanged;
            };

            Unloaded += (_, _) =>
            {
                // Disconnect event handlers
                App.Settings.ColorThemeChanged -= OnColorThemeChanged;
            };
        }


        // Handle navigation across multiple pages using the top navigation bar
        private void NavigationBar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                // Navigate to the settings page
                ContentFrame.Navigate(typeof(SettingsPage), null, args.RecommendedNavigationTransitionInfo);
            }
            else
            {
                var tag = args.SelectedItemContainer.Tag.ToString();
                var newPageType = pages.GetValueOrDefault(tag);

                // Get the page type before navigation to prevent duplicate entries in the backstack
                var prevPageType = ContentFrame.CurrentSourcePageType;

                // Only navigate if the selected page isn't currently loaded
                if (!(newPageType is null) && !Equals(prevPageType, newPageType))
                {
                    ContentFrame.Navigate(newPageType, null, args.RecommendedNavigationTransitionInfo);
                }
            }
        }

        private void NavigationBar_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }


        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavigationBar.IsBackEnabled = ContentFrame.CanGoBack &&
                ContentFrame.SourcePageType != typeof(WaterPage);

            if (ContentFrame.SourcePageType == typeof(SettingsPage))
            {
                // SettingsItem is not part of NavigationBar.MenuItems, and doesn't have a tag
                NavigationBar.SelectedItem = (NavigationViewItem)NavigationBar.SettingsItem;
            }
            else if (ContentFrame.SourcePageType != null)
            {
                var tag = pages.First(i => i.Value == e.SourcePageType).Key;

                NavigationBar.SelectedItem = NavigationBar.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals(tag));
            }
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new("Failed to load Page " + e.SourcePageType.FullName);
        }


        /// <summary>
        /// Settings.ColorThemeChanged event handler
        /// </summary>
        private void OnColorThemeChanged(ApplicationTheme theme, EventArgs args)
        {
            Application.Current.RequestedTheme = theme;
            ApplyRequestedColorTheme();
        }

        /// <summary>
        /// Applies the requested color theme to all window elements, including the title bar (without restarting the app)
        /// </summary>
        private void ApplyRequestedColorTheme()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ForegroundColor = (Resources["TitleBarForeground"] as SolidColorBrush).Color;
            titleBar.BackgroundColor = (Resources["NavigationViewTopPaneBackground"] as SolidColorBrush).Color;
            titleBar.ButtonForegroundColor = (Resources["TitleBarForeground"] as SolidColorBrush).Color;
            titleBar.ButtonBackgroundColor = (Resources["NavigationViewTopPaneBackground"] as SolidColorBrush).Color;
            titleBar.ButtonHoverForegroundColor = (Resources["TitleBarForeground"] as SolidColorBrush).Color;
            titleBar.ButtonHoverBackgroundColor = (Resources["TitleBarButtonHoverBackground"] as SolidColorBrush).Color;
            titleBar.InactiveForegroundColor = (Resources["TitleBarInactiveForeground"] as SolidColorBrush).Color;
            titleBar.InactiveBackgroundColor = (Resources["NavigationViewTopPaneBackground"] as SolidColorBrush).Color;
            titleBar.ButtonInactiveForegroundColor = (Resources["TitleBarInactiveForeground"] as SolidColorBrush).Color;
            titleBar.ButtonInactiveBackgroundColor = (Resources["NavigationViewTopPaneBackground"] as SolidColorBrush).Color;

            var theme = Application.Current.RequestedTheme == ApplicationTheme.Light ?
                ElementTheme.Light : ElementTheme.Dark;

            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.RequestedTheme = theme;
            }
        }
    }
}
