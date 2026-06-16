using System.Data;
using Microsoft.Data.SqlClient;
using WarehouseAccountingApp.Models;

namespace WarehouseAccountingApp.Services;

public sealed class DatabaseService
{
    private const string ConnectionString =
        "Server=10.211.55.2,1433;Database=WarehouseAccountingDb;User Id=warehouse_user;Password=Warehouse123!;TrustServerCertificate=True;";

    public UserSession? Authenticate(string login, string password)
    {
        const string sql = """
            SELECT u.UserId, u.Login, u.FullName, r.Name AS RoleName
            FROM dbo.Users u
            JOIN dbo.Roles r ON r.RoleId = u.RoleId
            WHERE u.Login = @Login AND u.PasswordHash = @Password AND u.IsActive = 1
            """;
        using var con = Open();
        using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@Login", login);
        cmd.Parameters.AddWithValue("@Password", password);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new UserSession
        {
            UserId   = r.GetInt32("UserId"),
            Login    = r.GetString("Login"),
            FullName = r.GetString("FullName"),
            RoleName = r.GetString("RoleName")
        };
    }

    public DataTable GetTable(string sql, params SqlParameter[] parameters)
    {
        using var con = new SqlConnection(ConnectionString);
        using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddRange(parameters);
        using var adapter = new SqlDataAdapter(cmd);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public void Execute(string sql, params SqlParameter[] parameters)
    {
        using var con = Open();
        using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddRange(parameters);
        cmd.ExecuteNonQuery();
    }

    public List<LookupItem> GetLookup(string table, string idCol, string nameCol, string? where = null)
    {
        var sql = $"SELECT {idCol} AS Id, {nameCol} AS Name FROM {table}";
        if (!string.IsNullOrWhiteSpace(where)) sql += " WHERE " + where;
        sql += $" ORDER BY {nameCol}";

        var list = new List<LookupItem>();
        using var con = Open();
        using var cmd = new SqlCommand(sql, con);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new LookupItem { Id = r.GetInt32("Id"), Name = r.GetString("Name") });
        return list;
    }

    public DashboardSummary GetDashboardSummary()
    {
        using var con = Open();
        using var cmd = new SqlCommand("dbo.GetDashboardSummary", con);
        cmd.CommandType = CommandType.StoredProcedure;
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return new DashboardSummary();
        return new DashboardSummary
        {
            TotalProducts      = r.GetInt32("TotalProducts"),
            LowStockProducts   = r.GetInt32("LowStockProducts"),
            OutOfStockProducts = r.GetInt32("OutOfStockProducts"),
            TotalWarehouseCost = r.IsDBNull("TotalWarehouseCost") ? 0 : r.GetDecimal("TotalWarehouseCost")
        };
    }

    public void RegisterStockIn(int productId, int userId, decimal quantity, int supplierId, string comment)
    {
        using var con = Open();
        using var cmd = new SqlCommand("dbo.RegisterStockIn", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@ProductId",  productId);
        cmd.Parameters.AddWithValue("@UserId",     userId);
        cmd.Parameters.AddWithValue("@Quantity",   quantity);
        cmd.Parameters.AddWithValue("@SupplierId", supplierId);
        cmd.Parameters.AddWithValue("@Comment",    comment);
        cmd.ExecuteNonQuery();
    }

    public void RegisterStockOut(int productId, int userId, decimal quantity, int reasonId, string comment)
    {
        using var con = Open();
        using var cmd = new SqlCommand("dbo.RegisterStockOut", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@ProductId", productId);
        cmd.Parameters.AddWithValue("@UserId",    userId);
        cmd.Parameters.AddWithValue("@Quantity",  quantity);
        cmd.Parameters.AddWithValue("@ReasonId",  reasonId);
        cmd.Parameters.AddWithValue("@Comment",   comment);
        cmd.ExecuteNonQuery();
    }

    public DataTable GetStockInReport(DateTime from, DateTime to) =>
        GetTable("""
            SELECT OperationDate AS [Дата], ProductName AS [Товар],
                   Quantity AS [Кол-во], SupplierName AS [Поставщик],
                   UserFullName AS [Сотрудник], Comment AS [Комментарий],
                   BalanceAfter AS [Остаток после]
            FROM dbo.v_StockOperationHistory
            WHERE OperationCode = 'IN' AND CAST(OperationDate AS date) BETWEEN @From AND @To
            ORDER BY OperationDate DESC
            """,
            new SqlParameter("@From", from.Date),
            new SqlParameter("@To",   to.Date));

    public DataTable GetStockOutReport(DateTime from, DateTime to) =>
        GetTable("""
            SELECT OperationDate AS [Дата], ProductName AS [Товар],
                   Quantity AS [Кол-во], WriteOffReason AS [Причина],
                   UserFullName AS [Сотрудник], Comment AS [Комментарий],
                   BalanceAfter AS [Остаток после]
            FROM dbo.v_StockOperationHistory
            WHERE OperationCode = 'OUT' AND CAST(OperationDate AS date) BETWEEN @From AND @To
            ORDER BY OperationDate DESC
            """,
            new SqlParameter("@From", from.Date),
            new SqlParameter("@To",   to.Date));

    public DataTable GetAllProductsReport() =>
        GetTable("""
            SELECT Name AS [Название], CategoryName AS [Категория],
                   SupplierName AS [Поставщик], Unit AS [Ед.изм.],
                   Quantity AS [Кол-во], MinQuantity AS [Мин.],
                   Price AS [Цена], TotalCost AS [Сумма], CalculatedStatus AS [Статус]
            FROM dbo.v_ProductBalances WHERE IsArchived = 0 ORDER BY Name
            """);

    public DataTable GetLowStockReport() =>
        GetTable("""
            SELECT Name AS [Название], CategoryName AS [Категория],
                   SupplierName AS [Поставщик], Quantity AS [Кол-во],
                   MinQuantity AS [Минимум], CalculatedStatus AS [Статус]
            FROM dbo.v_ProductBalances
            WHERE IsArchived = 0 AND Quantity < MinQuantity ORDER BY Quantity
            """);

    public DataTable GetOutOfStockReport() =>
        GetTable("""
            SELECT Name AS [Название], CategoryName AS [Категория],
                   SupplierName AS [Поставщик], Unit AS [Ед.изм.], Price AS [Цена]
            FROM dbo.v_ProductBalances WHERE IsArchived = 0 AND Quantity = 0 ORDER BY Name
            """);

    public DataTable GetCostReport() =>
        GetTable("""
            SELECT CategoryName AS [Категория],
                   COUNT(*) AS [Товаров],
                   SUM(Quantity) AS [Кол-во всего],
                   SUM(TotalCost) AS [Стоимость (₽)]
            FROM dbo.v_ProductBalances WHERE IsArchived = 0
            GROUP BY CategoryName ORDER BY SUM(TotalCost) DESC
            """);

    public DataTable GetSuppliersReport() =>
        GetTable("""
            SELECT s.Name AS [Поставщик], s.ContactPerson AS [Контакт],
                   s.Phone AS [Телефон], s.Email AS [Email],
                   COUNT(DISTINCT p.ProductId) AS [Товаров],
                   COALESCE(SUM(p.Quantity * p.Price), 0) AS [Сумма (₽)]
            FROM dbo.Suppliers s
            LEFT JOIN dbo.Products p ON p.SupplierId = s.SupplierId AND p.IsArchived = 0
            WHERE s.IsActive = 1 GROUP BY s.Name, s.ContactPerson, s.Phone, s.Email
            ORDER BY s.Name
            """);

    private SqlConnection Open()
    {
        var con = new SqlConnection(ConnectionString);
        con.Open();
        return con;
    }
}
