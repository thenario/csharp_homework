using System;//提供基础的数据类型定义，输入输出
using System.Collections.Generic;//提供泛型集合如list<>
using Microsoft.Data.Sqlite;//用于和sqlite进行交互的包
using Crawler.Entities;//实体类命名空间

namespace Crawler.Services.DbService
{
    public class FileDbService
    {
        private readonly string _connectionString = "Data Source=data.db";//readonly代表只读，无法更改，
        //datasource是关键字标识数据库的位置，data.db是具体的文件路径，即保存数据库的文件

        public void InitialFileDbService()//初始化，第一次运行时没有数据库则会构建数据库的表结构
        {
            using var connection = new SqliteConnection(_connectionString);//using表示会自动管理后面的资源，另一种写法是加上大括号，就像是py里的with
            connection.Open();//打开链接，准备执行sql语句
            var command = connection.CreateCommand();//返回一个SqliteCommand对象，在里面写入sql语句
            command.CommandText = """
            CREATE TABLE IF NOT EXISTS Files (--在没有表的前提下创建表
                Id INTEGER PRIMARY KEY AUTOINCREMENT,--id自增
                Title TEXT NOT NULL,
                Url TEXT NOT NULL,
                LocalPath TEXT NOT NULL,
                Type TEXT NOT NULL,
                OriginalName TEXT NOT NULL,
                FileSize TEXT NOT NULL,
                DownloadTime DATETIME NOT NULL
            )
            """;//三对双引号是用于多行字符串且不需要转义，就像node里的``，java里的""""""一样
            command.ExecuteNonQuery();//标识执行非查询操作，返回int表示受影响的行数，在建表时返回-1
        }

        public List<FileEntity> GetFilesByType(string type, int currentPage, int pageSize)//按类型获取数据库里的文件
        {
            using var connection = new SqliteConnection(_connectionString);//创建链接
            List<FileEntity> list = new List<FileEntity>();//创建存放结果的容器
            connection.Open();//打开链接

            var command = connection.CreateCommand();//创建指令
            //$防注入，就和?的占位功能一样，并且查找的顺序是不固定的，所以需要先排好序防止结果重复等奇怪的情况
            command.CommandText = "SELECT * FROM Files WHERE Type = $TYPE ORDER BY DownloadTime DESC LIMIT $LIMIT OFFSET $OFFSET";
            command.Parameters.AddWithValue("$TYPE", type);//按照类型查找
            command.Parameters.AddWithValue("$LIMIT", pageSize);//每页的尺寸
            command.Parameters.AddWithValue("$OFFSET", (currentPage - 1) * pageSize);//偏移量

            using var reader = command.ExecuteReader();//执行查找语句并返回一个读取器用于读取数据，每次加载一行数据到内存中
            //并且read之后不能再次读取已读的内容，除非重新查询
            //刚刚被创建时reader指向第行前面的位置
            while (reader.Read())//read操作会使reader移向下一行，有数据则返回true，没有则返回false
            {
                list.Add(new FileEntity {//索引对应着查询语句里的先后顺序，获取数据并且用于定义实体类存进list当中
                    Id = reader.GetInt32(0), Title = reader.GetString(1), Url = reader.GetString(2),
                    LocalPath = reader.GetString(3), Type = reader.GetString(4), OriginalName = reader.GetString(5),
                    FileSize = reader.GetString(6), DownloadTime = reader.GetDateTime(7)
                });
            }
            return list;
        }

        public void DeleteFile(int id)//删除数据库中的记录
        {
            using var connection = new SqliteConnection(_connectionString);//创建链接
            connection.Open();//打开链接
            var command = connection.CreateCommand();//创建指令
            command.CommandText = "DELETE FROM Files WHERE Id = $ID";//写入指令
            command.Parameters.AddWithValue("$ID", id);//填充占位符
            command.ExecuteNonQuery();//执行非读指令
        }

        public void SaveFile(FileEntity file)
        {
            using var connection = new SqliteConnection(_connectionString);//创建链接
            connection.Open();//打开链接
            var command = connection.CreateCommand();//创建指令
            //写入指令
            command.CommandText = "INSERT INTO Files (Title, Url, LocalPath, Type, OriginalName, FileSize, DownloadTime) VALUES ($title, $url, $path, $type, $name, $size, $time)";
            //填充占位符
            command.Parameters.AddWithValue("$title", file.Title ?? ""); 
            command.Parameters.AddWithValue("$url", file.Url ?? "");
            command.Parameters.AddWithValue("$path", file.LocalPath ?? ""); 
            command.Parameters.AddWithValue("$type", file.Type ?? "");
            command.Parameters.AddWithValue("$name", file.OriginalName ?? ""); 
            command.Parameters.AddWithValue("$size", file.FileSize ?? "");
            command.Parameters.AddWithValue("$time", file.DownloadTime);
            command.ExecuteNonQuery();//执行非读指令
        }
    }
}