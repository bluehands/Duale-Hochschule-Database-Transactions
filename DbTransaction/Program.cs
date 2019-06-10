using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbTransaction
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = @"Data Source=.;Initial Catalog=Shop;Integrated Security=True";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT [Id],[Firma],[Name],[Straße],[PLZ],[Ort],[Geburtsdatum] FROM [Kunden] where [Id] = 2004";
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        Console.WriteLine($"Name (Before) {reader["Name"]}");
                    }
                    reader.Close();
                }

                var transaction = connection.BeginTransaction();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = "UPDATE [dbo].[Kunden] SET [Name] = @name WHERE Id=2004";
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = "Name_" + Guid.NewGuid().ToString().Substring(0, 5);
                    var affectedRows = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Update {affectedRows} row(s)");
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = "SELECT [Id],[Firma],[Name],[Straße],[PLZ],[Ort],[Geburtsdatum] FROM [Kunden] where [Id] = 2004";
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        Console.WriteLine($"Name (After) in transaction {reader["Name"]}");
                    }
                    reader.Close();
                }
                //Set a breakpoint and run a query in SSMS 
                //Set Transaction isolation level READ COMMITTED  --READ UNCOMMITTED
                //SELECT* FROM[Shop].[dbo].[Kunden]
                //Change isolation level and inspect query execution (query will end or not)
                transaction.Rollback();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT [Id],[Firma],[Name],[Straße],[PLZ],[Ort],[Geburtsdatum] FROM [Kunden] where [Id] = 2004";
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        Console.WriteLine($"Name (After) outside transaction {reader["Name"]}");
                    }
                    reader.Close();
                }
            }

            Console.WriteLine("Press any key to stop");
            Console.Read();
        }
    }
}
