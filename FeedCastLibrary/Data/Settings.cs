/* 
    Copyright (c) 2011 Microsoft Corporation.  All rights reserved.
    Use of this sample source code is subject to the terms of the Microsoft license 
    agreement under which you licensed this sample source code and is provided AS-IS.
    If you did not accept the terms of the license agreement, you are not authorized 
    to use this sample source code.  For the terms of the license, please see the 
    license agreement between you and Microsoft.
  
    To see the MSDN article about this app, visit http://go.microsoft.com/fwlink/?LinkId=247592 
  
*/
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.IsolatedStorage;

namespace FeedCastLibrary
{

    // TODO move from ApplicationSettings to Isolated storage to improve performance
    public class Settings
    {
        // Isolated storage settings
        private static IsolatedStorageSettings _isolatedStore;

        // Isolated storage key names
        private const string WifiOnlySettingKeyName = "WifiOnlySetting";
        private const string InitialLaunchSettingKeyName = "InitialLaunchSetting";
        private const string FeaturedArticlesKeyName = "FeaturedArticles";
        private const string LastUpdatedTimeKeyName = "LastUpdatedTime";

        // Default values of our settings
        private const bool WifiOnlySettingDefault = false;
        private const bool InitialLaunchSettingDefault = true;
        private const Collection<Article> FeaturedArticlesDefault = null;
        private const string LastUpdatedDefault = null;

        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        public Settings()
        {
            try
            {
                // Get previous application settings.
                _isolatedStore = IsolatedStorageSettings.ApplicationSettings;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while using IsolatedStorageSettings: " + e.ToString());
            }
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (_isolatedStore.Contains(Key))
            {
                // If the value has changed
                if (_isolatedStore[Key] != value)
                {
                    // Store the new value
                    _isolatedStore[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                _isolatedStore.Add(Key, value);
                valueChanged = true;
            }

            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static valueType GetValueOrDefault<valueType>(string Key, valueType defaultValue)
        {
            valueType value;

            // If the key exists, retrieve the value.
            if (_isolatedStore.Contains(Key))
            {
                value = (valueType)_isolatedStore[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Save the settings.
        /// </summary>
        private static void Save()
        {
            _isolatedStore.Save();
        }


        /// <summary>
        /// Property for allowing downloading of feeds only through WiFi
        /// </summary>
        public static bool WifionlySetting
        {
            get
            {
                return GetValueOrDefault<bool>(WifiOnlySettingKeyName, WifiOnlySettingDefault);
            }
            set
            {
                AddOrUpdateValue(WifiOnlySettingKeyName, value);
                Save();
            }
        }

        /// <summary>
        /// Setting that determines whether application 
        /// </summary>
        public static bool InitialLaunchSetting
        {
            get
            {
                return GetValueOrDefault<bool>(InitialLaunchSettingKeyName, InitialLaunchSettingDefault);
            }
            set
            {
                AddOrUpdateValue(InitialLaunchSettingKeyName, value);
                Save();
            }
        }

        /// <summary>
        /// Setting storing the Featured Articles
        /// </summary>
        public static Collection<Article> FeaturedArticles
        {
            get
            {
                return GetValueOrDefault<Collection<Article>>(FeaturedArticlesKeyName, FeaturedArticlesDefault);
            }
            set
            {
                AddOrUpdateValue(FeaturedArticlesKeyName, value);
                Save();
            }
        }

        /// <summary>
        /// Setting storing the Last Updated Time
        /// </summary>
        public static string LastUpdatedTime
        {
            get
            {
                return GetValueOrDefault<string>(LastUpdatedTimeKeyName, LastUpdatedDefault);
            }
            set
            {
                AddOrUpdateValue(LastUpdatedTimeKeyName, value);
                Save();
            }
        }
    }
}

