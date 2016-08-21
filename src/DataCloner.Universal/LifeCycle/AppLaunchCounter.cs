using Windows.Storage;

namespace DataCloner.Universal.LifeCycle
{
    /// <summary>
    /// Manages how often the app has been launched.
    /// </summary>
    public class AppLaunchCounter
    {
        private const string LaunchCountKey = "LAUNCH_COUNT";
        
        /// <summary>
        /// Determines whether the app has been launched for the first time.
        /// </summary>
        /// <returns>True, if launched for first time. Otherwise, false.</returns>
        public static bool IsFirstLaunch()
        {
            var value = ApplicationData.Current.LocalSettings.Values[LaunchCountKey];
            return value==null || (int)value <= 1;
        }

        /// <summary>
        /// Registers the app launch.
        /// </summary>
        public static void RegisterLaunch()
        {
            var value = ApplicationData.Current.LocalSettings.Values[LaunchCountKey];

            if (value == null)
                ApplicationData.Current.LocalSettings.Values[LaunchCountKey] = 1;
            else
            {
                var i = (int)value;
                ApplicationData.Current.LocalSettings.Values[LaunchCountKey] = ++i;
            }
        }
    }
}
