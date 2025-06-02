using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using PgManager.Models;

namespace PgManager.Services
{
    public class ApiService
    {
        private HttpClient _httpClient;

        public void CreateHttpClient(ConnectionConfigsModel connectionConfigs = null)
        {
            _httpClient = new HttpClient() { BaseAddress = new Uri(connectionConfigs?.ApiUrl ?? Settings.Default.BaseUrl) };
        }



        public ApiService()
        {
            CreateHttpClient();
        }


        internal async Task<bool> CheckConnection(ConnectionConfigsModel connectionConfigs)
        {
            try
            {

                var body = new
                {
                    Host = connectionConfigs.DbHost,
                    Port = connectionConfigs.DbPort,
                    Username = connectionConfigs.DbUser,
                    Password = connectionConfigs.DbPassword,
                };
                using var response = await _httpClient.PostAsJsonAsync("/api/connection/test", body);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to post data. Status code: {response.StatusCode}");
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }
        internal async Task<List<string>> GetDatabases()
        {
            try
            {

                var body = new
                {
                    Host = Settings.Default.DbHost,
                    Port = Settings.Default.DbPort,
                    Username = Settings.Default.DbUser,
                    Password = Global.DbPassword,
                };
                using var response = await _httpClient.PostAsJsonAsync("/api/connection/databases", body);
                if (response.IsSuccessStatusCode)
                {
                    var dbs = JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());

                    return dbs;
                }
                else
                {
                    Console.WriteLine($"Failed to post data. Status code: {response.StatusCode}");
                    return new List<string>() { };
                }
            }
            catch
            {
                return new List<string>() { };
            }

        }
        internal async Task<DbInfoTreeNode> GetDbTree()
        {
            try
            {

                var body = new
                {
                    Host = Settings.Default.DbHost,
                    Port = Settings.Default.DbPort,
                    Username = Settings.Default.DbUser,
                    Password = Global.DbPassword,
                };
                using var response = await _httpClient.PostAsJsonAsync("/api/connection/get-dbtree", body);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var dbs = JsonSerializer.Deserialize<DbInfoTreeNode>(json);

                    return dbs;
                }
                else
                {
                    Console.WriteLine($"Failed to post data. Status code: {response.StatusCode}");
                    return null;
                }
            }
            catch
            {
                return null;
            }

        }

        public async Task<List<DbInfoTreeNode>> GetTableTree(string database, string schema)
        {
            try
            {

                var body = new
                {
                    Host = Settings.Default.DbHost,
                    Port = Settings.Default.DbPort,
                    Username = Settings.Default.DbUser,
                    Password = Global.DbPassword,
                    Database = database,
                    Schema = schema
                };
                using var response = await _httpClient.PostAsJsonAsync("/api/connection/get-table-tree", body);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var dbs = JsonSerializer.Deserialize<List<DbInfoTreeNode>>(json);

                    return dbs;
                }
                else
                {
                    Console.WriteLine($"Failed to post data. Status code: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal async Task<SqlQueryResult> SendQuery(string database, string query, int page, int limit)
        {
            try
            {

                var body = new
                {
                    Host = Settings.Default.DbHost,
                    Port = Settings.Default.DbPort,
                    Username = Settings.Default.DbUser,
                    Password = Global.DbPassword,
                    Database = database,
                    Sql = query,
                    Offset = (page - 1) * limit,
                    Limit = limit
                };
                using var response = await _httpClient.PostAsJsonAsync("/api/connection/execute", body);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var dbs = JsonSerializer.Deserialize<SqlQueryResult>(json);

                    return dbs;
                }
                else
                {
                    Console.WriteLine($"Failed to post data. Status code: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
