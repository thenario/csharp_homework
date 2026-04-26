using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Crawler.Sevices.DbService;

public class FileDbService
{
    private readonly string _connectionString = "Data Source = data.db";
    public void InitialFileDbService()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        string dbsql = """
        CREATE TABLE IF NOT EXISTS File (
        Id INTEGER PRIMARY KET AUTOINCREMENT,
        Title TEXT NOT NULL,
        Url TEXT NOT NULL,
        LocalPath TEXT NOT NULL,
        Type TEXT NOT NULL,
        OriginalName TEXT NOT NULL,
        FileSize TEXT NOT NULL,
        DownloadTime DATETIME NOT NULL,
        FileSize TEXT NOT NULL
        )
        """;
        command.ExecuteNonQuery();
    }
}