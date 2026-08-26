using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Telemetry.Api.Domain.Constants;

namespace Telemetry.Api.Application.DTOs
{
    /// <summary>
    ///     DTO for a single time-saved entry from automated Revit task processing.
    /// </summary>
    public class SavingsEntryDto
    {
        /// <summary>
        ///     Unique key identifying the source queue directory.
        /// </summary>
        [MaxLength(100)]
        [JsonPropertyName(SavingsPropertyNames.QueueKey)]
        public required string QueueKey { get; init; }

        /// <summary>
        ///     Type of savings entry: "payload" for completed tasks or "op" for model operations.
        /// </summary>
        [MaxLength(20)]
        [JsonPropertyName(SavingsPropertyNames.Kind)]
        public required string Kind { get; init; }

        /// <summary>
        ///     Source row ID from the originating task_queue.db.
        /// </summary>
        [JsonPropertyName(SavingsPropertyNames.SourceId)]
        public required long SourceId { get; init; }

        /// <summary>
        ///     When the work occurred (Unix timestamp or ISO 8601).
        /// </summary>
        [JsonPropertyName(SavingsPropertyNames.OccurredAt)]
        public required DateTimeOffset OccurredAt { get; init; }

        /// <summary>
        ///     Descriptive label: payload name for tasks, operation name for model ops.
        /// </summary>
        [MaxLength(250)]
        [JsonPropertyName(SavingsPropertyNames.Label)]
        public required string Label { get; init; }

        /// <summary>
        ///     Manual minutes saved by this entry.
        /// </summary>
        [JsonPropertyName(SavingsPropertyNames.Minutes)]
        public required double Minutes { get; init; }

        /// <summary>
        ///     When this entry was recorded in the local ledger.
        /// </summary>
        [JsonPropertyName(SavingsPropertyNames.RecordedAt)]
        public required DateTimeOffset RecordedAt { get; init; }

        /// <summary>
        ///     Hostname of the machine that ran the work.
        /// </summary>
        [MaxLength(100)]
        [JsonPropertyName(SavingsPropertyNames.Hostname)]
        public string? Hostname { get; init; }

        /// <summary>
        ///     Windows username of the user who ran the work.
        /// </summary>
        [MaxLength(100)]
        [JsonPropertyName(SavingsPropertyNames.Username)]
        public string? Username { get; init; }
    }
}
