using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CellManager.ViewModels;

namespace CellManager.Views
{
    /// <summary>
    ///     Code-behind for the schedule view handling drag/drop and layout adjustments for steps.
    /// </summary>
    public partial class ScheduleView : UserControl
    {
        private const string DragSourceFormat = "ScheduleView_IsFromSequence";
        private Point _dragStart;
        private ItemsControl? _dragSourceList;
        private FrameworkElement? _dragItemContainer;
        private StepTemplate? _dragTemplate;
        private InsertionAdorner? _insertionAdorner;
        private DragAdorner? _dragAdorner;
        private UIElement? _dragScope;
        private ScheduleViewModel? _viewModel;
        private INotifyCollectionChanged? _pagedCalendarSubscription;

        public ScheduleView()
        {
            InitializeComponent();
            DataContextChanged += ScheduleView_DataContextChanged;
            Loaded += ScheduleView_Loaded;
        }

        private void ScheduleView_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ScheduleView_Loaded;
        }

        private void ScheduleView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ScheduleViewModel oldVm)
            {
                oldVm.PropertyChanged -= ViewModelOnPropertyChanged;
                if (_pagedCalendarSubscription != null)
                    _pagedCalendarSubscription.CollectionChanged -= PagedCalendarDays_CollectionChanged;
            }

            _viewModel = e.NewValue as ScheduleViewModel;
            _pagedCalendarSubscription = null;

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
                if (_viewModel.PagedCalendarDays is INotifyCollectionChanged collection)
                {
                    _pagedCalendarSubscription = collection;
                    collection.CollectionChanged += PagedCalendarDays_CollectionChanged;
                }
            }
            ResetCalendarScroll();
        }

        private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ScheduleViewModel.CalendarMode) ||
                e.PropertyName == nameof(ScheduleViewModel.CalendarPageIndex))
            {
                ResetCalendarScroll();
            }
            else if (e.PropertyName == nameof(ScheduleViewModel.IsCalendarExpanded))
            {
                if (_viewModel?.IsCalendarExpanded == true)
                {
                    ResetCalendarScroll();
                }
            }
        }

        private void PagedCalendarDays_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ResetCalendarScroll();
        }

        private void ResetCalendarScroll()
        {
            if (CalendarScrollViewer == null)
                return;

            CalendarScrollViewer.Dispatcher.BeginInvoke(() =>
            {
                CalendarScrollViewer.ScrollToHorizontalOffset(0);
            }, DispatcherPriority.Background);
        }
        private void ProfileList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
            CaptureDragStart(sender, e);
        }

        private void ProfileList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResetDragState();
        }

        private void ProfileList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            var pos = e.GetPosition(null);
            if (Math.Abs(pos.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(pos.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;
            if (sender is not ItemsControl list) return;
            if (!ReferenceEquals(list, _dragSourceList)) return;
            if (_dragItemContainer is not FrameworkElement item) return;
            if (_dragTemplate == null) return;
            BeginDrag(list, item, _dragTemplate, DragDropEffects.Copy);
        }

        private void ScheduleList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
            CaptureDragStart(sender, e);
        }

        private void ScheduleList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResetDragState();
        }

        private void ScheduleList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            var pos = e.GetPosition(null);
            if (Math.Abs(pos.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(pos.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;
            if (sender is not ItemsControl list) return;
            if (!ReferenceEquals(list, _dragSourceList)) return;
            if (_dragItemContainer is not FrameworkElement item) return;
            if (_dragTemplate == null) return;
            BeginDrag(list, item, _dragTemplate, DragDropEffects.Move);
        }

        private void CaptureDragStart(object sender, MouseButtonEventArgs e)
        {
            ResetDragState();
            if (sender is not ItemsControl list) return;

            _dragSourceList = list;
            _dragItemContainer = GetItemUnderMouse(list, e.GetPosition(list));
            if (_dragItemContainer?.DataContext is StepTemplate template)
            {
                _dragTemplate = template;
            }
        }

        private void ResetDragState()
        {
            _dragSourceList = null;
            _dragItemContainer = null;
            _dragTemplate = null;
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

            var isFromSequence = e.Data.GetDataPresent(DragSourceFormat) &&
                                 e.Data.GetData(DragSourceFormat) is bool fromSequence && fromSequence;

            if (isFromSequence)
                vm.MoveStep(step, index);
            else
                vm.InsertStep(step, index);

            RemoveInsertionAdorner();
            RemoveDragAdorner();
        }

        private void ScheduleList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is not ListView listView) return;
            if (listView.View is not GridView gridView) return;

            double workingWidth = listView.ActualWidth;

            var scrollViewer = FindVisualChild<ScrollViewer>(listView);
            if (scrollViewer?.ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                workingWidth -= SystemParameters.VerticalScrollBarWidth;
            }

            if (gridView.Columns.Count >= 3)
            {
                gridView.Columns[0].Width = workingWidth * 0.15;
                gridView.Columns[1].Width = workingWidth * 0.60;
                gridView.Columns[2].Width = workingWidth * 0.25;
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild) return tChild;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private static int GetInsertIndex(ItemsControl list, Point position)
        {
            var orientation = GetOrientation(list);
            for (int i = 0; i < list.Items.Count; i++)
            {
                if (list.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement item)
                {
                    var topLeft = item.TranslatePoint(new Point(), list);
                    var rect = new Rect(topLeft, item.RenderSize);
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
                    var topLeft = item.TranslatePoint(new Point(), list);
                    var rect = new Rect(topLeft, item.RenderSize);
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

        private void BeginDrag(ItemsControl source, FrameworkElement item, StepTemplate step, DragDropEffects effect)
        {
            var data = new DataObject(typeof(StepTemplate), step);
            data.SetData(DragSourceFormat, effect == DragDropEffects.Move);
            _dragScope = Window.GetWindow(this)?.Content as UIElement;
            if (_dragScope != null)
            {
                var layer = AdornerLayer.GetAdornerLayer(_dragScope);
                if (layer != null)
                {
                    _dragAdorner = new DragAdorner(_dragScope, item);
                    layer.Add(_dragAdorner);
                    _dragAdorner.SetPosition(Mouse.GetPosition(_dragScope));
                    _dragScope.AddHandler(DragOverEvent, new DragEventHandler(DragScope_DragOver), true);
                }
            }
            DragDrop.DoDragDrop(source, data, effect);
            RemoveDragAdorner();
            ResetDragState();
        }

        private void DragScope_DragOver(object sender, DragEventArgs e)
        {
            _dragAdorner?.SetPosition(e.GetPosition(_dragScope!));
        }

        private void RemoveDragAdorner()
        {
            if (_dragAdorner != null && _dragScope != null)
            {
                var layer = AdornerLayer.GetAdornerLayer(_dragScope);
                if (layer != null)
                    layer.Remove(_dragAdorner);
                _dragAdorner = null;
                _dragScope.RemoveHandler(DragOverEvent, new DragEventHandler(DragScope_DragOver));
                _dragScope = null;
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

        private class DragAdorner : Adorner
        {
            private readonly VisualBrush _brush;
            private Point _position;

            public DragAdorner(UIElement owner, UIElement adorned)
                : base(owner)
            {
                _brush = new VisualBrush(adorned) { Opacity = 0.7 };
                IsHitTestVisible = false;
            }

            public void SetPosition(Point point)
            {
                _position = point;
                InvalidateVisual();
            }

            protected override void OnRender(DrawingContext dc)
            {
                if (_brush.Visual is not FrameworkElement fe) return;
                var size = fe.RenderSize;
                dc.DrawRectangle(_brush, null, new Rect(_position.X - size.Width / 2, _position.Y - size.Height / 2, size.Width, size.Height));
            }
        }
    }
}
