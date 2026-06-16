namespace WarehouseAccountingApp.Models;

public sealed class UserSession
{
    public int UserId { get; set; }
    public string Login { get; set; } = "";
    public string FullName { get; set; } = "";
    public string RoleName { get; set; } = "";

    public bool IsAdmin => RoleName == "Admin";
    public bool IsWarehouse => RoleName == "Warehouse";
    public bool IsManager => RoleName == "Manager";
}

public sealed class LookupItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public override string ToString() => Name;
}

public sealed class DashboardSummary
{
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public decimal TotalWarehouseCost { get; set; }
}
