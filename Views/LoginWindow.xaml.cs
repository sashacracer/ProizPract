using System.Windows;
using WarehouseAccountingApp.Services;

namespace WarehouseAccountingApp.Views;

public partial class LoginWindow : Window
{
    private readonly DatabaseService _database = new();

    public LoginWindow()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorTextBlock.Text = "";

        try
        {
            var user = _database.Authenticate(LoginTextBox.Text.Trim(), PasswordBox.Password);

            if (user == null)
            {
                ErrorTextBlock.Text = "Неверный логин или пароль.";
                return;
            }

            var mainWindow = new MainWindow(user);
            mainWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            ErrorTextBlock.Text = "Ошибка подключения к базе данных: " + ex.Message;
        }
    }
}
