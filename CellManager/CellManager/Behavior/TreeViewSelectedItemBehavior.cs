using System.Windows;
using System.Windows.Controls;

namespace CellManager.Behaviors
{
    /// <summary>
    /// TreeView.SelectedItem을 바인딩 가능하게 만들어주는 Attached Behavior
    /// </summary>
    public static class TreeViewSelectedItemBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(TreeViewSelectedItemBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static void SetEnable(DependencyObject element, bool value) =>
            element.SetValue(EnableProperty, value);

        public static bool GetEnable(DependencyObject element) =>
            (bool)element.GetValue(EnableProperty);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeView tv)
            {
                if ((bool)e.NewValue)
                    tv.SelectedItemChanged += Tv_SelectedItemChanged;
                else
                    tv.SelectedItemChanged -= Tv_SelectedItemChanged;
            }
        }

        public static readonly DependencyProperty BindableSelectedItemProperty =
            DependencyProperty.RegisterAttached(
                "BindableSelectedItem",
                typeof(object),
                typeof(TreeViewSelectedItemBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindableSelectedItemChanged));

        public static void SetBindableSelectedItem(DependencyObject element, object value) =>
            element.SetValue(BindableSelectedItemProperty, value);

        public static object GetBindableSelectedItem(DependencyObject element) =>
            element.GetValue(BindableSelectedItemProperty);

        private static void Tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tv = (TreeView)sender;
            SetBindableSelectedItem(tv, e.NewValue);
        }

        // VM -> View 방향(선택 항목을 코드로 바꾸기)은 트리 가상화/컨테이너 탐색이 필요하므로 생략
        // (현재 요구사항은 View->VM만으로 충분)
        private static void OnBindableSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 필요 시 선택 항목을 View 쪽으로 강제 반영하는 로직을 추가 가능
        }
    }
}
