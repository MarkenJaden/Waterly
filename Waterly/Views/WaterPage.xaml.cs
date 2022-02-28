﻿using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Toolkit;
using Waterly.Models;


namespace Waterly
{
    public sealed partial class WaterPage : Page
    {
        // ComboBox index conversion table
        private readonly int[] intervals = {
            10, 15, 20, 25, 30, 40, 50, 60, 75, 90, 105, 120, 150, 180, 240
        };


        public WaterPage()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                WaterAmountTextBlock.Text = App.User.Water.Amount.ToString("0' mL'");
                WaterBar.Value = App.User.Water.Amount;

                WaterTargetTextBlock.Text = App.User.Water.Target.ToString("'/ '0");
                WaterBar.Maximum = App.User.Water.Target;

                switch (App.Settings.NotificationSetting)
                {
                    case Settings.NotificationLevel.Disabled:
                        NotificationDisabledRadioButton.IsChecked = true;
                        break;

                    case Settings.NotificationLevel.Standard:
                        NotificationStandardRadioButton.IsChecked = true;
                        break;

                    case Settings.NotificationLevel.Alarm:
                        NotificationAlarmRadioButton.IsChecked = true;
                        break;
                }

                if (App.Settings.NotificationsEnabled)
                {
                    ReminderIntervalComboBox.IsEnabled = true;
                    ReminderIntervalTextBlock.Opacity = 1;
                }
                else
                {
                    ReminderIntervalComboBox.IsEnabled = false;
                    ReminderIntervalTextBlock.Opacity = 0.5;
                }

                ReminderIntervalComboBox.SelectedIndex = ConvertIntervalToIndex(App.User.Water.ReminderInterval);

                GlassSizeTextBox.Text = App.User.Water.GlassSize.ToString();
                RegisterDrinkAmountTextBox.Text = App.User.Water.GlassSize.ToString();

                // Attach UI Controls events to their handlers
                RegisterDrinkAmountTextBox.BeforeTextChanging += RegisterDrinkAmountTextBox_ValidateInput;
                RegisterDrinkAmountTextBox.KeyDown += TextBox_CheckEnter;
                RegisterDrinkAmountTextBox.LostFocus += RegisterDrinkAmountTextBox_Apply;
                RegisterDrinkButton.Click += RegisterDrinkButton_Clicked;
                NotificationDisabledRadioButton.Checked += NotificationsLevel_Changed;
                NotificationStandardRadioButton.Checked += NotificationsLevel_Changed;
                NotificationAlarmRadioButton.Checked += NotificationsLevel_Changed;
                ReminderIntervalComboBox.SelectionChanged += ReminderIntervalComboBox_SelectionChanged;
                GlassSizeTextBox.BeforeTextChanging += GlassSizeTextBox_ValidateInput;
                GlassSizeTextBox.KeyDown += TextBox_CheckEnter;
                GlassSizeTextBox.LostFocus += GlassSizeTextBox_Apply;

                // Hook up event delegates to the corresponding data-related events
                App.User.Water.WaterAmountChanged += OnWaterAmountChanged;
                Window.Current.SizeChanged += OnSizeChanged;

                // The first SizeChanged event is missed because it happens before Loaded
                // and so we trigger the function manually to adjust the window layout properly
                AdjustPageLayout(Window.Current.Bounds.Width, Window.Current.Bounds.Height);
            };

            Unloaded += (_, _) =>
            {
                // Disconnect all event handlers
                App.User.Water.WaterAmountChanged -= OnWaterAmountChanged;
                Window.Current.SizeChanged -= OnSizeChanged;

                RegisterDrinkAmountTextBox.BeforeTextChanging -= RegisterDrinkAmountTextBox_ValidateInput;
                RegisterDrinkAmountTextBox.KeyDown -= TextBox_CheckEnter;
                RegisterDrinkAmountTextBox.LostFocus -= RegisterDrinkAmountTextBox_Apply;
                RegisterDrinkButton.Click -= RegisterDrinkButton_Clicked;
                NotificationDisabledRadioButton.Checked -= NotificationsLevel_Changed;
                NotificationStandardRadioButton.Checked -= NotificationsLevel_Changed;
                NotificationAlarmRadioButton.Checked -= NotificationsLevel_Changed;
                ReminderIntervalComboBox.SelectionChanged -= ReminderIntervalComboBox_SelectionChanged;
                GlassSizeTextBox.BeforeTextChanging -= GlassSizeTextBox_ValidateInput;
                GlassSizeTextBox.KeyDown -= TextBox_CheckEnter;
                GlassSizeTextBox.LostFocus -= GlassSizeTextBox_Apply;
            };

        }


        private void OnSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            AdjustPageLayout(e.Size.Width, e.Size.Height);
        }

        private void AdjustPageLayout(double newWidth, double newHeight)
        {
            const double MIN_WIDTH_FOR_HORIZONTAL_LAYOUT = 680;

            // Adjust layout orientation based on window size
            if (RootPanel.Orientation == Orientation.Vertical)
            {
                if (newWidth >= MIN_WIDTH_FOR_HORIZONTAL_LAYOUT)
                {
                    RootPanel.Orientation = Orientation.Horizontal;
                    RootPanel.Margin = new()
                    {
                        Left = 32,
                        Top = 50,
                        Right = 0,
                        Bottom = 0
                    };
                    CircleGrid.Margin = new()
                    {
                        Left = 15,
                        Top = 0,
                        Right = 30,
                        Bottom = 10
                    };
                }
            }
            else    /* Orientation.Horizontal */
            {
                if (newWidth < MIN_WIDTH_FOR_HORIZONTAL_LAYOUT)
                {
                    RootPanel.Orientation = Orientation.Vertical;
                    CircleGrid.Margin = new()
                    {
                        Left = 15,
                        Top = 0,
                        Right = 30,
                        Bottom = 20
                    };
                    RootPanel.Margin = new()
                    {
                        Left = 60,
                        Top = 40,
                        Right = 0,
                        Bottom = 20
                    };
                }
            }
        }

        private void OnWaterAmountChanged(Water waterObj, EventArgs args)
        {
            WaterBar.Value = waterObj.Amount;
            WaterAmountTextBlock.Text = waterObj.Amount.ToString("0' mL'");
        }


        private void TextBox_CheckEnter(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Accept)
            {
                Focus(FocusState.Pointer);
            }
        }


        private void RegisterDrinkAmountTextBox_ValidateInput(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            // Only allow integer values
            args.Cancel = !(args.NewText.IsNumeric() || args.NewText.Length == 0);
        }

        private void RegisterDrinkAmountTextBox_Apply(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text.Length == 0)
            {
                textBox.Text = "0";
            }
            else if (int.Parse(textBox.Text) > 2000)
            {
                textBox.Text = "2000";
            }
        }


        private void RegisterDrinkButton_Clicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (RegisterDrinkAmountTextBox.Text.Length == 0)
                RegisterDrinkAmountTextBox.Text = "0";

            // Add the specified water amount to the current total
            var amount = int.Parse(RegisterDrinkAmountTextBox.Text);
            if (amount > 0)
            {
                App.User.Water.Amount += amount;
            }
            else
            {
                RegisterDrinkAmountTextBox.Text = "0";
            }
        }


        private void NotificationsLevel_Changed(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;

            double opacity;
            switch (radioButton.Tag)
            {
                case "off":
                    App.Settings.NotificationSetting = Settings.NotificationLevel.Disabled;
                    opacity = 0.5;
                    break;

                case "standard":
                    App.Settings.NotificationSetting = Settings.NotificationLevel.Standard;
                    opacity = 1;
                    break;

                case "alarm":
                    App.Settings.NotificationSetting = Settings.NotificationLevel.Alarm;
                    opacity = 1;
                    break;

                default:
                    throw new ApplicationException("Invalid RadioButon tag");
            }

            ReminderIntervalComboBox.IsEnabled = App.Settings.NotificationsEnabled;
            ReminderIntervalTextBlock.Opacity = opacity;
        }


        private void ReminderIntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            App.User.Water.ReminderInterval = intervals[comboBox.SelectedIndex];
        }

        private int ConvertIntervalToIndex(int value)
        {
            for (var i = 0; i < intervals.Length; i++)
            {
                if (intervals[i] == value)
                    return i;
            }

            // If the value doesn't fall in the range of ComboBox options, reset it to default
            App.User.Water.ReminderInterval = 30;
            return 4;
        }


        private void GlassSizeTextBox_ValidateInput(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            // Only allow integer values
            args.Cancel = !(args.NewText.IsNumeric() || args.NewText.Length == 0);
        }

        private void GlassSizeTextBox_Apply(object sender, RoutedEventArgs e)
        {
            if (GlassSizeTextBox.Text.Length == 0)
                GlassSizeTextBox.Text = "0";

            // Update the GlassSize with the value written in the TextBox
            var size = int.Parse(GlassSizeTextBox.Text);
            if (size > 0)
            {
                // Cap the value at 2000mL
                if (size > 2000)
                {
                    size = 2000;
                    GlassSizeTextBox.Text = "2000";
                }
                App.User.Water.GlassSize = size;
                RegisterDrinkAmountTextBox.Text = App.User.Water.GlassSize.ToString();
            }
            else
            {
                GlassSizeTextBox.Text = App.User.Water.GlassSize.ToString();
            }
        }

    }
}
