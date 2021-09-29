using Backend;
using Backend.Entities.GraphNodes;
using Serilog;
using SpotifySongTagger.Utils;
using SpotifySongTagger.ViewModels;
using SpotifySongTagger.ViewModels.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpotifySongTagger.Views.Controls
{
    public partial class GraphEditor : UserControl
    {
        private GraphEditorViewModel ViewModel => DataContext as GraphEditorViewModel;
        public GraphEditor()
        {
            InitializeComponent();
            // DataContext is set from outside because the DataContext changes when another Page is selected
        }

        private async void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateCanvasSize();
            if(ViewModel != null)
            {
                ViewModel.ClearAllInputResults();
                await ViewModel.RefreshInputResults();
            }
            await BaseViewModel.DataContainer.LoadTags();
        }


        #region move elements in canvas
        private void FrameworkElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Canvas.Focus();
            if (ViewModel == null) return;

            Selectable_MouseDown(sender, e);

            var frameworkElement = sender as FrameworkElement;
            ViewModel.ClickedNodeViewModel = frameworkElement.DataContext as GraphNodeViewModel;
            var curPos = e.GetPosition(Canvas);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // initialize moving GraphNode
                ViewModel.LastMousePos = curPos;
                ViewModel.PressedMouseButton = MouseButton.Left;
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                // initialize arrow
                ViewModel.PressedMouseButton = MouseButton.Right;
                var contentPresenter = UIHelper.FindVisualParent<ContentPresenter>(frameworkElement);
                var startX = Canvas.GetLeft(contentPresenter) + contentPresenter.ActualWidth;
                var startY = Canvas.GetTop(contentPresenter) + contentPresenter.ActualHeight / 2;
                ViewModel.NewArrowStartPoint = new Point(startX, startY);
            }

            e.Handled = true;
            Log.Information($"MouseDown {sender.GetType()} {ViewModel.PressedMouseButton} {curPos.X:N2}/{curPos.Y:N2}");
        }
        private async void FrameworkElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null) return;

            if (ViewModel.PressedMouseButton == MouseButton.Left)
            {
                // stop moving node
                Log.Information($"StopMovingNode {sender.GetType()}");
                if (ViewModel.ClickedNodeViewModel == null) return;

                ViewModel.SaveMovedGraphNodePosition();
                ViewModel.ClickedNodeViewModel = null;
                ViewModel.LastMousePos = default;
                e.Handled = true;
            }
            if (ViewModel.PressedMouseButton == MouseButton.Right)
            {
                // add connection
                var nodeVM = ViewModel.GetHoveredGraphNodeViewModel(e.GetPosition(Canvas));
                await ViewModel.AddConnection(nodeVM);
                ViewModel.NewArrowStartPoint = default;
                ViewModel.NewArrow = null;
            }
            ViewModel.PressedMouseButton = null;
            e.Handled = true;
        }
        private void FrameworkElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (ViewModel == null || ViewModel.PressedMouseButton == null) return;

            var curPos = e.GetPosition(Canvas);
            if (ViewModel.PressedMouseButton == MouseButton.Left)
            {
                // move node
                var delta = curPos - ViewModel.LastMousePos;
                ViewModel.ClickedNodeViewModel.X += delta.X;
                ViewModel.ClickedNodeViewModel.Y += delta.Y;
                ViewModel.LastMousePos = curPos;
            }
            if (ViewModel.PressedMouseButton == MouseButton.Right)
            {
                // draw arrow
                ViewModel.NewArrow = GeometryUtil.GetArrow(ViewModel.NewArrowStartPoint, curPos);
            }

            e.Handled = true;
            //Log.Information($"MouseMove {sender.GetType()} {curPos.X:N2}/{curPos.Y:N2}");
        }
        #endregion


        #region track size of nodes
        private void Node_Loaded(object sender, EventArgs e)
        {
            UpdateSize(sender);
        }
        private void Node_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize(sender);
        }
        private static void UpdateSize(object sender)
        {
            var frameworkElement = sender as FrameworkElement;
            var nodeVM = frameworkElement.DataContext as GraphNodeViewModel;
            nodeVM.Width = frameworkElement.ActualWidth;
            nodeVM.Height = frameworkElement.ActualHeight;
        }
        #endregion


        private void Selectable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Canvas.Focus();
            var frameworkElement = sender as FrameworkElement;
            var selectable = frameworkElement.DataContext as ISelectable;
            ViewModel.SelectedObject = selectable;
            e.Handled = true;
        }
        private void UnselectAll(object sender, MouseButtonEventArgs e)
        {
            ViewModel.SelectedObject = null;
        }


        private async void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            // canvas has to be in focus for this event to fire
            if (e.Key == Key.Escape)
                ViewModel.SelectedObject = null;

            if (e.Key == Key.Delete || e.Key == Key.Return)
                await ViewModel.DeleteSelected();
        }

        private void PlaylistOutputNodeName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // if validation gives an error for NewTagName, it is not updated in the ViewModel
            var textBox = sender as TextBox;
            var frameworkElement = sender as FrameworkElement;
            var outputNode = frameworkElement.DataContext as PlaylistOutputNode;

            outputNode.PlaylistName = textBox.Text;
            ConnectionManager.Instance.Database.SaveChanges();
        }

        #region UpdateCanvasSize
        private void UpdateCanvasSize_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateCanvasSize();
        private void UpdateCanvasSize()
        {
            if (ViewModel == null) return;
            ViewModel.UpdateCanvasSize(Canvas.ActualWidth, Canvas.ActualHeight);
        }
        #endregion

        private async void UpdateGraphNode(object sender, SelectionChangedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            var from = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
            var to = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            ConnectionManager.Instance.Database.SaveChanges();
            Log.Information($"{frameworkElement.DataContext} changed from {from} to {to}");
            await ViewModel.RefreshInputResults();
        }

        private void SwapRemoveNodeInputs(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            var removeNode = frameworkElement.DataContext as RemoveNode;
            removeNode.SwapSets();
        }
    }
}
