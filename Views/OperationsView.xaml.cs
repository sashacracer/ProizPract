using System.Windows;
using System.Windows.Controls;
using WarehouseAccountingApp.Models;
using WarehouseAccountingApp.Services;

namespace WarehouseAccountingApp.Views;

public partial class OperationsView : UserControl
{
    private readonly DatabaseService _db = new();
    private readonly UserSession _user;

    public OperationsView(UserSession user)
    {
        InitializeComponent();
        _user = user;

        InProductBox.ItemsSource  = _db.GetLookup("dbo.Products",       "ProductId", "Name",   "IsArchived = 0");
        OutProductBox.ItemsSource = _db.GetLookup("dbo.Products",       "ProductId", "Name",   "IsArchived = 0");
        SupplierBox.ItemsSource   = _db.GetLookup("dbo.Suppliers",      "SupplierId","Name",   "IsActive = 1");
        ReasonBox.ItemsSource     = _db.GetLookup("dbo.WriteOffReasons","ReasonId",  "Name",   "IsActive = 1");

        InProductBox.SelectedIndex  = 0;
        OutProductBox.SelectedIndex = 0;
        SupplierBox.SelectedIndex   = 0;
        ReasonBox.SelectedIndex     = 0;
    }

    private void StockIn_Click(object sender, RoutedEventArgs e)
    {
        if (InProductBox.SelectedItem is not LookupItem product) { MessageBox.Show("Выберите товар."); return; }
        if (SupplierBox.SelectedItem  is not LookupItem supplier){ MessageBox.Show("Выберите поставщика."); return; }
        if (!decimal.TryParse(InQuantityBox.Text, out var qty) || qty <= 0)
        {
            MessageBox.Show("Введите корректное количество (> 0).", "Ошибка");
            return;
        }

        try
        {
            _db.RegisterStockIn(product.Id, _user.UserId, qty, supplier.Id, InCommentBox.Text);
            InQuantityBox.Text   = "1";
            InCommentBox.Text    = "";
            MessageBox.Show($"Приход {qty} единиц товара «{product.Name}» оформлен.", "Готово");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
    }

    private void StockOut_Click(object sender, RoutedEventArgs e)
    {
        if (OutProductBox.SelectedItem is not LookupItem product) { MessageBox.Show("Выберите товар."); return; }
        if (ReasonBox.SelectedItem     is not LookupItem reason)  { MessageBox.Show("Выберите причину."); return; }
        if (!decimal.TryParse(OutQuantityBox.Text, out var qty) || qty <= 0)
        {
            MessageBox.Show("Введите корректное количество (> 0).", "Ошибка");
            return;
        }

        try
        {
            _db.RegisterStockOut(product.Id, _user.UserId, qty, reason.Id, OutCommentBox.Text);
            OutQuantityBox.Text  = "1";
            OutCommentBox.Text   = "";
            MessageBox.Show($"Расход {qty} единиц товара «{product.Name}» оформлен.", "Готово");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка"); }
    }
}
