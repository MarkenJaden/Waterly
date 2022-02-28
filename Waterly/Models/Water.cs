using System;
using System.Globalization;
using Windows.Storage;

namespace Waterly.Models
{

    public class WaterAmountEventArgs : EventArgs
    {
        public int DeltaAmount { get; }

        public WaterAmountEventArgs(int amount)
        {
            DeltaAmount = amount;
        }
    }

    public class WaterSettingsEventArgs : EventArgs
    {
        public bool RescheduleTime { get; }

        /// <param name="rescheduleTime">Whether the scheduled time of previous reminders has to be recalculated
        /// (e.g. when ReminderInterval is changed)</param>
        public WaterSettingsEventArgs(bool rescheduleTime)
        {
            RescheduleTime = rescheduleTime;
        }
    }


    public class Water
    {
        // Delegate declarations
        public delegate void WaterAmountChangedHandler(Water water, WaterAmountEventArgs args);
        public delegate void WaterSettingsChangedHandler(Water water, WaterSettingsEventArgs args);

        // Event declarations
        public event WaterAmountChangedHandler WaterAmountChanged;
        public event WaterSettingsChangedHandler WaterSettingsChanged;


        private const int DefaultReminderInterval = 30;
        private const int DefaultReminderDelay = 5;
        private const int DefaultGlassSize = 250;
        private const int DefaultWaterAmount = 0;
        private const int DefaultWaterTarget = 2000;
        

        /// <summary>
        /// The last time at which the water amount value has been updated
        /// </summary>
        public DateTime Timestamp { get; private set; }


        private int amount = DefaultWaterAmount;
        /// <summary>
        /// The amount of water drank by the user in the current day, 
        /// written in the application's LocalSettings storage
        /// </summary>
        public int Amount
        {
            get => amount;
            set
            {
                var delta = value - amount;

                amount = value;
                Save();

                // Emit the event to inform any listener of the updated value
                WaterAmountChanged?.Invoke(this, new(delta));
            }
        }

        private int target = DefaultWaterTarget;
        /// <summary>
        /// How much water the user has to drink throughout the day
        /// </summary>
        public int Target 
        { 
            get => target; 
            set
            {
                if (value is > 0 and <= 10000)
                {
                    target = value;
                    Save();

                    // Emit the event to inform any listener of the updated settings
                    WaterSettingsChanged?.Invoke(this, new(false));
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value), @"Valid WaterTarget values range from 1 to 10000 mL");
                }
            }
        }

        private int glassSize = DefaultGlassSize;
        /// <summary>
        /// The amount of water (in mL) contained in a glass
        /// </summary>
        public int GlassSize
        {
            get => glassSize;
            set
            {
                if (value is > 0 and <= 2000)
                {
                    glassSize = value;
                    Save();

                    // Emit the event to inform any listener of the updated settings
                    WaterSettingsChanged?.Invoke(this, new(false));
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value), @"Valid GlassSize values range from 1 to 2000 mL");
                }
            }
        }

        private int reminderInterval = DefaultReminderInterval;
        /// <summary>
        /// The time (in minutes) used to schedule periodic drink reminders
        /// </summary>
        public int ReminderInterval
        {
            get => reminderInterval;
            set
            {
                if (value is > 0 and <= 1440)
                {
                    reminderInterval = value;
                    Save();

                    // Emit the event to inform any listener of the updated settings
                    WaterSettingsChanged?.Invoke(this, new(true));
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value), @"Valid ReminderInterval values range from 1 to 1440 minutes");
                }
            }
        }

        private int reminderDelay = DefaultReminderDelay;
        /// <summary>
        /// The time (in minutes) by how much the drink reminder is postponed
        /// </summary>
        public int ReminderDelay
        {
            get => reminderDelay;
            set
            {
                if (value is > 0 and <= 720)
                {
                    reminderDelay = value;
                    Save();

                    // Emit the event to inform any listener of the updated settings
                    WaterSettingsChanged?.Invoke(this, new(true));
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value), @"Valid ReminderDelay values range from 1 to 720 minutes");
                }
            }
        }


        /// <summary>
        /// Load the latest water amount (for the current day) and preferences from the application's LocalSettings
        /// </summary>
        public void Load()
        {
            try
            {
                var container = (ApplicationDataCompositeValue)
                    ApplicationData.Current.LocalSettings.Values["Water"];

                // Load values from LocalSettings (if not available default to 0)
                if (container != null)
                {
                    reminderInterval = ((int?)container["ReminderInterval"]).Value;
                    reminderDelay = ((int?)container["ReminderDelay"]).Value;
                    glassSize = ((int?)container["GlassSize"]).Value;
                    target = ((int?)container["Target"]).Value;
                    amount = ((int?)container["Amount"]).Value;
                    Timestamp = DateTime.ParseExact(container["Timestamp"] as string, "O",
                        CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

                    // Reset the values if they refer to a previous day
                    var today = DateTime.UtcNow;
                    if (today.Date > Timestamp.Date)
                    {
                        amount = DefaultWaterAmount;
                        Timestamp = today;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);

                // Load default settings and values
                reminderInterval = DefaultReminderInterval;
                reminderDelay = DefaultReminderDelay;
                glassSize = DefaultGlassSize;
                amount = DefaultWaterAmount;
                target = DefaultWaterTarget;
                Timestamp = DateTime.UtcNow;
            }

            // Emit events to inform any listener of the updated value and settings
            WaterAmountChanged?.Invoke(this, new(amount));
            WaterSettingsChanged?.Invoke(this, new(true));
        }


        /// <summary>
        /// Write the current water amount and preferences (along with a timestamp) to the application's LocalSettings
        /// </summary>
        public void Save()
        {
            // Update the timestamp
            Timestamp = DateTime.UtcNow;

            try
            {
                var water = new ApplicationDataCompositeValue()
                {
                    ["ReminderInterval"] = reminderInterval,
                    ["ReminderDelay"] = reminderDelay,
                    ["GlassSize"] = glassSize,
                    ["Target"] = target,
                    ["Amount"] = amount,
                    ["Timestamp"] = Timestamp.ToString("O", CultureInfo.InvariantCulture)
                };

                // Save the value in the local settings storage
                ApplicationData.Current.LocalSettings.Values["Water"] = water;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}
