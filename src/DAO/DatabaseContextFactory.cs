using System;
using System.Configuration;

namespace DAO;

public static class DatabaseContextFactory
{
    private const string ConnectionName = "QUANLICHTL";

    public static DataClasses1DataContext Create()
    {
        var connectionString = ConfigurationManager
            .ConnectionStrings[ConnectionName]?
            .ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ConfigurationErrorsException(
                $"Không tìm thấy connection string '{ConnectionName}'.");
        }

        return new DataClasses1DataContext(connectionString);
    }
}
