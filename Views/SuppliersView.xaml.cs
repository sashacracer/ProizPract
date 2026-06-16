using System.Data;
using System.Windows;
using System.Windows.Controls;
using WarehouseAccountingApp.Models;
using WarehouseAccountingApp.Services;

namespace WarehouseAccountingApp.Views;

public partial class SuppliersView : UserControl
{
    private readonly DatabaseService _db = new();
    private DataTable _suppliers = new();

    public SuppliersView(UserSession user)
    {
        InitializeComponent();
        AddButton.Visibility = user.IsManager ? Visibility.Collapsed : Visibility.Visible;
        LoadData();
    }

    private void LoadData()
    {
        _suppliers = _db.GetTable("""
            SELECT SupplierId AS [ID], Name AS [Название],
                   ContactPerson AS [Контакт], Phone AS [Телефон],
                   Email AS [Email], Address AS [Адрес],
                   Comment AS [Комментарий], IsActive AS [Активен]
            FROM dbo.Suppliers
            ORDER BY Name
            """);
        SuppliersGrid.ItemsSource = _suppliers.DefaultView;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var text = SearchBox.Text.Replace("'", "''");
        _suppliers.DefaultView.RowFilter =
            $"[Название] LIKE '%{text}%' OR [Контакт] LIKE '%{text}%' OR [Телефон] LIKE '%{text}%'";
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (new SupplierEditWindow().ShowDialog() == true)
            LoadData();
    }
}
