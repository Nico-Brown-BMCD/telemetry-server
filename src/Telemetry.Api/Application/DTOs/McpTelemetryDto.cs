using System.Text.Json;
using System.Text.Json.Serialization;

namespace Telemetry.Api.Application.DTOs
{
    /// <summary>
    /// MCP tool-call telemetry payload for the Azure SQL table contract.
    /// </summary>
    public class McpTelemetryDto
    {
        /// <summary>
        /// The MCP session identifier.
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string? SessionId { get; init; }

        /// <summary>
        /// The originating event timestamp.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTimeOffset? Timestamp { get; init; }

        /// <summary>
        /// User name for the active request.
        /// </summary>
        [JsonPropertyName("username")]
        public string? Username { get; init; }

        /// <summary>
        /// Host user name associated with the request.
        /// </summary>
        [JsonPropertyName("host_user")]
        public string? HostUser { get; init; }

        /// <summary>
        /// Host machine name.
        /// </summary>
        [JsonPropertyName("hostname")]
        public string? Hostname { get; init; }

        /// <summary>
        /// MCP server name.
        /// </summary>
        [JsonPropertyName("mcp_server")]
        public string? McpServer { get; init; }

        /// <summary>
        /// Server version string.
        /// </summary>
        [JsonPropertyName("server_version")]
        public string? ServerVersion { get; init; }

        /// <summary>
        /// Transport used for the request.
        /// </summary>
        [JsonPropertyName("transport")]
        public string? Transport { get; init; }

        /// <summary>
        /// Client application name.
        /// </summary>
        [JsonPropertyName("client_name")]
        public string? ClientName { get; init; }

        /// <summary>
        /// Client application version.
        /// </summary>
        [JsonPropertyName("client_version")]
        public string? ClientVersion { get; init; }

        /// <summary>
        /// Tool name invoked.
        /// </summary>
        [JsonPropertyName("tool_name")]
        public string? ToolName { get; init; }

        /// <summary>
        /// Tool namespace.
        /// </summary>
        [JsonPropertyName("tool_namespace")]
        public string? ToolNamespace { get; init; }

        /// <summary>
        /// Unique request identifier.
        /// </summary>
        [JsonPropertyName("request_id")]
        public string? RequestId { get; init; }

        /// <summary>
        /// Execution timestamp.
        /// </summary>
        [JsonPropertyName("exec_timestamp")]
        public DateTimeOffset? ExecTimestamp { get; init; }

        /// <summary>
        /// Duration in milliseconds.
        /// </summary>
        [JsonPropertyName("duration_ms")]
        public int? DurationMs { get; init; }

        /// <summary>
        /// Result code.
        /// </summary>
        [JsonPropertyName("resultcode")]
        public int? ResultCode { get; init; }

        /// <summary>
        /// Whether the call succeeded.
        /// </summary>
        [JsonPropertyName("success")]
        public bool? Success { get; init; }

        /// <summary>
        /// A compact summary of the input.
        /// </summary>
        [JsonPropertyName("input_summary")]
        public string? InputSummary { get; init; }

        /// <summary>
        /// A compact summary of the result.
        /// </summary>
        [JsonPropertyName("result_summary")]
        public string? ResultSummary { get; init; }

        /// <summary>
        /// Error message, when available.
        /// </summary>
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Event type.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        /// <summary>
        /// Environment label.
        /// </summary>
        [JsonPropertyName("environment")]
        public string? Environment { get; init; }

        /// <summary>
        /// JSON config payload.
        /// </summary>
        [JsonPropertyName("config")]
        public JsonElement? Config { get; init; }

        /// <summary>
        /// JSON metadata payload.
        /// </summary>
        [JsonPropertyName("metadata")]
        public JsonElement? Metadata { get; init; }
    }
}
