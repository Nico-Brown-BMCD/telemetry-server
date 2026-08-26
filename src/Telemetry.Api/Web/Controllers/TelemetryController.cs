using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telemetry.Api.Application.Mappings;
using Telemetry.Api.Application.DTOs;
using Telemetry.Api.Application.Interfaces;
using Telemetry.Api.Domain.Models;
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