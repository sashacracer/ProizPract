using System.Data;
using System.Windows;
using System.Windows.Controls;
using WarehouseAccountingApp.Services;

namespace WarehouseAccountingApp.Views;

public partial class HistoryView : UserControl
{
    private readonly DatabaseService _db = new();
    private DataTable _history = new();

    public HistoryView()
    {
        InitializeComponent();
        TypeFilter.ItemsSource   = new[] { "Все", "Приход", "Расход" };
        TypeFilter.SelectedIndex = 0;
        LoadData();
    }

    private void LoadData()
    {
        _history = _db.GetTable("""
            SELECT OperationDate AS [Дата],
                   OperationType AS [Тип],
                   ProductName   AS [Товар],
                   Quantity      AS [Кол-во],
                   UserFullName  AS [Сотрудник],
                   SupplierName  AS [Поставщик],
                   WriteOffReason AS [Причина],
                   Comment       AS [Комментарий],
                   BalanceAfter  AS [Остаток]
            FROM dbo.v_StockOperationHistory
            ORDER BY OperationDate DESC, OperationId DESC
            """);
        HistoryGrid.ItemsSource = _history.DefaultView;
    }

    private void ApplyFilter()
    {
        var search = SearchBox.Text.Replace("'", "''");
        var type   = TypeFilter.SelectedItem?.ToString();

        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
            filters.Add($"([Товар] LIKE '%{search}%' OR [Сотрудник] LIKE '%{search}%' OR [Поставщик] LIKE '%{search}%')");
        if (type is not null and not "Все")
            filters.Add($"[Тип] = '{type}'");

        _history.DefaultView.RowFilter = filters.Count > 0 ? string.Join(" AND ", filters) : "";
    }

    private void SearchBox_TextChanged(object s, TextChangedEventArgs e) => ApplyFilter();
    private void TypeFilter_Changed(object s, SelectionChangedEventArgs e) => ApplyFilter();
    private void Refresh_Click(object s, RoutedEventArgs e) => LoadData();

    private void ExportExcel_Click(object s, RoutedEventArgs e)
    {
        var path = ExportService.PickExcelPath("История_операций");
        if (path == null) return;
        try { ExportService.ExportToExcel(_history, "История складских операций", path); MessageBox.Show($"Excel сохранён:\n{path}", "Готово"); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
    }

    private void ExportPdf_Click(object s, RoutedEventArgs e)
    {
        var path = ExportService.PickPdfPath("История_операций");
        if (path == null) return;
        try { ExportService.ExportToPdf(_history, "История складских операций", path); MessageBox.Show($"PDF сохранён:\n{path}", "Готово"); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
    }
}
