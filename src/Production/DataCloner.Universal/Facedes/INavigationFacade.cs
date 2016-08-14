namespace DataCloner.Universal.Facedes
{
    public interface INavigationFacade
    {
        /// <summary>
        /// Goes back to the previews view(s).
        /// </summary>
        /// <param name="steps">Number of views to go back.</param>
        void GoBack(int steps = 1);

        /// <summary>
        /// Navigates to the main view.
        /// </summary>
        void NavigateToMainPage();

        /// <summary>
        /// Navigates to the welcome view.
        /// </summary>
        void NavigateToWelcomePage();
    }
}