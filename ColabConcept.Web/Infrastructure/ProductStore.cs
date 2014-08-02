using ColabConcept.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web;

namespace ColabConcept.Web.Infrastructure
{
    public class ProductStore : IProductStore
    {
        private static readonly SQLiteConnection _connection;

        static ProductStore()
        {
            _connection = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;");

            _connection.Open();

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE Product (Id TEXT PRIMARY KEY, Name TEXT NOT NULL, Description TEXT NOT NULL, LockedBy TEXT)";
                command.ExecuteNonQuery();
            }
        }

        public void Add(Product product)
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Product (Id, Name, Description, LockedBy) VALUES (?, ?, ?, ?)";
                command.CommandType = System.Data.CommandType.Text;
                
                var idParameter = command.CreateParameter();
                idParameter.Value = product.Id.ToString();

                var nameParameter = command.CreateParameter();
                nameParameter.Value = product.Name;

                var descriptionParameter = command.CreateParameter();
                descriptionParameter.Value = product.Description;

                var lockedByParameter = command.CreateParameter();
                lockedByParameter.Value = product.LockedBy;

                command.Parameters.AddRange(
                    new SQLiteParameter[] {
                        idParameter,
                        nameParameter,
                        descriptionParameter,
                        lockedByParameter
                    });

                command.ExecuteNonQuery();
            }
        }

        public bool Update(Product product)
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "UPDATE Product SET Name = ?, Description = ?, LockedBy = NULL WHERE Id = ? and LockedBy = ?";
                command.CommandType = System.Data.CommandType.Text;

                var idParameter = command.CreateParameter();
                idParameter.Value = product.Id.ToString();

                var nameParameter = command.CreateParameter();
                nameParameter.Value = product.Name;

                var descriptionParameter = command.CreateParameter();
                descriptionParameter.Value = product.Description;

                var lockedByParameter = command.CreateParameter();
                lockedByParameter.Value = product.LockedBy;

                command.Parameters.AddRange(
                    new SQLiteParameter[] {
                        nameParameter,
                        descriptionParameter,
                        idParameter,
                        lockedByParameter
                    });

                return command.ExecuteNonQuery() == 1;
            }
        }

        public bool Remove(Product product)
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM Product WHERE Id = ?";
                command.CommandType = System.Data.CommandType.Text;

                var idParameter = command.CreateParameter();
                idParameter.Value = product.Id.ToString();

                command.Parameters.AddRange(
                    new SQLiteParameter[] {
                        idParameter
                    });

                return command.ExecuteNonQuery() == 1;
            }
        }

        public Product Get(Guid id)
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Name, Description, LockedBy FROM Product WHERE Id = ?";
                command.CommandType = System.Data.CommandType.Text;

                var idParameter = command.CreateParameter();
                idParameter.Value = id.ToString();

                command.Parameters.AddRange(
                    new SQLiteParameter[] {
                        idParameter
                    });

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return 
                            new Product 
                            {
                                Id = Guid.Parse(reader.GetString(0)),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2),
                                LockedBy = reader.IsDBNull(3) ? null : reader.GetString(3)
                            };
                    }
                }

                return null;
            }
        }

        public IEnumerable<Product> GetAll()
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Name, Description, LockedBy FROM Product";
                command.CommandType = System.Data.CommandType.Text;

                using (var reader = command.ExecuteReader())
                {
                    return ReadProducts(reader);
                }

            }
        }

        public bool LockProduct(Guid productId, string lockedBy)
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "UPDATE Product SET LockedBy = ? WHERE Id = ? AND LockedBy is NULL";
                command.CommandType = System.Data.CommandType.Text;

                var idParameter = command.CreateParameter();
                idParameter.Value = productId.ToString();

                var lockedByParameter = command.CreateParameter();
                lockedByParameter.Value = lockedBy.ToString();

                command.Parameters.AddRange(
                    new SQLiteParameter[] {
                        lockedByParameter,
                        idParameter
                    });

                int count = command.ExecuteNonQuery();

                return count > 0;
            }
        }

        public IEnumerable<Guid> CancelEdits(string user)
        {
            //no sprocs meh
            List<Product> products;

            using (var queryCommand = _connection.CreateCommand())
            {
                queryCommand.CommandText = "SELECT Id, Name, Description, LockedBy FROM Product WHERE LockedBy = ?";
                queryCommand.CommandType = CommandType.Text;

                var lockedByParameter = queryCommand.CreateParameter();
                lockedByParameter.Value = user;
                queryCommand.Parameters.Add(lockedByParameter);
                                
                using (var reader = queryCommand.ExecuteReader())
                {
                     products = ReadProducts(reader);
                }
            }

            using (var updateCommand = _connection.CreateCommand())
            {
                StringBuilder parameterisedUpdateStatement = new StringBuilder("UPDATE Product SET LockedBy = NULL WHERE Id IN (");

                parameterisedUpdateStatement.Append(
                    string.Join(",",
                        Enumerable
                        .Range(0, products.Count)
                        .Select(x => "?")
                        .ToList())
                    );

                parameterisedUpdateStatement.Append(")");

                updateCommand.CommandType = CommandType.Text;
                updateCommand.CommandText = parameterisedUpdateStatement.ToString();

                updateCommand.Parameters.AddRange(
                    products
                        .Select(
                            product =>
                            {
                                var parameter = updateCommand.CreateParameter();
                                parameter.Value = product.Id.ToString();
                                return parameter;
                            })
                        .ToArray());

                updateCommand.ExecuteNonQuery();
            }

            return 
                products
                .Select(x => x.Id);
        }

        private List<Product> ReadProducts(SQLiteDataReader reader)
        {
            List<Product> products = new List<Product>();

            while (reader.Read())
            {
                products
                    .Add(
                        new Product
                            {
                                Id = Guid.Parse(reader.GetString(0)),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2),
                                LockedBy = reader.IsDBNull(3) ? null : reader.GetString(3)
                            });
                    
            }

            return products;
        }

        public bool UnlockProduct(Guid productId, string lockedBy)
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "UPDATE Product SET LockedBy = NULL WHERE Id = ? AND LockedBy = ?";
                command.CommandType = System.Data.CommandType.Text;

                var idParameter = command.CreateParameter();
                idParameter.Value = productId.ToString();

                var lockedByParameter = command.CreateParameter();
                lockedByParameter.Value = lockedBy.ToString();

                command.Parameters.AddRange(
                    new SQLiteParameter[] {
                        idParameter,
                        lockedByParameter
                    });

                int count = command.ExecuteNonQuery();

                return count > 0;
            }
        }
    }
}