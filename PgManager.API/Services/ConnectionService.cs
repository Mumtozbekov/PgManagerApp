// Services/ConnectionService.cs
using System.Data;
using Npgsql;
using PgManager.API.Dtos;
using System.Text.Json;

namespace PgManager.API.Services;

public class ConnectionService
{

    string CreateDbConnectionString(DbConnectionConfigDto config)
    {
        return $"Host={config.Host};Port={config.Port};Database={config.Database};Username={config.Username};Password={config.Password};Pooling=false;Timeout=3";
    }

    string CreatePgConnectionString(PgConnectionConfigDto config)
    {
        return $"Host={config.Host};Port={config.Port};Database=postgres;Username={config.Username};Password={config.Password};Pooling=false;Timeout=3";
    }



    public async Task<bool> TestConnectionAsync(PgConnectionConfigDto config)
    {
        var connString = CreatePgConnectionString(config);
        try
        {
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            return conn.State == ConnectionState.Open;
        }
        catch
        {
            return false;
        }
    }


    public async Task<List<string>> GetDatabasesAsync(PgConnectionConfigDto config)
    {
        var connString = CreatePgConnectionString(config);
        var databases = new List<string>();

        try
        {
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand("SELECT datname FROM pg_database WHERE datistemplate = false", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                databases.Add(reader.GetString(0));
            }

            return databases;
        }
        catch (Exception ex)
        {
            return new List<string>();
        }
    }


    public async Task<List<string>> GetSchemasAsync(DbConnectionConfigDto config)
    {
        var connString = CreateDbConnectionString(config);
        var schemas = new List<string>();

        try
        {
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand("SELECT schema_name FROM information_schema.schemata ORDER BY schema_name", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                schemas.Add(reader.GetString(0));
            }

            return schemas;
        }
        catch
        {
            return new List<string>();
        }
    }


    public async Task<List<string>> GetTablesAsync(SchemaConnectionConfigDto config)
    {
        var connString = CreateDbConnectionString(config);
        var tables = new List<string>();

        try
        {
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = @schema AND table_type = 'BASE TABLE'
            ORDER BY table_name", conn);
            cmd.Parameters.AddWithValue("schema", config.Schema ?? "public");

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }

            return tables;
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<List<TableColumnInfoDto>> GetTableColumnsAsync(SchemaConnectionConfigDto config, string tableName)
    {
        var connString = CreateDbConnectionString(config);
        var columns = new List<TableColumnInfoDto>();

        try
        {
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(@"
            SELECT column_name, data_type, is_nullable, column_default
            FROM information_schema.columns
            WHERE table_schema = @schema AND table_name = @table
            ORDER BY ordinal_position", conn);

            cmd.Parameters.AddWithValue("schema", config.Schema ?? "public");
            cmd.Parameters.AddWithValue("table", tableName);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add(new TableColumnInfoDto
                {
                    ColumnName = reader.GetString(0),
                    DataType = reader.GetString(1),
                    IsNullable = reader.GetString(2) == "YES",
                    DefaultValue = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return columns;
        }
        catch
        {
            return new List<TableColumnInfoDto>();
        }
    }

    public async Task<List<TableConstraintInfoDto>> GetTableConstraintsAsync(SchemaConnectionConfigDto config, string tableName)
    {
        var constraints = new List<TableConstraintInfoDto>();
        var connString = CreateDbConnectionString(config);
        try
        {
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            var sql = @"
            SELECT 
                tc.table_name, 
                tc.constraint_name, 
                kcu.column_name, 
                tc.constraint_type
            FROM 
                information_schema.table_constraints AS tc
            JOIN 
                information_schema.key_column_usage AS kcu
                ON tc.constraint_name = kcu.constraint_name 
                AND tc.constraint_schema = kcu.constraint_schema
            WHERE 
                tc.table_schema = @schema AND 
                tc.table_name = @table;
        ";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("schema", config.Schema);
            cmd.Parameters.AddWithValue("table", tableName);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                constraints.Add(new TableConstraintInfoDto
                {
                    TableName = reader.GetString(0),
                    ConstraintName = reader.GetString(1),
                    ColumnName = reader.GetString(2),
                    ConstraintType = reader.GetString(3)
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Xatolik: " + ex.Message);
        }

        return constraints;
    }

    public async Task<DbInfoTreeNode> GetDbInfos(PgConnectionConfigDto config)
    {
        DbInfoTreeNode tree = new DbInfoTreeNode("Databases");
        var databasesList = await GetDatabasesAsync(config);

        foreach (var db in databasesList)
        {
            var dbNode = new DbInfoTreeNode(db, "db");
            var dbConfig = DbConnectionConfigDto.Create(config);
            dbConfig.Database = db;
            var schemas = await GetSchemasAsync(dbConfig);
            if (schemas != null)
            {
                var schemasNode = new DbInfoTreeNode("Schemas", "sch") { DbName = dbNode.Name };
                foreach (var schema in schemas)
                {
                    var schemaNode = new DbInfoTreeNode(schema, "sch") { DbName = dbNode.Name };
                    schemasNode.Children.Add(schemaNode);
                }
                dbNode.Children.Add(schemasNode);
            }
            tree.Children.Add(dbNode);

        }


        return tree;
    }
    public async Task<List<DbInfoTreeNode>> GetTableTree(SchemaConnectionConfigDto config)
    {
        List<DbInfoTreeNode> tables = new();
        var tablesNames = await GetTablesAsync(config);

        foreach (var tableName in tablesNames)
        {
            var tableNode = new DbInfoTreeNode(tableName, "tb");
            var columnsInfos = await GetTableColumnsAsync(config, tableName);
            var columns = new DbInfoTreeNode("Columns", "col");
            if (columnsInfos != null)
            {
                foreach (var columnInfo in columnsInfos)
                    columns.Children.Add(new(columnInfo.ColumnName, "") { ColumnInfo = columnInfo });
            }
            tableNode.Children.Add(columns);

            var constraintInfos = await GetTableConstraintsAsync(config, tableName);
            var constraints = new DbInfoTreeNode("Constraints", "cons");
            if (constraintInfos != null)
            {
                foreach (var constraintInfo in constraintInfos)
                    constraints.Children.Add(new(constraintInfo.ConstraintName, "") {DbName=config.Database, ColumnInfo = new TableColumnInfoDto() { ColumnName = constraintInfo.ColumnName } }); ;
            }
            tableNode.Children.Add(constraints);

            tables.Add(tableNode);

        }


        return tables;
    }


    public async Task<SqlQueryResult> ExecuteSqlAsync(SqlQueryRequest request)
    {
        var connStr = CreateDbConnectionString(request);
        var result = new SqlQueryResult();

        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();
            // COUNT query
            var countSql = $"SELECT COUNT(*) FROM ({request.Sql}) AS sub";
            await using (var countCmd = new NpgsqlCommand(countSql, conn))
            {
                var count = await countCmd.ExecuteScalarAsync();
                result.TotalCount = Convert.ToInt32(count);
            }

            var pagedSql = $"{request.Sql} LIMIT {request.Limit} OFFSET {request.Offset}";
            await using var cmd = new NpgsqlCommand(pagedSql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                result.Columns.Add(reader.GetName(i));
            }

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                }
                result.Rows.Add(row);
            }
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
        }

        return result;
    }

}
