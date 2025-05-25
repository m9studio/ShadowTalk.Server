using System.Data.SQLite;

namespace M9Studio.ShadowTalk.Server
{
    public class DataBase
    {
        private SQLiteConnection connection;
        public DataBase()
        {
            string connectionString = "Data Source=localdb.sqlite;Version=3;";

            bool isNew = false;
            if (!System.IO.File.Exists("localdb.sqlite"))
            {
                SQLiteConnection.CreateFile("localdb.sqlite");
                isNew = true;

            }
            connection = new SQLiteConnection(connectionString);
            connection.Open();
            if (isNew)
            {
                string createTable = @"
                    CREATE TABLE users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT,
                        email TEXT,
                        password TEXT,
                        salt TEXT,
                        verifier TEXT,
                        is2fa INTEGER,
                        rsa TEXT
                    );";
                using (var cmd = new SQLiteCommand(createTable, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public List<User> Users(string request, params object[] param)
        {
            var users = new List<User>();

            using (var cmd = new SQLiteCommand(request, connection))
            {
                foreach (var p in param)
                {
                    cmd.Parameters.AddWithValue(null, p);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"]?.ToString(),
                            Email = reader["email"]?.ToString(),
                            Password = reader["password"]?.ToString(),
                            Salt = reader["salt"]?.ToString(),
                            Verifier = reader["verifier"]?.ToString(),
                            Is2FA = Convert.ToInt32(reader["is2fa"]) == 1,
                            RSA = reader["rsa"]?.ToString()
                        });
                    }
                }
            }

            return users;
        }
        public void Send(string request, params object[] param)
        {
            using (var cmd = new SQLiteCommand(request, connection))
            {
                foreach (var p in param)
                {
                    cmd.Parameters.AddWithValue(null, p);
                }
                cmd.ExecuteNonQuery();
            }
        }
        public void Close() => connection.Close();
    }
}
