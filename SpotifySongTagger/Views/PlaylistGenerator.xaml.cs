using MaterialDesignThemes.Wpf;
using Serilog;
using SpotifySongTagger.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpotifySongTagger.Views
{
    public partial class PlaylistGenerator : UserControl
    {
        private PlaylistGeneratorViewModel ViewModel { get; }
        public PlaylistGenerator()
        {
            InitializeComponent();
            ViewModel = new PlaylistGeneratorViewModel();
            DataContext = ViewModel;
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e) => await PlaylistGeneratorViewModel.Init();


        private void AddGraphGeneratorPageDialog_Cancel(object sender, RoutedEventArgs e) => ViewModel.NewGraphGeneratorPageName = null;
        private void AddGraphGeneratorPageDialog_Add(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ViewModel.NewGraphGeneratorPageName))
            {
                ViewModel.NewGraphGeneratorPageName = null;
                return;
            }
            ViewModel.AddGraphGeneratorPage(ViewModel.NewGraphGeneratorPageName);
            ViewModel.NewGraphGeneratorPageName = null;
        }



        #region drag & drop new nodes
        private void NewNode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.SelectedGraphGeneratorPageVM == null) return;
            if (ViewModel.SelectedNodeType == null) return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(sender as FrameworkElement, ViewModel.SelectedNodeType, DragDropEffects.Copy);
                Log.Information("NewNode_MouseDown");
            }
        }
        private void GraphEditor_Drop(object sender, DragEventArgs e)
        {
            if (ViewModel.SelectedGraphGeneratorPageVM == null) return;

            var nodeType = (NodeType)e.Data.GetData(typeof(NodeType));
            var pos = e.GetPosition(GraphEditor);
            ViewModel.GraphEditorVM.AddGraphNode(nodeType, pos);

            Log.Information($"Drop nodeType={nodeType.Name}");
            e.Handled = true;
        }
        #endregion


        private void DeleteGraphGeneratorPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedItem == null) return;

            var enumerator = listBox.Resources.Values.GetEnumerator();
            enumerator.MoveNext();
            var dialog = enumerator.Current;

            ViewModel.SelectedGraphGeneratorPageVM = listBox.SelectedItem as GraphGeneratorPageViewModel;

            DialogHost.OpenDialogCommand.Execute(dialog, listBox);

            listBox.SelectedItem = null;
        }

        private void EditGraphGeneratorPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedItem == null) return;

            var enumerator = listBox.Resources.Values.GetEnumerator();
            enumerator.MoveNext();
            var dialog = enumerator.Current;

            ViewModel.SelectedGraphGeneratorPageVM = listBox.SelectedItem as GraphGeneratorPageViewModel;
            ViewModel.NewGraphGeneratorPageName = ViewModel.SelectedGraphGeneratorPageVM.GraphGeneratorPage.Name;

            DialogHost.OpenDialogCommand.Execute(dialog, listBox);

            listBox.SelectedItem = null;
        }

        private async void PlayGraphGeneratorPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedItem == null) return;

            var pageVM = listBox.SelectedItem as GraphGeneratorPageViewModel;
            await pageVM.Run();
            listBox.SelectedItem = null;
        }

        private void EditPageDialog_Cancel(object sender, RoutedEventArgs e)
        {
            ViewModel.NewGraphGeneratorPageName = null;
        }

        private void EditPageDialog_Save(object sender, RoutedEventArgs e)
        {
            ViewModel.EditGraphGeneratorPageName();
            ViewModel.NewGraphGeneratorPageName = null;
        }

        private void DeletePageDialog_Delete(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveGraphGeneratorPage(ViewModel.SelectedGraphGeneratorPageVM);
            ViewModel.SelectedGraphGeneratorPageVM = null;
        }

        private async void RunAll(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsRunningAll) return;
            Log.Information("RunAll GraphGeneratorPages");
            ViewModel.IsRunningAll = true;
            foreach (var ggpVM in ViewModel.GraphGeneratorPageVMs)
                ggpVM.IsRunning = true;
            foreach (var ggpVM in ViewModel.GraphGeneratorPageVMs)
                await ggpVM.Run();
            ViewModel.IsRunningAll = false;
            Log.Information("Finished RunAll GraphGeneratorPages");
        }
    }
}
