using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Storage;

namespace TextPad.Model
{
    /// <summary>
    /// Represents the settings for the app and
    /// provides a data access mechanism for retrieving
    /// and storing the settings in the user's roaming profile.
    /// </summary>
    public sealed class Settings
    {
        private const string DefaultCharsetSettingName = "App.Settings.DefaultCharset";

        private Charset? defaultCharset_ = null;

        private Settings()
        {
        }

        /// <summary>
        /// Retrieve the settings from the user's roaming profile.
        /// </summary>
        /// <returns></returns>
        public static Settings Load()
        {
            var instance = new Settings();

            var settings_ = ApplicationData.Current.RoamingSettings;

            // load settings - a null value means the setting will have an appropriate default value

            var charset = settings_.Values[DefaultCharsetSettingName] as String;
            if (charset != null)
            {
                Charset parsed;
                if (Enum.TryParse<Charset>(charset, out parsed))
                    instance.defaultCharset_ = parsed;
            }

            return instance;
        }

        /// <summary>
        /// The default charset to use when a text file encoding is not recognized automatically.
        /// </summary>
        public Charset DefaultCharset
        {
            get { return defaultCharset_ ?? Charset.UTF8; }
            set { defaultCharset_ = value != Charset.UTF8 ? (Charset?)value : null; }
        }

        /// <summary>
        /// Store the settings in the user's roaming profile.
        /// </summary>
        public void Save()
        {
            var settings_ = ApplicationData.Current.RoamingSettings;

            if (defaultCharset_ == null)
                settings_.Values.Remove(DefaultCharsetSettingName);
            else
                settings_.Values[DefaultCharsetSettingName] = defaultCharset_.ToString();
        }
    }
}