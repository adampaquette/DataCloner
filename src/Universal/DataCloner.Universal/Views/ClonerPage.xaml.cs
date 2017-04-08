using DataCloner.Universal.ViewModels;
using Windows.UI.Xaml.Controls;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace DataCloner.Universal.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class ClonerPage : Page
    {
        private readonly ClonerPageViewModel _viewModel;

        public ClonerPage()
        {
            this.InitializeComponent();

            _viewModel = new ClonerPageViewModel();
        }
    }
}
