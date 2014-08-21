using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ColabConcept.Web.Infrastructure
{
    public class UserStore : IUserStore
    {
        private static readonly SQLiteConnection UsersDatabase;

        static UserStore()
        {
            UsersDatabase = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;");

            UsersDatabase.Open();

            using (var command = UsersDatabase.CreateCommand())
            {
                command.CommandText = "CREATE TABLE Users (UserName TEXT PRIMARY KEY, UUID TEXT NOT NULL)";
                command.CommandType = System.Data.CommandType.Text;

                command.ExecuteNonQuery();
            }
        }

        public async Task<bool> ExistsAsync(string userName)
        {

                using (var command = UsersDatabase.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Users WHERE UserName = ?";
                    command.CommandType = System.Data.CommandType.Text;

                    var nameParameter = command.CreateParameter();
                    nameParameter.Value = userName;
                    command.Parameters.Add(nameParameter);

                    long count = (long)(await command.ExecuteScalarAsync());

                    return count > 0;
                }
        }

        public async Task<Guid> AddAsync(string userName)
        {

                using (var command = UsersDatabase.CreateCommand())
                {
                    Guid uuid = Guid.NewGuid();

                    command.CommandText = "INSERT INTO Users (UserName, UUID) VALUES (?, ?)";
                    command.CommandType = System.Data.CommandType.Text;

                    var nameParameter = command.CreateParameter();
                    nameParameter.Value = userName;
                    command.Parameters.Add(nameParameter);

                    var uuidParameter = command.CreateParameter();
                    uuidParameter.Value = uuid.ToString();
                    command.Parameters.Add(uuidParameter);

                    await command.ExecuteNonQueryAsync();

                    return uuid;
                }
        }
    }
}