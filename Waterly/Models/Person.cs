using System;
using Windows.Storage;

namespace Waterly.Models
{
    public sealed class Person
    {
        // Delegate declaration
        public delegate void PersonChangedHandler(Person person, EventArgs args);

        // Event declaration
        public event PersonChangedHandler PersonChanged;


        public enum HealthStatusType
        {
            Undefined,
            Underweight,
            Healthy,
            Overweight,
            Obese,
            ExtremelyObese
        }

        public enum GenderType
        {
            Male,
            Female
        }

        private GenderType gender;
        /// <summary>
        /// The gender selected by the user in the BMI Calculator form
        /// </summary>
        public GenderType Gender { get => gender; set { gender = value; Save(); } }

        private int age;
        /// <summary>
        /// The age of the user, as specified in the BMI Calculator form
        /// </summary>
        public int Age { get => age; set { age = value; Save(); } }

        private float weight;
        /// <summary>
        /// The user's body weight, as specified in the BMI Calculator form
        /// </summary>
        public float Weight { get => weight; set { weight = value; Save(); } }

        private float height;
        /// <summary>
        /// The user's height, as specified in the BMI Calculator form
        /// </summary>
        public float Height { get => height; set { height = value; Save(); } }

        /// <summary>
        /// A metric representative of the user's body health status, based on the Body Mass Index value
        /// </summary>
        public HealthStatusType HealthStatus
        {
            get
            {
                var bmi = BodyMassIndex;
                if (bmi == 0.0f)
                    return HealthStatusType.Undefined;
                else if (bmi < 18.5)
                    return HealthStatusType.Underweight;
                else if (bmi < 25)
                    return HealthStatusType.Healthy;
                else if (bmi < 30)
                    return HealthStatusType.Overweight;
                else if (bmi < 40)
                    return HealthStatusType.Obese;
                else return HealthStatusType.ExtremelyObese;
            }
        }

        /// <summary>
        /// The Body Mass Index (BMI) value, computed using the user-provided body information
        /// </summary>
        public float BodyMassIndex { get => height > 0 ? weight / (height * height) : 0.0f; }

        /// <summary>
        /// The amount of water that a person should be drinking in a day, based on gender and age
        /// NOTE: not currently used, WaterTarget is hard-coded as 2000 mL
        /// </summary>
        public int WaterTarget
        {
            get
            {
                return age switch
                {
                    <= 3 => 1200,
                    <= 6 => 1600,
                    <= 10 => 1800,
                    <= 14 => (gender == GenderType.Male) ? 2100 : 1900,
                    _ => (gender == GenderType.Male) ? 2500 : 2000
                };
            }
        }


        /// <summary>
        /// Load user data from the local application storage.
        /// If one or more fields are not found, default values are loaded
        /// </summary>
        public void Load()
        {
            try
            {
                // Access local ApplicationDataContainer for settings
                var bodyInfo = (ApplicationDataCompositeValue)
                    ApplicationData.Current.LocalSettings.Values["BodyInformation"];

                if (bodyInfo != null)
                {
                    gender = (GenderType)bodyInfo["Gender"];
                    age = (int)bodyInfo["Age"];
                    weight = (float)bodyInfo["Weight"];
                    height = (float)bodyInfo["Height"];

                    // Fire the PersonChanged event to inform any listener about the loaded data
                    PersonChanged?.Invoke(this, EventArgs.Empty);

                    return;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }

            // Load default/null settings
            gender = GenderType.Male;
            age = 0;
            weight = 0.0f;
            height = 0.0f;

            // Write the loaded default settings and inform any listener of their values
            Save();
        }


        /// <summary>
        /// Write user data to the application's local storage
        /// </summary>
        public void Save()
        {
            try
            {
                // Save body information to local ApplicationDataContainer
                var bodyInfo = new ApplicationDataCompositeValue
                {
                    ["Gender"] = (int)gender,
                    ["Age"] = age,
                    ["Weight"] = weight,
                    ["Height"] = height
                };

                ApplicationData.Current.LocalSettings.Values["BodyInformation"] = bodyInfo;

                // Inform any listener that pending changes have been confirmed
                PersonChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}
