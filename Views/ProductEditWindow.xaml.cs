using System.Windows;
using Microsoft.Data.SqlClient;
using WarehouseAccountingApp.Models;
using WarehouseAccountingApp.Services;

namespace WarehouseAccountingApp.Views;

public partial class ProductEditWindow : Window
{
    private readonly DatabaseService _db = new();

    public ProductEditWindow()
    {
        InitializeComponent();
        CategoryBox.ItemsSource = _db.GetLookup("dbo.Categories", "CategoryId", "Name", "IsActive = 1");
        SupplierBox.ItemsSource = _db.GetLookup("dbo.Suppliers",  "SupplierId", "Name", "IsActive = 1");
        CategoryBox.SelectedIndex = 0;
        SupplierBox.SelectedIndex = 0;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("Введите название товара.", "Ошибка"); return;
        }
        if (!decimal.TryParse(QuantityBox.Text, out var qty) || qty < 0)
        {
            MessageBox.Show("Количество не может быть отрицательным.", "Ошибка"); return;
        }
        if (!decimal.TryParse(MinQuantityBox.Text, out var minQty) || minQty < 0)
        {
            MessageBox.Show("Минимальный остаток не может быть отрицательным.", "Ошибка"); return;
        }
        if (!decimal.TryParse(PriceBox.Text, out var price) || price < 0)
        {
            MessageBox.Show("Цена не может быть меньше 0.", "Ошибка"); return;
        }

        var category = (LookupItem)CategoryBox.SelectedItem;
        var supplier = (LookupItem)SupplierBox.SelectedItem;

        try
        {
            _db.Execute("""
                INSERT INTO dbo.Products (Name, CategoryId, SupplierId, Unit, Quantity, MinQuantity, Price, Status)
                VALUES (@Name, @CategoryId, @SupplierId, @Unit, @Quantity, @MinQuantity, @Price, N'В наличии')
                """,
                new SqlParameter("@Name",        NameBox.Text.Trim()),
                new SqlParameter("@CategoryId",  category.Id),
                new SqlParameter("@SupplierId",  supplier.Id),
                new SqlParameter("@Unit",        UnitBox.Text.Trim()),
                new SqlParameter("@Quantity",    qty),
                new SqlParameter("@MinQuantity", minQty),
                new SqlParameter("@Price",       price));

            DialogResult = true;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
    }
}
