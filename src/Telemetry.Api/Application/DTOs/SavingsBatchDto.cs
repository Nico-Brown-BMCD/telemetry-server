using System.Text.Json.Serialization;

namespace Telemetry.Api.Application.DTOs
{
    /// <summary>
    ///     DTO for a batch of savings entries to be recorded.
    /// </summary>
    public class SavingsBatchDto
    {
        /// <summary>
        ///     Information about telemetry record schema.
        /// </summary>
        [JsonPropertyName("meta")]
        public required MetaDto Meta { get; init; }

        /// <summary>
        ///     Array of savings entries to record.
        /// </summary>
        [JsonPropertyName("entries")]
        public required IReadOnlyList<SavingsEntryDto> Entries { get; init; }
    }
}
