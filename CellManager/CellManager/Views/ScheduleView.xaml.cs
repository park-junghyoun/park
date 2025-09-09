using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CellManager.ViewModels;

namespace CellManager.Views
{
    public partial class ScheduleView : UserControl
    {
        private Point _dragStart;
        private InsertionAdorner? _insertionAdorner;

        public ScheduleView()
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
            if (sender is ItemsControl list)
            {
                var item = GetItemUnderMouse(list, e.GetPosition(list));
                if (item?.DataContext is StepTemplate template)
                {
                    var data = new DataObject(typeof(StepTemplate), template);
                    DragDrop.DoDragDrop(list, data, DragDropEffects.Copy);
                }
            }
        }

        private void ScheduleList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
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
                if (item.DataContext is StepTemplate step)
                {
                    var data = new DataObject(typeof(StepTemplate), step);
                    DragDrop.DoDragDrop(list, data, DragDropEffects.Move);
                }
            }
        }

        private void ScheduleList_DragOver(object sender, DragEventArgs e)
        {
            var list = (ItemsControl)sender;
            var index = GetInsertIndex(list, e.GetPosition(list));
            ShowInsertionAdorner(list, index);
        }

        private void ScheduleList_DragLeave(object sender, DragEventArgs e)
        {
            RemoveInsertionAdorner();
        }

        private void ScheduleList_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is not ScheduleViewModel vm) return;
            if (!e.Data.GetDataPresent(typeof(StepTemplate))) return;
            var list = (ItemsControl)sender;
            var index = GetInsertIndex(list, e.GetPosition(list));
            var step = (StepTemplate)e.Data.GetData(typeof(StepTemplate));

            if (vm.Sequence.Contains(step))
                vm.MoveStep(step, index);
            else
                vm.InsertStep(step, index);

            RemoveInsertionAdorner();
        }

        private static int GetInsertIndex(ItemsControl list, Point position)
        {
            var orientation = GetOrientation(list);
            for (int i = 0; i < list.Items.Count; i++)
            {
                if (list.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement item)
                {
                    var bounds = VisualTreeHelper.GetDescendantBounds(item);
                    var topLeft = item.TranslatePoint(new Point(), list);
                    var rect = new Rect(topLeft, bounds.Size);
                    if (rect.Contains(position))
                    {
                        if (orientation == Orientation.Horizontal)
                            return position.X < rect.Left + rect.Width / 2 ? i : i + 1;
                        return position.Y < rect.Top + rect.Height / 2 ? i : i + 1;
                    }
                }
            }
            return list.Items.Count;
        }

        private static Orientation GetOrientation(ItemsControl list)
        {
            var panel = list.ItemsPanel.LoadContent() as Panel;
            return panel switch
            {
                StackPanel sp => sp.Orientation,
                VirtualizingStackPanel vsp => vsp.Orientation,
                _ => Orientation.Vertical
            };
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

        private void ShowInsertionAdorner(ItemsControl list, int index)
        {
            var orientation = GetOrientation(list);
            if (_insertionAdorner == null || _insertionAdorner.AdornedElement != list)
            {
                RemoveInsertionAdorner();
                var layer = AdornerLayer.GetAdornerLayer(list);
                if (layer == null) return;
                _insertionAdorner = new InsertionAdorner(list, orientation);
                layer.Add(_insertionAdorner);
            }
            _insertionAdorner.Update(index);
        }

        private void RemoveInsertionAdorner()
        {
            if (_insertionAdorner != null)
            {
                var layer = AdornerLayer.GetAdornerLayer(_insertionAdorner.AdornedElement);
                if (layer != null)
                    layer.Remove(_insertionAdorner);
                _insertionAdorner = null;
            }
        }

        private class InsertionAdorner : Adorner
        {
            private readonly ItemsControl _owner;
            private readonly Orientation _orientation;
            private int _index;

            public InsertionAdorner(ItemsControl owner, Orientation orientation)
                : base(owner)
            {
                _owner = owner;
                _orientation = orientation;
                IsHitTestVisible = false;
            }

            public void Update(int index)
            {
                _index = index;
                InvalidateVisual();
            }

            protected override void OnRender(DrawingContext dc)
            {
                if (_owner.Items.Count == 0) return;
                var pen = new Pen(Brushes.Red, 2);
                double offset;
                if (_index < _owner.Items.Count)
                {
                    var container = (FrameworkElement)_owner.ItemContainerGenerator.ContainerFromIndex(_index);
                    var pt = container.TranslatePoint(new Point(), this);
                    offset = _orientation == Orientation.Horizontal ? pt.X : pt.Y;
                }
                else
                {
                    var last = (FrameworkElement)_owner.ItemContainerGenerator.ContainerFromIndex(_owner.Items.Count - 1);
                    var pt = last.TranslatePoint(new Point(last.ActualWidth, last.ActualHeight), this);
                    offset = _orientation == Orientation.Horizontal ? pt.X : pt.Y;
                }

                if (_orientation == Orientation.Horizontal)
                {
                    dc.DrawLine(pen, new Point(offset, 0), new Point(offset, ActualHeight));
                }
                else
                {
                    dc.DrawLine(pen, new Point(0, offset), new Point(ActualWidth, offset));
                }
            }
        }
    }
}
