using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telemetry.Api.Application.Mappings;
using Telemetry.Api.Application.DTOs;
using Telemetry.Api.Application.Interfaces;
using Telemetry.Api.Domain.Models;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Diagnostics;

namespace Telemetry.Api.Web.Controllers
{
    /// <summary>
    /// Controller
    /// </summary>
    [ApiController]
    [ApiVersion("2")]
    [Route("api/v{version:apiVersion}")]
    public class TelemetryController : ControllerBase
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<TelemetryController> _logger;
        private readonly IServiceInfo _serviceInfo;

        /// <summary>
        /// Creates controller.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="serviceInfo">Service info.</param>
        public TelemetryController(IApplicationDbContext context, ILogger<TelemetryController> logger,
            IServiceInfo serviceInfo)
        {
            _context = context;
            _logger = logger;
            _serviceInfo = serviceInfo;
        }

        /// <summary>
        ///     Adds script record to DB.
        /// </summary>
        /// <param name="dto">Adding script records.</param>
        /// <returns>Returns adding script records task.</returns>
        [HttpPost("scripts")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> PostScript([FromBody] ScriptRecordDto dto)
        {
            try
            {
                CancellationToken ct = HttpContext.RequestAborted;

                if (dto.Meta.SchemaVersion.Major != 2)
                {
                    return BadRequest(new ProblemDetails()
                    {
                        Title = "Unsupported Schema Version",
                        Detail = $"Expected version 2.0.0, but received {dto.Meta.SchemaVersion}",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                ScriptRecord record = dto.ToModel();
                await _context.AddScriptRecord(record, ct);

                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("Script record saved: {ExecId} by {Username}", record.ExecId, record.Username);
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while saving script record");
                return StatusCode(503);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while saving script record");
                return StatusCode(500);
            }
        }

        /// <summary>
        ///     Adds event record to DB.
        /// </summary>
        /// <param name="dto">Adding event records.</param>
        /// <returns>Returns adding event records task.</returns>
        [HttpPost("events")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> PostEvent([FromBody] EventRecordDto dto)
        {
            try
            {
                CancellationToken ct = HttpContext.RequestAborted;
                
                if (dto.Meta.SchemaVersion.Major != 2)
                {
                    return BadRequest(new ProblemDetails()
                    {
                        Title = "Unsupported Schema Version",
                        Detail = $"Expected version 2.0.0, but received {dto.Meta.SchemaVersion}",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                EventRecord record = dto.ToModel();
                await _context.AddEventRecord(record, ct);

                await _context.SaveChangesAsync(ct);
                _logger.LogInformation("Event record saved: {Type} by {Username}", record.EventType, record.Username);
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while saving an event record");
                return StatusCode(503);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while saving event record");
                return StatusCode(500);
            }
        }

        /// <summary>
        ///     Adds savings entries to DB (idempotent batch insert).
        /// </summary>
        /// <param name="dto">Batch of savings entries to record.</param>
        /// <returns>Returns the number of new entries recorded.</returns>
        [HttpPost("savings")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> PostSavings([FromBody] SavingsBatchDto dto)
        {
            try
            {
                CancellationToken ct = HttpContext.RequestAborted;

                if (dto.Meta.SchemaVersion.Major != 2)
                {
                    return BadRequest(new ProblemDetails()
                    {
                        Title = "Unsupported Schema Version",
                        Detail = $"Expected version 2.0.0, but received {dto.Meta.SchemaVersion}",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                if (dto.Entries.Count == 0)
                {
                    return Ok(new { recorded = 0, total = 0 });
                }

                if (dto.Entries.Count > 1000)
                {
                    return BadRequest(new ProblemDetails()
                    {
                        Title = "Batch Too Large",
                        Detail = "Maximum 1000 entries per request.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var sw = Stopwatch.StartNew();

                // Bulk lookup existing keys to avoid per-entry round-trips
                var existingKeys = await _context.GetExistingSavingsKeysAsync(dto.Entries, ct);

                int recorded = 0;
                foreach (var entry in dto.Entries)
                {
                    var key = (entry.QueueKey, entry.Kind, entry.SourceId, entry.OccurredAt);
                    if (!existingKeys.Contains(key))
                    {
                        await _context.AddSavingsEntry(entry.ToModel(), ct);
                        recorded++;
                    }
                }

                await _context.SaveChangesAsync(ct);
                sw.Stop();

                _logger.LogInformation(
                    "Savings batch: {Recorded}/{Total} entries recorded in {ElapsedMs}ms",
                    recorded, dto.Entries.Count, sw.ElapsedMilliseconds);

                return Ok(new { recorded, total = dto.Entries.Count });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while saving savings entries");
                return StatusCode(503);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while saving savings entries");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Inserts an MCP telemetry row and reports database failures.
        /// </summary>
        [HttpPost("mcp-telemetry")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [ProducesResponseType(typeof(ProblemDetails), 503)]
        public async Task<IActionResult> PostMcpTelemetry([FromBody] McpTelemetryDto dto)
        {
            try
            {
                if (dto is null)
                {
                    return BadRequest();
                }

                var normalized = new McpTelemetryDto
                {
                    SessionId = dto.SessionId,
                    Timestamp = dto.Timestamp,
                    Username = dto.Username,
                    HostUser = dto.HostUser,
                    Hostname = dto.Hostname,
                    McpServer = dto.McpServer,
                    ServerVersion = dto.ServerVersion,
                    Transport = dto.Transport,
                    ClientName = dto.ClientName,
                    ClientVersion = dto.ClientVersion,
                    ToolName = dto.ToolName,
                    ToolNamespace = dto.ToolNamespace,
                    RequestId = dto.RequestId,
                    ExecTimestamp = dto.ExecTimestamp,
                    DurationMs = dto.DurationMs,
                    ResultCode = dto.ResultCode ?? 0,
                    Success = dto.Success ?? false,
                    InputSummary = dto.InputSummary,
                    ResultSummary = dto.ResultSummary,
                    ErrorMessage = dto.ErrorMessage,
                    Type = dto.Type ?? "tool_call",
                    Environment = dto.Environment,
                    Config = dto.Config,
                    Metadata = dto.Metadata
                };

                await _context.AddMcpTelemetryAsync(normalized, HttpContext.RequestAborted);
                await _context.SaveChangesAsync(HttpContext.RequestAborted);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MCP telemetry insert failed. TraceId: {TraceId}; RequestId: {RequestId}",
                    HttpContext.TraceIdentifier, dto?.RequestId);

                var cause = ex.GetBaseException();
                var databaseError = cause as DbException;
                var status = databaseError is null ? StatusCodes.Status500InternalServerError : StatusCodes.Status503ServiceUnavailable;
                var problem = new ProblemDetails
                {
                    Title = "MCP telemetry insert failed",
                    Status = status,
                    Detail = "The telemetry write failed. Use the traceId to find the full error in the server logs.",
                    Extensions =
                    {
                        ["traceId"] = HttpContext.TraceIdentifier,
                        ["requestId"] = dto?.RequestId,
                        ["errorType"] = cause.GetType().Name
                    }
                };

                if (databaseError is not null)
                {
                    problem.Extensions["databaseErrorCode"] = databaseError.ErrorCode;
                }

                if (cause is SqlException sqlError)
                {
                    problem.Extensions["sqlErrorNumber"] = sqlError.Number;
                    problem.Extensions["sqlState"] = sqlError.State;
                    problem.Extensions["sqlSeverity"] = sqlError.Class;
                    problem.Extensions["clientConnectionId"] = sqlError.ClientConnectionId;
                    problem.Detail = sqlError.Number switch
                    {
                        208 => "SQL Server could not find a referenced table or object.",
                        229 => "SQL Server denied permission to execute the database operation.",
                        515 => "A required database column received NULL.",
                        245 or 8169 => "SQL Server could not convert a supplied value to the required column type.",
                        8152 or 2628 => "A supplied value exceeds the database column length.",
                        18456 => "SQL Server rejected the database login.",
                        4060 => "SQL Server could not open the configured database.",
                        -2 => "The database operation timed out.",
                        _ => problem.Detail
                    };
                }

                return StatusCode(status, problem);
            }
        }

        /// <summary>
        /// Retrieves the current status.
        /// </summary>
        /// <returns>The current status as a status object.</returns>
        [HttpGet("status")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                CancellationToken ct = HttpContext.RequestAborted;

                bool canConnect = await _context.CanConnectAsync(ct);
                string provider = _context.GetDbProvider();
                string version = await _context.GetDbVersionAsync(ct);

                Dictionary<string, StatusCheckDto> checks = new()
                {
                    {provider, new StatusCheckDto {Status = canConnect ? "pass" : "fail", Version = version}}
                };

                StatusRecordDto status = new()
                {
                    Status = canConnect ? "pass" : "fail",
                    ServiceId = _serviceInfo.ServiceId,
                    Version = typeof(TelemetryController).Assembly.GetName().Version?.ToString() ?? "2.0.0.0",
                    Checks = checks
                };

                return canConnect ? Ok(status) : StatusCode(503, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking status");
                return StatusCode(500);
            }
        }
    }
}