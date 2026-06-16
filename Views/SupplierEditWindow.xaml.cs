using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Data.SqlClient;
using WarehouseAccountingApp.Services;

namespace WarehouseAccountingApp.Views;

public partial class SupplierEditWindow : Window
{
    private readonly DatabaseService _db = new();
    private bool _isFormatting;
    private int? _editingId; 

    private static readonly Regex PhoneRegex = new(
        @"^(\+7|8)[\s\-]?\(?\d{3}\)?[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}$",
        RegexOptions.Compiled);

    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    public SupplierEditWindow()
    {
        InitializeComponent();
    }

    public SupplierEditWindow(int id, string name, string contact, string phone,
                              string email, string address, string comment)
    {
        InitializeComponent();
        _editingId = id;

        Title = "Редактирование поставщика";
        NameBox.Text = name;
        ContactPersonBox.Text = contact;
        PhoneBox.Text = phone;
        EmailBox.Text = email;
        AddressBox.Text = address;
        CommentBox.Text = comment;
    }

    private void PhoneBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_isFormatting) return;

        _isFormatting = true;

        var raw = PhoneBox.Text;
        var digits = new string(raw.Where(char.IsDigit).ToArray());

        if (digits.Length > 0)
        {
            if (digits[0] == '8' || digits[0] == '7')
                digits = "7" + digits[1..];
            else
                digits = "7" + digits;
        }

        if (digits.Length > 11)
            digits = digits[..11];

        string formatted = FormatPhone(digits);

        int oldLen = raw.Length;
        int caret = PhoneBox.CaretIndex;

        PhoneBox.Text = formatted;

        if (caret >= oldLen)
            PhoneBox.CaretIndex = PhoneBox.Text.Length;
        else
            PhoneBox.CaretIndex = Math.Min(caret, PhoneBox.Text.Length);

        _isFormatting = false;
    }

    private static string FormatPhone(string digits)
    {
        if (digits.Length == 0) return "";

        var sb = new System.Text.StringBuilder("+7");

        if (digits.Length > 1)
        {
            sb.Append(" (");
            sb.Append(digits.AsSpan(1, Math.Min(3, digits.Length - 1)));
        }
        if (digits.Length >= 4)
        {
            sb.Append(") ");
            sb.Append(digits.AsSpan(4, Math.Min(3, digits.Length - 4)));
        }
        if (digits.Length >= 7)
        {
            sb.Append('-');
            sb.Append(digits.AsSpan(7, Math.Min(2, digits.Length - 7)));
        }
        if (digits.Length >= 9)
        {
            sb.Append('-');
            sb.Append(digits.AsSpan(9, Math.Min(2, digits.Length - 9)));
        }

        return sb.ToString();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Введите название организации.", "Ошибка валидации");
            NameBox.Focus();
            return;
        }

        var phone = PhoneBox.Text.Trim();
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (!string.IsNullOrWhiteSpace(phone) && !PhoneRegex.IsMatch(phone))
        {
            MessageBox.Show(
                "Неверный формат телефона.\n\nДопустимые форматы:\n" +
                "  +7 (999) 123-45-67\n" +
                "  8 (999) 123-45-67\n" +
                "  +79991234567",
                "Ошибка валидации");
            PhoneBox.Focus();
            return;
        }

        if (digits.Length > 0 && digits.Length != 11)
        {
            MessageBox.Show("Телефон должен содержать 11 цифр.");
            PhoneBox.Focus();
            return;
        }

        var email = EmailBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(email) && !EmailRegex.IsMatch(email))
        {
            MessageBox.Show(
                "Неверный формат email.\n\nПример: supplier@company.ru",
                "Ошибка валидации");
            EmailBox.Focus();
            return;
        }

        try
        {
            if (_editingId.HasValue)
            {
                _db.Execute("""
                    UPDATE dbo.Suppliers
                    SET Name = @Name,
                        ContactPerson = @ContactPerson,
                        Phone = @Phone,
                        Email = @Email,
                        Address = @Address,
                        Comment = @Comment
                    WHERE Id = @Id
                    """,
                    new SqlParameter("@Id", _editingId.Value),
                    new SqlParameter("@Name", name),
                    new SqlParameter("@ContactPerson", ContactPersonBox.Text.Trim()),
                    new SqlParameter("@Phone", phone),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@Address", AddressBox.Text.Trim()),
                    new SqlParameter("@Comment", CommentBox.Text.Trim()));
            }
            else
            {
                _db.Execute("""
                    INSERT INTO dbo.Suppliers (Name, ContactPerson, Phone, Email, Address, Comment)
                    VALUES (@Name, @ContactPerson, @Phone, @Email, @Address, @Comment)
                    """,
                    new SqlParameter("@Name", name),
                    new SqlParameter("@ContactPerson", ContactPersonBox.Text.Trim()),
                    new SqlParameter("@Phone", phone),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@Address", AddressBox.Text.Trim()),
                    new SqlParameter("@Comment", CommentBox.Text.Trim()));
            }

            DialogResult = true;
            Close();
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            MessageBox.Show("Поставщик с таким названием уже существует.", "Ошибка");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка");
        }
    }
}