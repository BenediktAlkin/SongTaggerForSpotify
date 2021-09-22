using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SpotifySongTagger.Utils
{
    public static class UIHelper
    {
        public static List<T> FindVisualChildren<T>(DependencyObject current) where T : DependencyObject
        {
            if (current == null) return null;

            var result = new List<T>();
            int childrenCount = VisualTreeHelper.GetChildrenCount(current);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(current, i);
                if (child is T childT) result.Add(childT);
                result.AddRange(FindVisualChildren<T>(child));
            }
            return result;
        }
        public static T FindVisualChild<T>(DependencyObject current) where T : DependencyObject
        {
            if (current == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(current);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(current, i);
                if (child is T) return (T)child;
                T result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
        public static T FindVisualParent<T>(DependencyObject current) where T : DependencyObject
        {
            if (current == null) return null;

            var child = VisualTreeHelper.GetParent(current);
            if (child is T) return (T)child;
            T result = FindVisualParent<T>(child);

            return result;
        }


        public static int GetDataGridRowIndex(DataGrid dataGrid, RoutedEventArgs e)
        {
            double GetRowHeight(object item)
            {
                var itemContainer = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(item);
                var height = itemContainer.ActualHeight;
                var marginTop = itemContainer.Margin.Top;
                return marginTop + height;
            }
            int GetItemIndex(int offset, double positionY)
            {
                var index = 0;
                double height = 0;

                for (var i = offset; i < dataGrid.Items.Count; i++)
                {
                    height += GetRowHeight(dataGrid.Items.GetItemAt(i));
                    if (height > positionY) return index;
                    index++;
                }

                return index;
            }

            var headersPresenter = UIHelper.FindVisualChild<DataGridColumnHeadersPresenter>(dataGrid);
            double headerHight = headersPresenter.ActualHeight;

            // get offset from ScrollViewer
            var scrollViewer = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(dataGrid, 0), 0) as ScrollViewer;
            var offset = (int)scrollViewer.VerticalOffset;

            // get position
            Point position;
            if (e is DragEventArgs ev)
                position = ev.GetPosition(dataGrid);
            var positionY = position.Y - headerHight;
            var index = (int)(GetItemIndex(offset, positionY) + offset);

            return index;
        }
    }
}
