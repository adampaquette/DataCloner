using DataCloner.Universal.Commands;
using Windows.UI.Xaml.Media;

namespace DataCloner.Universal.Menu
{
    /// <summary>
    /// Defines a menu item for the navigation bar.
    /// </summary>
    public interface IMenuItem
    {
        /// <summary>
        /// Gets action to execute.
        /// </summary>
        RelayCommand Command { get; }

        /// <summary>
        /// Gets the image that is displayed in the navigation bar.
        /// </summary>
        ImageSource Image { get; }

        /// <summary>
        /// Gets the title displayed in the navigation bar.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Gets the positions of the current item in the navigation bar.
        /// </summary>
        MenuItemPosition Position { get; }

        /// <summary>
        /// Gets the location of the current item in the navigation bar.
        /// </summary>
        MenuItemLocation Location { get; }
    }
}
