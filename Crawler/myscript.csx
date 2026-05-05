// clear_db.csx
#r "nuget: Microsoft.Data.Sqlite"

using Microsoft.Data.Sqlite;
using System;

string dbPath = "data.db";

try 
{
    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();
    
    using var command = connection.CreateCommand();
    command.CommandText = "DELETE FROM Files";
    
    int count = command.ExecuteNonQuery();
    Console.WriteLine($"清理成功！从 {dbPath} 中删除了 {count} 条记录。");
}
catch (Exception ex)
{
    Console.WriteLine($"运行出错: {ex.Message}");
}