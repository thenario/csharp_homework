using Microsoft.Data.Sqlite;

namespace assignment08;

public static class DatabaseHelper
{
    private const string ConnectionString = "Data Source=vocab.db";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Words (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                English TEXT NOT NULL,
                Chinese TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();

        command.CommandText = "SELECT COUNT(*) FROM Words";
        if ((long)command.ExecuteScalar()! == 0)
        {
            command.CommandText = """
                INSERT INTO Words (English, Chinese) VALUES 
                ('interface', '接口'),
                ('framework', '框架'),
                ('repository', '仓库'),
                ('dependency', '依赖');
                """;
            command.ExecuteNonQuery();
        }
    }

    public static List<WordModel> GetAll()
    {
        var words = new List<WordModel>();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Words";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            words.Add(new WordModel(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2)
            ));
        }
        return words;
    }
}