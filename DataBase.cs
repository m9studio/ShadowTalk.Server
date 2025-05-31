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
            if (!File.Exists("localdb.sqlite"))
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


                string createMessagesTable = @"
                    CREATE TABLE messages (
                        sender INTEGER,
                        recipient INTEGER,
                        uuid TEXT PRIMARY KEY,
                        text TEXT,
                        type INTEGER DEFAULT 0
                    );";
                using (var cmd = new SQLiteCommand(createMessagesTable, connection))
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
        public List<Message> Messages(string request, params object[] param)
        {
            var messages = new List<Message>();

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
                        messages.Add(new Message
                        {
                            Sender = Convert.ToInt32(reader["sender"]),
                            Recipient = Convert.ToInt32(reader["recipient"]),
                            UUID = reader["uuid"]?.ToString(),
                            Text = reader["text"]?.ToString(),
                            Type = Convert.ToInt32(reader["type"])
                        });
                    }
                }
            }

            return messages;
        }

        public int Count(string request, params object[] param)
        {
            int count = -1;
            using (var cmd = new SQLiteCommand(request, connection))
            {
                foreach (var p in param)
                {
                    cmd.Parameters.AddWithValue(null, p);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        count = Convert.ToInt32(reader["num"]);
                    }
                }
            }
            return count;
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
