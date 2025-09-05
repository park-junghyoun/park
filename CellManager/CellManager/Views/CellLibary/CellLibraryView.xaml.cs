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
        // 총합이 1이 되도록 마지막 컬럼 비율을 수정했습니다.
        private readonly double[] columnRatios = { 0.05, 0.25, 0.16, 0.16, 0.12, 0.08, 0.18 };

        private void CellLibraryView_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure column widths are recalculated once the layout is ready
            ListView_SizeChanged(null, null);
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (lv_cells.View is GridView gridView)
            {
                if (gridView.Columns.Count != columnRatios.Length )
                    return; // 비율 개수와 맞지 않으면 스킵

                // ListView width must be larger than the scrollbar width
                if (lv_cells.ActualWidth <= SystemParameters.VerticalScrollBarWidth)
                    return;

                // Actions 컬럼을 제외한 전체 사용 가능한 너비
                double totalWidth = lv_cells.ActualWidth - SystemParameters.VerticalScrollBarWidth;

                // totalWidth가 0 이하일 경우 재조정 생략
                if (totalWidth <= 0)
                    return;

                // Actions 컬럼을 제외한 나머지 컬럼에 비율대로 너비 적용
                for (int i = 0; i < gridView.Columns.Count; i++)
                {
                    gridView.Columns[i].Width = Math.Round(totalWidth * columnRatios[i]);
                }
            }
        }
    }
}
