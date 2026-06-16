using System.Data.Common;

namespace WarehouseAccountingApp.Services;

public static class DataReaderExtensions
{
    public static int GetInt32(this DbDataReader r, string col) => r.GetInt32(r.GetOrdinal(col));
    public static string GetString(this DbDataReader r, string col) => r.GetString(r.GetOrdinal(col));
    public static decimal GetDecimal(this DbDataReader r, string col) => r.GetDecimal(r.GetOrdinal(col));
    public static bool IsDBNull(this DbDataReader r, string col) => r.IsDBNull(r.GetOrdinal(col));
}
