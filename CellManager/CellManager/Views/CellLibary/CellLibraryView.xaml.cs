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
            Loaded += CellLibraryView_Loaded;
        }
        // 각 컬럼의 비율 (Actions 컬럼 제외)
        private readonly double[] columnRatios = { 0.05, 0.25, 0.16,0.16, 0.12, 0.08, 0.23, };
        private const double ActionsColumnWidth = 60; // 마지막 Actions 컬럼 고정 너비

        private void CellLibraryView_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure column widths are recalculated once the layout is ready
            ListView_SizeChanged(null, null);
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (lv_cells.View is GridView gridView)
            {
                if (gridView.Columns.Count != columnRatios.Length + 1)
                    return; // Actions 컬럼을 제외한 비율 개수와 맞지 않으면 스킵

                // ListView width must be larger than the scrollbar width
                if (lv_cells.ActualWidth <= SystemParameters.VerticalScrollBarWidth + ActionsColumnWidth)
                    return;

                // Actions 컬럼을 제외한 전체 사용 가능한 너비
                double totalWidth = lv_cells.ActualWidth - SystemParameters.VerticalScrollBarWidth - ActionsColumnWidth;

                // totalWidth가 0 이하일 경우 재조정 생략
                if (totalWidth <= 0)
                    return;

                // 마지막 전(Activation) 컬럼 오차 방지를 위해 먼저 앞쪽 컬럼 계산
                double usedWidth = 0;
                for (int i = 0; i < gridView.Columns.Count - 2; i++)
                {
                    gridView.Columns[i].Width = Math.Round(totalWidth * columnRatios[i]);
                    usedWidth += gridView.Columns[i].Width;
                }

                // Activation 컬럼은 남은 공간 모두 채움
                gridView.Columns[^2].Width = totalWidth - usedWidth;

                // 마지막 Actions 컬럼은 고정 너비
                gridView.Columns[^1].Width = ActionsColumnWidth;
            }
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
