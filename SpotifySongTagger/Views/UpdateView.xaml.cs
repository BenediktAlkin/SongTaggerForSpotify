using SpotifySongTagger.ViewModels;
using System.Windows.Controls;

namespace SpotifySongTagger.Views
{
    public partial class UpdateView : UserControl
    {
        public UpdateViewModel ViewModel { get; set; }

        public UpdateView()
        {
            InitializeComponent();
            ViewModel = new UpdateViewModel();
            DataContext = ViewModel;
        }
    }
}
