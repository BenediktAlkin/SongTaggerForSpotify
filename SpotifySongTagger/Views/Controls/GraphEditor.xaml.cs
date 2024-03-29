﻿using Backend.Entities.GraphNodes;
using Backend.Entities.GraphNodes.AudioFeaturesFilters;
using SpotifySongTagger.Utils;
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
            if (ViewModel != null)
            {
                await ViewModel.LoadGraphNodes();
                ViewModel.ClearAllInputResults();
                await ViewModel.RefreshInputResults();
            }
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
                ViewModel.NewArrowStartNodeRect = new Rect(Canvas.GetLeft(contentPresenter), Canvas.GetTop(contentPresenter), contentPresenter.ActualWidth, contentPresenter.ActualHeight);
                ViewModel.UpdateNewArrow(curPos);
            }

            e.Handled = true;
            //Log.Information($"MouseDown {sender.GetType()} {ViewModel.PressedMouseButton} {curPos.X:N2}/{curPos.Y:N2}");
        }
        private async void FrameworkElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null) return;

            if (ViewModel.PressedMouseButton == MouseButton.Left)
            {
                // stop moving node
                //Log.Information($"StopMovingNode {sender.GetType()}");
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
                ViewModel.NewArrowStartNodeRect = null;
                ViewModel.NewArrow = null;
                await ViewModel.AddConnection(nodeVM);
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
                ViewModel.UpdateNewArrow(curPos);

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
            if (ViewModel != null)
                ViewModel.SelectedObject = null;
            Canvas.Focus(); // removes focus from GraphNode editable fields
        }


        private async void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            // canvas has to be in focus for this event to fire
            if (e.Key == Key.Escape)
                ViewModel.SelectedObject = null;

            if (e.Key == Key.Delete || e.Key == Key.Return)
                await ViewModel.DeleteSelected();
        }


        #region UpdateCanvasSize
        private void UpdateCanvasSize_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateCanvasSize();
        private void UpdateCanvasSize()
        {
            if (ViewModel == null) return;
            ViewModel.UpdateCanvasSize(Canvas.ActualWidth, Canvas.ActualHeight);
        }
        #endregion

        #region update GraphNode properties
        private void PlaylistOutputNodeName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // if validation gives an error for NewTagName, it is not updated in the ViewModel
            var textBox = sender as TextBox;
            var frameworkElement = sender as FrameworkElement;
            var outputNode = frameworkElement.DataContext as PlaylistOutputNode;

            ViewModel.PlaylistOutputNode_SetName(outputNode, textBox.Text);
        }
        private async void AssignTagNode_TagChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            var frameworkElement = sender as FrameworkElement;
            var node = frameworkElement.DataContext as AssignTagNode;
            if (node == null) return;

            await ViewModel.AssignTagNode_TagChanged(node, node.Tag);
        }
        private async void FilterTagNode_TagChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            var frameworkElement = sender as FrameworkElement;
            var node = frameworkElement.DataContext as FilterTagNode;
            if (node == null) return;

            await ViewModel.FilterTagNode_TagChanged(node, node.Tag);
        }
        private async void FilterArtistNode_ArtistChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            var frameworkElement = sender as FrameworkElement;
            var node = frameworkElement.DataContext as FilterArtistNode;
            if (node == null) return;

            await ViewModel.FilterArtistNode_ArtistChanged(node, node.Artist);
        }
        private async void PlaylistInputNode_PlaylistChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            var frameworkElement = sender as FrameworkElement;
            var node = frameworkElement.DataContext as PlaylistInputNode;
            if (node == null) return;

            await ViewModel.PlaylistInputNode_PlaylistChanged(node, node.Playlist);
        }
        private async void SwapRemoveNodeInputs(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            var removeNode = frameworkElement.DataContext as RemoveNode;

            await ViewModel.RemoveNode_SwapSets(removeNode);
        }
        private async void FilterRangeNode_ValueChanged(object sender, TextChangedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (Validation.GetErrors(frameworkElement).Count == 0)
            {
                var filterRangeNode = frameworkElement.DataContext as FilterRangeNode;
                if (filterRangeNode == null) return;
                await ViewModel.FilterRangeNode_Edit(filterRangeNode, filterRangeNode.ValueFrom, filterRangeNode.ValueTo);
            }
        }
        #endregion

        #region update AudioFeatures GraphNode properties
        private async void FilterKeyNode_KeyChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            var frameworkElement = sender as FrameworkElement;
            var node = frameworkElement.DataContext as FilterKeyNode;
            if (node == null) return;

            await ViewModel.FilterKeyNode_KeyChanged(node, node.Key);
        }
        private async void FilterModeNode_ModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            var frameworkElement = sender as FrameworkElement;
            var node = frameworkElement.DataContext as FilterModeNode;
            if (node == null) return;

            await ViewModel.FilterModeNode_ModeChanged(node, node.Mode);
        }
        private async void FilterTimeSignatureNode_TimeSignatureChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            var frameworkElement = sender as FrameworkElement;
            var node = frameworkElement.DataContext as FilterTimeSignatureNode;
            if (node == null) return;

            await ViewModel.FilterTimeSignatureNode_TimeSignatureChanged(node, node.TimeSignature);
        }
        private async void FilterGenreNode_GenreChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;
            var frameworkElement = sender as FrameworkElement;
            var node = frameworkElement.DataContext as FilterGenreNode;
            if (node == null) return;

            await ViewModel.FilterGenreNode_GenreChanged(node, node.Genre);
        }
        #endregion
    }
}
