using Backend.Entities;
using MaterialDesignThemes.Wpf;
using SpotifySongTagger.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpotifySongTagger.Views
{
    public partial class PlaylistGenerator : UserControl
    {
        private PlaylistGeneratorViewModel ViewModel { get; }
        public PlaylistGenerator(ISnackbarMessageQueue messageQueue)
        {
            InitializeComponent();
            ViewModel = new PlaylistGeneratorViewModel(messageQueue);
            DataContext = ViewModel;
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e) => await ViewModel.Loaded();
        private void UserControl_Unloaded(object sender, RoutedEventArgs e) => ViewModel.Unloaded();


        #region drag & drop new nodes
        private void NodeType_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.SelectedGraphGeneratorPage == null) return;

            var frameworkElement = sender as FrameworkElement;
            var nodeType = frameworkElement.DataContext as NodeType;
            if (nodeType == null) return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(frameworkElement, nodeType, DragDropEffects.Copy);
                //Log.Information("NodeType_PreviewMouseDown");
            }
        }
        private void GraphEditor_Drop(object sender, DragEventArgs e)
        {
            if (ViewModel.SelectedGraphGeneratorPage == null) return;

            var nodeType = (NodeType)e.Data.GetData(typeof(NodeType));
            var pos = e.GetPosition(GraphEditor);
            ViewModel.GraphEditorVM.AddGraphNode(nodeType, pos);

            //Log.Information($"drop nodeType={nodeType.Name}");
            e.Handled = true;
        }
        #endregion

        #region GraphGeneratorPages dialogues
        private void AddGraphGeneratorPageDialog_Cancel(object sender, RoutedEventArgs e) => ViewModel.NewGraphGeneratorPageName = null;
        private void AddGraphGeneratorPageDialog_Add(object sender, RoutedEventArgs e) => ViewModel.AddGraphGeneratorPage();

        private void EditPageDialog_Cancel(object sender, RoutedEventArgs e)
        {
            ViewModel.NewGraphGeneratorPageName = null;
            EditIcons.SelectedItem = null;
        }
        private void EditPageDialog_Save(object sender, RoutedEventArgs e)
        {
            var ggp = EditIcons.SelectedItem as GraphGeneratorPage;
            ViewModel.EditGraphGeneratorPageName(ggp);
            EditIcons.SelectedItem = null;
        }

        private void DeletePageDialog_Delete(object sender, RoutedEventArgs e)
        {
            var ggp = DeleteIcons.SelectedItem as GraphGeneratorPage;
            ViewModel.RemoveGraphGeneratorPage(ggp);
            DeleteIcons.SelectedItem = null;
            ViewModel.NewGraphGeneratorPageName = null;
        }
        #endregion

        #region GraphGeneratorPages actions
        private void DeleteGraphGeneratorPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedItem == null) return;
            ViewModel.NewGraphGeneratorPageName = (listBox.SelectedItem as GraphGeneratorPage).Name;

            var enumerator = listBox.Resources.Values.GetEnumerator();
            enumerator.MoveNext();
            var dialog = enumerator.Current;
            DialogHost.OpenDialogCommand.Execute(dialog, listBox);
        }

        private void EditGraphGeneratorPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedItem == null) return;

            var enumerator = listBox.Resources.Values.GetEnumerator();
            enumerator.MoveNext();
            var dialog = enumerator.Current;

            var ggp = listBox.SelectedItem as GraphGeneratorPage;
            ViewModel.NewGraphGeneratorPageName = ggp.Name;
            DialogHost.OpenDialogCommand.Execute(dialog, listBox);
        }

        private async void PlayGraphGeneratorPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedItem == null) return;

            var ggp = listBox.SelectedItem as GraphGeneratorPage;
            listBox.SelectedItem = null;
            await ViewModel.Run(ggp);
        }

        private async void RunAll(object sender, RoutedEventArgs e) => await ViewModel.RunAll();
        #endregion

    }
}
