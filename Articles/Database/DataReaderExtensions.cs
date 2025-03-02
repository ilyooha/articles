using System.Data;

namespace Database;

public static class DataReaderExtensions
{
    public static string? GetNullableString(this IDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}