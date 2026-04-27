using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Crawler.Entities;

namespace Crawler.Services.DbService;

public class FileDbService
{
    private readonly string _connectionString = "Data Source=data.db";

    public void InitialFileDbService()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
        CREATE TABLE IF NOT EXISTS Files (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Title TEXT NOT NULL,
            Url TEXT NOT NULL,
            LocalPath TEXT NOT NULL,
            Type TEXT NOT NULL,
            OriginalName TEXT NOT NULL,
            FileSize TEXT NOT NULL,
            DownloadTime DATETIME NOT NULL
        )
        """;
        command.ExecuteNonQuery();
    }

    public List<FileEntity> GetAllFiles(int currentPage, int pageSize)
    {
        using var connection = new SqliteConnection(_connectionString);
        List<FileEntity> list = new List<FileEntity>();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Files ORDER BY DownloadTime DESC LIMIT $LIMIT OFFSET $OFFSET";

        command.Parameters.AddWithValue("$LIMIT", pageSize);
        command.Parameters.AddWithValue("$OFFSET", (currentPage - 1) * pageSize);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new FileEntity
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Url = reader.GetString(2),
                LocalPath = reader.GetString(3),
                Type = reader.GetString(4),
                OriginalName = reader.GetString(5),
                FileSize = reader.GetString(6),
                DownloadTime = reader.GetDateTime(7)
            });
        }
        return list;
    }
}