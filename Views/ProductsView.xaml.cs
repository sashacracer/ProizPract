using System.Data;
using System.Windows;
using System.Windows.Controls;
using WarehouseAccountingApp.Models;
using WarehouseAccountingApp.Services;

namespace WarehouseAccountingApp.Views;

public partial class ProductsView : UserControl
{
    private readonly DatabaseService _db = new();
    private readonly UserSession _user;
    private DataTable _products = new();

    public ProductsView(UserSession user)
    {
        InitializeComponent();
        _user = user;
        AddButton.Visibility = user.IsManager ? Visibility.Collapsed : Visibility.Visible;

        StatusFilter.ItemsSource   = new[] { "Все", "В наличии", "Малый остаток", "Нет остатка" };
        StatusFilter.SelectedIndex = 0;

        LoadProducts();
    }

    private void LoadProducts()
    {
        _products = _db.GetTable("""
            SELECT ProductId, Name AS [Название], CategoryName AS [Категория],
                   SupplierName AS [Поставщик], Unit AS [Ед.изм.],
                   Quantity AS [Кол-во], MinQuantity AS [Мин.],
                   Price AS [Цена, ₽], TotalCost AS [Сумма, ₽],
                   CalculatedStatus AS [Статус]
            FROM dbo.v_ProductBalances
            WHERE IsArchived = 0
            ORDER BY Name
            """);
        ProductsGrid.ItemsSource = _products.DefaultView;
    }

    private void ApplyFilter()
    {
        var search = SearchTextBox.Text.Replace("'", "''");
        var status = StatusFilter.SelectedItem?.ToString();

        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
            filters.Add($"([Название] LIKE '%{search}%' OR [Категория] LIKE '%{search}%' OR [Поставщик] LIKE '%{search}%')");
        if (status is not null and not "Все")
            filters.Add($"[Статус] = '{status}'");

        _products.DefaultView.RowFilter = filters.Count > 0 ? string.Join(" AND ", filters) : "";
    }

    private void SearchTextBox_TextChanged(object s, TextChangedEventArgs e) => ApplyFilter();
    private void StatusFilter_Changed(object s, SelectionChangedEventArgs e)  => ApplyFilter();
    private void Refresh_Click(object s, RoutedEventArgs e) => LoadProducts();

    private void AddButton_Click(object s, RoutedEventArgs e)
    {
        if (new ProductEditWindow().ShowDialog() == true) LoadProducts();
    }

    private void ProductsGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        if (e.Row.Item is not DataRowView row) return;
        var status = row["Статус"]?.ToString();
        e.Row.Background = status switch
        {
            "Малый остаток" => (System.Windows.Media.Brush)FindResource("LowStockRowBrush"),
            "Нет остатка"   => (System.Windows.Media.Brush)FindResource("OutOfStockRowBrush"),
            _               => (System.Windows.Media.Brush)FindResource("PanelBackgroundBrush")
        };
    }

    private void ExportExcel_Click(object s, RoutedEventArgs e)
    {
        var path = ExportService.PickExcelPath("Товары_склад");
        if (path == null) return;
        try { ExportService.ExportToExcel(_products, "Товары на складе", path); MessageBox.Show($"Excel сохранён:\n{path}", "Готово"); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
    }

    private void ExportPdf_Click(object s, RoutedEventArgs e)
    {
        var path = ExportService.PickPdfPath("Товары_склад");
        if (path == null) return;
        try { ExportService.ExportToPdf(_products, "Товары на складе", path); MessageBox.Show($"PDF сохранён:\n{path}", "Готово"); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
    }
}
