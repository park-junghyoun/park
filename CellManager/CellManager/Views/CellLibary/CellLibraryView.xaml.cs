using CellManager.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static MaterialDesignThemes.Wpf.Theme;
using Button = System.Windows.Controls.Button;

namespace CellManager.Views.CellLibary
{
    /// <summary>
    /// CellLibraryView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CellLibraryView : UserControl
    {
        public CellLibraryView()
        {
            InitializeComponent();
        }
        // 각 컬럼의 비율 (총합 = 1.0)
        private readonly double[] columnRatios = { 0.19, 0.14, 0.13, 0.08, 0.12, 0.12, 0.12, 0.1  };

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs? e)
        {
            if (lv_cells.View is GridView gridView)
            {
                if (gridView.Columns.Count != columnRatios.Length)
                    return; // 컬럼 개수와 비율 개수가 다르면 스킵

                double availableWidth = lv_cells.ActualWidth;

                // 스크롤바 너비보다 작거나 같으면 계산하지 않음
                if (availableWidth <= SystemParameters.VerticalScrollBarWidth)
                    return;

                // 전체 사용 가능한 너비
                double totalWidth = availableWidth - SystemParameters.VerticalScrollBarWidth;

                if (totalWidth <= 0)
                    return;

                // 마지막 컬럼 오차 방지를 위해 먼저 앞쪽 컬럼 계산
                double usedWidth = 0;
                for (int i = 0; i < gridView.Columns.Count - 1; i++)
                {
                    gridView.Columns[i].Width = Math.Round(totalWidth * columnRatios[i]);
                    usedWidth += gridView.Columns[i].Width;
                }

                // 마지막 컬럼은 남은 공간 모두 채움
                gridView.Columns[^1].Width = Math.Max(0, totalWidth - usedWidth);
            }
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            // 레이아웃이 완료된 후 한 번 더 컬럼 너비를 조정
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ListView_SizeChanged(sender, null);
            }), DispatcherPriority.Loaded);
        }
        private void Active_Bt_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var vm = Application.Current.MainWindow?.DataContext as MainViewModel;
                var cell = vm.SelectedCell;
                if (cell.Id != 0 && btn.Content.ToString() == "Inactive")
                {
                    btn.Content = "Active";
                }
                else 
                { 
                    btn.Content = "Inactive"; 
                }
            }
        }
    }
}
