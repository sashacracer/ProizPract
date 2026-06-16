using System.Data;
using System.Windows;
using System.Windows.Controls;
using WarehouseAccountingApp.Services;

namespace WarehouseAccountingApp.Views;

public partial class ReportsView : UserControl
{
    private readonly DatabaseService _db = new();
    private DataTable _current = new();
    private string _currentTitle = "Отчёт";

    public ReportsView()
    {
        InitializeComponent();
        DateFrom.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        DateTo.SelectedDate   = DateTime.Today;
        AllProducts_Click(this, new RoutedEventArgs());
    }

    private void Show(DataTable table, string title)
    {
        _current = table;
        _currentTitle = title;
        ReportsGrid.ItemsSource = table.DefaultView;
        ReportTitleText.Text = $"{title} — {table.Rows.Count} записей";
    }

    private DateTime From => DateFrom.SelectedDate ?? DateTime.Today.AddMonths(-1);
    private DateTime To   => DateTo.SelectedDate   ?? DateTime.Today;

    private void AllProducts_Click(object s, RoutedEventArgs e)  => Show(_db.GetAllProductsReport(),         "Все товары");
    private void LowStock_Click   (object s, RoutedEventArgs e)  => Show(_db.GetLowStockReport(),            "Малый остаток");
    private void OutOfStock_Click (object s, RoutedEventArgs e)  => Show(_db.GetOutOfStockReport(),          "Нет остатка");
    private void StockIn_Click    (object s, RoutedEventArgs e)  => Show(_db.GetStockInReport(From, To),     $"Приход {From:dd.MM.yy}–{To:dd.MM.yy}");
    private void StockOut_Click   (object s, RoutedEventArgs e)  => Show(_db.GetStockOutReport(From, To),    $"Расход {From:dd.MM.yy}–{To:dd.MM.yy}");
    private void Cost_Click       (object s, RoutedEventArgs e)  => Show(_db.GetCostReport(),                "Стоимость склада");
    private void Suppliers_Click  (object s, RoutedEventArgs e)  => Show(_db.GetSuppliersReport(),           "Отчёт по поставщикам");

    private void ExportExcel_Click(object sender, RoutedEventArgs e)
    {
        var path = ExportService.PickExcelPath(_currentTitle.Replace(" ", "_"));
        if (path == null) return;
        try
        {
            ExportService.ExportToExcel(_current, _currentTitle, path);
            MessageBox.Show($"Excel сохранён:\n{path}", "Готово");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
    }

    private void ExportPdf_Click(object sender, RoutedEventArgs e)
    {
        var path = ExportService.PickPdfPath(_currentTitle.Replace(" ", "_"));
        if (path == null) return;
        try
        {
            ExportService.ExportToPdf(_current, _currentTitle, path);
            MessageBox.Show($"PDF сохранён:\n{path}", "Готово");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
    }
}
