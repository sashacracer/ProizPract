using System.Windows;
using System.Windows.Controls;
using WarehouseAccountingApp.Models;

namespace WarehouseAccountingApp.Views;

public partial class MainWindow : Window
{
    private readonly UserSession _user;

    public MainWindow(UserSession user)
    {
        InitializeComponent();
        _user = user;
        UserTextBlock.Text = $"{_user.FullName} | {_user.RoleName}";
        UserInitialsText.Text = string.IsNullOrWhiteSpace(_user.FullName) ? "U" : _user.FullName[..1].ToUpper();
        UsersButton.Visibility = _user.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        ShowPage("Главная", DashboardButton, new DashboardView(_user));
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e) => ShowPage("Главная", DashboardButton, new DashboardView(_user));
    private void Products_Click(object sender, RoutedEventArgs e) => ShowPage("Товары", ProductsButton, new ProductsView(_user));
    private void Suppliers_Click(object sender, RoutedEventArgs e) => ShowPage("Поставщики", SuppliersButton, new SuppliersView(_user));
    private void Operations_Click(object sender, RoutedEventArgs e) => ShowPage("Приход / расход", OperationsButton, new OperationsView(_user));
    private void History_Click(object sender, RoutedEventArgs e) => ShowPage("История операций", HistoryButton, new HistoryView());
    private void Reports_Click(object sender, RoutedEventArgs e) => ShowPage("Отчеты", ReportsButton, new ReportsView());
    private void Users_Click(object sender, RoutedEventArgs e) => ShowPage("Пользователи", UsersButton, new UsersView());

    private void ShowPage(string title, Button activeButton, object view)
    {
        PageTitleText.Text = title;
        MainContent.Content = view;

        DashboardButton.Tag = null;
        ProductsButton.Tag = null;
        SuppliersButton.Tag = null;
        OperationsButton.Tag = null;
        HistoryButton.Tag = null;
        ReportsButton.Tag = null;
        UsersButton.Tag = null;
        activeButton.Tag = "Active";
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        new LoginWindow().Show();
        Close();
    }
}
