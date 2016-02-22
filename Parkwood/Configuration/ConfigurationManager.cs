namespace Parkwood.Configuration
{
    public static class ConfigurationManager
    {
        public static string Get(string settingName)
        {
            var setting = Windows.Storage.ApplicationData.Current.LocalSettings.Values[settingName];
            return setting == null ? string.Empty : setting.ToString();
        }

        public static void Set(string settingName, string value)
        {
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[settingName] = value;
        }
    }
}
