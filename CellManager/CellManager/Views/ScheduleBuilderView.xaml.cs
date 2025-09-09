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
                var data = new DataObject(typeof(ProfileReference), profile);
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
            if (sender is ItemsControl list && GetItemUnderMouse(list, e.GetPosition(list)) is FrameworkElement item)
            {
                if (item.DataContext is ScheduledProfile sp)
                {
                    var data = new DataObject(typeof(ProfileReference), sp.Reference);
                    DragDrop.DoDragDrop(list, data, DragDropEffects.Move);
                }
            }
        }

        private void ScheduleList_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is not ScheduleViewModel vm) return;
            if (!e.Data.GetDataPresent(typeof(ProfileReference))) return;
            var profile = (ProfileReference)e.Data.GetData(typeof(ProfileReference));
            var list = (ItemsControl)sender;
            var index = GetInsertIndex(list, e.GetPosition(list));
            vm.InsertProfile(profile, index);
        }

        private void ScheduleList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
        }
        private static int GetInsertIndex(ItemsControl list, Point position)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                if (list.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement item)
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

        private static FrameworkElement? GetItemUnderMouse(ItemsControl list, Point point)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                if (list.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement item)
                {
                    var bounds = VisualTreeHelper.GetDescendantBounds(item);
                    var topLeft = item.TranslatePoint(new Point(), list);
                    var rect = new Rect(topLeft, bounds.Size);
                    if (rect.Contains(point)) return item;
                }
            }
            return null;
        }
    }
}