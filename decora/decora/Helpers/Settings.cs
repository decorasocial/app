using System;
using System.Collections.Generic;
using System.Text;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace decora.Helpers
{

  public static class Settings
{
        private static ISettings AppSettings =>
        CrossSettings.Current;

        public static bool IsCodeSet => AppSettings.Contains(App.config_code);

        public static string config_type
        {
            get => AppSettings.GetValueOrDefault(App.config_type, string.Empty);
            set => AppSettings.AddOrUpdateValue(App.config_type, value);
        }

        public static string config_code
        {
            get => AppSettings.GetValueOrDefault(App.config_code, string.Empty);
            set => AppSettings.AddOrUpdateValue(App.config_code, value);
        }

        public static string config_user
        {
            get => AppSettings.GetValueOrDefault(App.config_user, string.Empty);
            set => AppSettings.AddOrUpdateValue(App.config_user, value);
        }

        public static string config_image
        {
            get => AppSettings.GetValueOrDefault(App.config_image, string.Empty);
            set => AppSettings.AddOrUpdateValue(App.config_image, value);
        }

        public static string config_screen
        {
            get => AppSettings.GetValueOrDefault(App.config_screen, string.Empty);
            set => AppSettings.AddOrUpdateValue(App.config_screen, value);
        }

        public static string config_email
        {
            get => AppSettings.GetValueOrDefault(App.config_email, string.Empty);
            set => AppSettings.AddOrUpdateValue(App.config_email, value);
        }

        public static void deleteAll()
        {
            CrossSettings.Current.Remove(App.config_code);
            CrossSettings.Current.Remove(App.config_user);
            CrossSettings.Current.Remove(App.config_image);
            CrossSettings.Current.Remove(App.config_screen);
            CrossSettings.Current.Remove(App.config_email);
            CrossSettings.Current.Remove(App.config_type);
        }


    }
}
