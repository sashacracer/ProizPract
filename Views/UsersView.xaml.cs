using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using WarehouseAccountingApp.Models;
using WarehouseAccountingApp.Services;

namespace WarehouseAccountingApp.Views;

public partial class UsersView : UserControl
{
    private readonly DatabaseService _database = new();

    public UsersView()
    {
        InitializeComponent();
        RoleBox.ItemsSource = _database.GetLookup("dbo.Roles", "RoleId", "Name");
        RoleBox.SelectedIndex = 0;
        LoadData();
    }

    private void LoadData()
    {
        UsersGrid.ItemsSource = _database.GetTable("""
            SELECT u.UserId, u.Login, u.FullName, r.Name AS RoleName, u.IsActive, u.CreatedAt
            FROM dbo.Users u
            JOIN dbo.Roles r ON r.RoleId = u.RoleId
            ORDER BY u.UserId
            """).DefaultView;
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LoginBox.Text) ||
            string.IsNullOrWhiteSpace(PasswordBox.Text) ||
            string.IsNullOrWhiteSpace(FullNameBox.Text))
        {
            MessageBox.Show("Заполните логин, пароль и ФИО.");
            return;
        }

        var role = (LookupItem)RoleBox.SelectedItem;

        _database.Execute("""
            INSERT INTO dbo.Users (Login, PasswordHash, FullName, RoleId)
            VALUES (@Login, @PasswordHash, @FullName, @RoleId)
            """,
            new SqlParameter("@Login", LoginBox.Text.Trim()),
            new SqlParameter("@PasswordHash", PasswordBox.Text.Trim()),
            new SqlParameter("@FullName", FullNameBox.Text.Trim()),
            new SqlParameter("@RoleId", role.Id));

        LoginBox.Clear();
        PasswordBox.Clear();
        FullNameBox.Clear();
        LoadData();
    }
}
