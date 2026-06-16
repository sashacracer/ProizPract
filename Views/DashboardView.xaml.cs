using System.Windows.Controls;
using WarehouseAccountingApp.Models;
using WarehouseAccountingApp.Services;

namespace WarehouseAccountingApp.Views;

public partial class DashboardView : UserControl
{
    private readonly DatabaseService _database = new();

    public DashboardView(UserSession user)
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        var summary = _database.GetDashboardSummary();
        TotalProductsText.Text = summary.TotalProducts.ToString();
        LowStockText.Text = summary.LowStockProducts.ToString();
        OutOfStockText.Text = summary.OutOfStockProducts.ToString();
        TotalCostText.Text = $"{summary.TotalWarehouseCost:N2} ₽";

        OperationsGrid.ItemsSource = _database.GetTable("""
            SELECT TOP 10 OperationDate, OperationType, ProductName, Quantity, UserFullName, BalanceAfter
            FROM dbo.v_StockOperationHistory
            ORDER BY OperationDate DESC, OperationId DESC
            """).DefaultView;
    }
}
