using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using MySqlConnector;

namespace WindowsServerManager.Components.Libraries.Services
{
    public class DatabaseService : IDisposable
    {
        private readonly MySqlConnection _connection;

        public static string DataTableSystemTextJson(DataTable dataTable)
        {
            if (dataTable.Rows.Count == 0) return "[]";

            IEnumerable<Dictionary<string, object?>> data = dataTable.Rows.OfType<DataRow>()
                .Select(row => dataTable.Columns.OfType<DataColumn>()
                    .ToDictionary(
                        col => col.ColumnName,
                        c =>
                        {
                            object value = row[c];
                            if (value == DBNull.Value)
                                return null;
                            return value is IDictionary { Count: 0 } ? null : value;
                        }
                    )
                );

            return JsonSerializer.Serialize(data);
        }

        public string Query(string query, Dictionary<string, object?>? sqlParams = null)
        {
            using MySqlCommand command = _connection.CreateCommand();
            command.CommandText = query;

            if (sqlParams != null)
                foreach (KeyValuePair<string, object?> sqlParam in sqlParams)
                    command.Parameters.AddWithValue(sqlParam.Key, sqlParam.Value ?? DBNull.Value);

            try
            {
                // If the query is not a SELECT statement, execute the query and return the number of rows affected
                if (!query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) && !query.Contains("RETURNING"))
                {
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected.ToString();
                }

                DataTable dataTable = new();
                dataTable.Load(command.ExecuteReader());
                return DataTableSystemTextJson(dataTable);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return $"FAILURE|{e}";
            }
        }


        public DatabaseService(string username, string password, string database, IPAddress address)
        {
            MySqlConnectionStringBuilder builder = new()
            {
                Server = address.ToString(),
                UserID = username,
                Password = password,
                Database = database,
                ConvertZeroDateTime = true
            };

            Debug.WriteLine(builder.ConnectionString);

            _connection = new MySqlConnection(builder.ConnectionString);
            _connection.Open();
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
