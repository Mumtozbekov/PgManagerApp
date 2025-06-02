namespace PgManager.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using PgManager.API.Dtos;
using PgManager.API.Services;

[ApiController]
[Route("api/[controller]")]
public class ConnectionController : ControllerBase
{
    private readonly ConnectionService _connectionService;

    public ConnectionController()
    {
        _connectionService = new ConnectionService(); 
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestConnection([FromBody] PgConnectionConfigDto config)
    {
        var isConnected = await _connectionService.TestConnectionAsync(config);
        return Ok(new { success = isConnected });
    }
    [HttpPost("get-dbtree")]
    public async Task<IActionResult> GetdbInfo([FromBody] PgConnectionConfigDto config)
    {
        var dbInfo = await _connectionService.GetDbInfos(config);
        return Ok(dbInfo);
    }

    [HttpPost("get-table-tree")]
    public async Task<IActionResult> GetTableInfo([FromBody] SchemaConnectionConfigDto config)
    {
        var dbInfo = await _connectionService.GetTableTree(config);
        return Ok(dbInfo);
    }


    [HttpPost("databases")]
    public async Task<IActionResult> GetDatabases([FromBody] PgConnectionConfigDto config)
    {
        var result = await _connectionService.GetDatabasesAsync(config);
        return Ok(result);
    }

    [HttpPost("schemas")]
    public async Task<IActionResult> GetSchemas([FromBody] DbConnectionConfigDto config)
    {
        var result = await _connectionService.GetSchemasAsync(config);
        return Ok(result);
    }

    [HttpPost("tables")]
    public async Task<IActionResult> GetTables([FromBody] SchemaConnectionConfigDto config)
    {
        var result = await _connectionService.GetTablesAsync(config);
        return Ok(result);
    }

    [HttpPost("table-columns")]
    public async Task<IActionResult> GetTableColumns([FromBody] TableRequestDto request)
    {
        var result = await _connectionService.GetTableColumnsAsync(request, request.TableName);
        return Ok(result);
    }
    [HttpPost("table-constraints")]
    public async Task<IActionResult> GetTableConstraints([FromBody] TableRequestDto request)
    {
        var result = await _connectionService.GetTableConstraintsAsync(request, request.TableName);
        Console.WriteLine(request.TableName);
        return Ok(result);
    }

    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteSql([FromBody] SqlQueryRequest request)
    {
        var result = await _connectionService.ExecuteSqlAsync(request);
        return Ok(result);
    }
}