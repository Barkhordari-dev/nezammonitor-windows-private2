using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NezamMonitor.App.Services;
using NezamMonitor.App.ViewModels;

namespace NezamMonitor.App.Views;

public partial class CasesView : UserControl
{
    private CasesViewModel _vm;
    private bool _isLoaded = false;

    public CasesView()
    {
        InitializeComponent();
        _vm = new CasesViewModel(DatabaseService.Instance);
        DataContext = _vm;

        // بازیابی اندازه ذخیره شده + اسکرول + ترتیب ستون‌ها
        Loaded += (_, _) =>
        {
            // بازیابی نسبت splitter
            var savedRatio = DatabaseService.Instance.GetSetting("cases_splitter_ratio");
            if (!string.IsNullOrEmpty(savedRatio))
            {
                var parts = savedRatio.Split(':');
                if (parts.Length == 2 && double.TryParse(parts[0], out double t) && double.TryParse(parts[1], out double d) && t > 0 && d > 0)
                {
                    TableRow.Height = new GridLength(t, GridUnitType.Star);
                    DetailRow.Height = new GridLength(d, GridUnitType.Star);
                }
            }

            // بازیابی ترتیب ستون‌ها
            if (!_isLoaded)
            {
                _isLoaded = true;
                RestoreColumnOrder();
            }

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
            {
                var sv = FindChild<ScrollViewer>(CasesGrid);
                if (sv != null) sv.ScrollToEnd();
            });
        };
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CasesViewModel.Cases))
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
                {
                    var sv = FindChild<ScrollViewer>(CasesGrid);
                    if (sv != null) sv.ScrollToEnd();
                });
        };

        // ذخیره ترتیب ستون‌ها هنگام تغییر توسط کاربر
        CasesGrid.ColumnReordered += (_, _) => SaveColumnOrder();
    }

    private void SaveColumnOrder()
    {
        // ذخیره ترتیب ستون‌ها به صورت نام‌های x:Name
        var order = string.Join(",", CasesGrid.Columns.Select(c => c.Header?.ToString() ?? ""));
        DatabaseService.Instance.SaveSetting("cases_column_order", order);
    }

    private void RestoreColumnOrder()
    {
        var savedOrder = DatabaseService.Instance.GetSetting("cases_column_order");
        if (string.IsNullOrEmpty(savedOrder)) return;

        var orderHeaders = savedOrder.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var columns = CasesGrid.Columns.ToList();

        // مرتب‌سازی ستون‌ها بر اساس ترتیب ذخیره شده
        int displayIndex = 0;
        foreach (var header in orderHeaders)
        {
            var col = columns.FirstOrDefault(c => c.Header?.ToString() == header);
            if (col != null)
            {
                col.DisplayIndex = displayIndex;
                displayIndex++;
            }
        }
    }

    private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
        // ذخیره نسبت ارتفاع‌ها (star ratio)
        var tableHeight = TableRow.Height.Value;
        var detailHeight = DetailRow.Height.Value;
        if (tableHeight > 0 && detailHeight > 0)
        {
            var ratio = $"{tableHeight:F1}:{detailHeight:F1}";
            DatabaseService.Instance.SaveSetting("cases_splitter_ratio", ratio);
        }
    }

    private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T found) return found;
            var result = FindChild<T>(child);
            if (result != null) return result;
        }
        return null;
    }

    private void CasesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CasesGrid.SelectedItem is CaseDisplayItem selectedItem)
            _vm.ShowCaseDetails(selectedItem);
    }
}
