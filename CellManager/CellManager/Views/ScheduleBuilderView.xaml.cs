using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CellManager.Models;
using CellManager.ViewModels;

namespace CellManager.Views
{
    public partial class ScheduleBuilderView : UserControl
    {
        private Point _dragStart;

        public ScheduleBuilderView()
        {
            InitializeComponent();
        }

        private void ProfileList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
        }

        private void ProfileList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            var pos = e.GetPosition(null);
            if (Math.Abs(pos.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(pos.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;
            if (sender is ListBox list && list.SelectedItem is ProfileReference profile)
            {
                var data = new DataObject(typeof(TestProfileModel), profile);
                data.SetData("DragSource", "ProfileList");
                DragDrop.DoDragDrop(list, data, DragDropEffects.Copy);
            }
        }

        private void ScheduleList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            var pos = e.GetPosition(null);
            if (Math.Abs(pos.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(pos.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;
            if (sender is ListBox list && GetItemUnderMouse(list, e.GetPosition(list)) is ListBoxItem item)
            {
                var data = new DataObject(typeof(TestProfileModel), item.DataContext);
                data.SetData("DragSource", "ScheduleList");
                DragDrop.DoDragDrop(list, data, DragDropEffects.Move);
            }
        }

        private void ScheduleList_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is not ScheduleViewModel vm) return;
            if (!e.Data.GetDataPresent(typeof(ProfileReference))) return;
            var profile = (ProfileReference)e.Data.GetData(typeof(ProfileReference));
            var source = e.Data.GetData("DragSource") as string;
            var isMove = source == "ScheduleList";
            var list = (ListBox)sender;
            var index = GetInsertIndex(list, e.GetPosition(list));
            vm.InsertProfile(profile, index);
        }

        private void ScheduleList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
        }
        private static int GetInsertIndex(ListBox list, Point position)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                var item = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(i);
                if (item != null)
                {
                    var bounds = VisualTreeHelper.GetDescendantBounds(item);
                    var topLeft = item.TranslatePoint(new Point(), list);
                    var rect = new Rect(topLeft, bounds.Size);
                    if (rect.Contains(position))
                        return position.Y < rect.Top + rect.Height / 2 ? i : i + 1;
                }
            }
            return list.Items.Count;
        }

        private static ListBoxItem? GetItemUnderMouse(ListBox list, Point point)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                var item = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(i);
                if (item == null) continue;
                var bounds = VisualTreeHelper.GetDescendantBounds(item);
                var topLeft = item.TranslatePoint(new Point(), list);
                var rect = new Rect(topLeft, bounds.Size);
                if (rect.Contains(point)) return item;
            }
            return null;
        }
    }
}