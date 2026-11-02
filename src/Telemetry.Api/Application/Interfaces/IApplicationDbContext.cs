using Telemetry.Api.Application.DTOs;
using Telemetry.Api.Domain.Models;

namespace Telemetry.Api.Application.Interfaces
{
    /// <summary>
    /// Represents the contract for the application's database context.
    /// Defines the operations and entities available for data access in the application.
    /// </summary>
    public interface IApplicationDbContext
    {
        /// <summary>
        /// Asynchronously adds a new event record to the database context.
        /// </summary>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        Task AddEventRecord(EventRecord eventRecord, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously adds a new script record to the database context.
        /// </summary>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        Task AddScriptRecord(ScriptRecord scriptRecord, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously adds a new savings entry to the database context.
        /// </summary>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        Task AddSavingsEntry(SavingsEntry savingsEntry, CancellationToken cancellationToken);

        /// <summary>
        /// Inserts a best-effort MCP telemetry row using the Azure SQL table contract.
        /// </summary>
        Task AddMcpTelemetryAsync(McpTelemetryDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Checks if a savings entry with the given composite key already exists.
        /// </summary>
        /// <returns>True if an entry with matching queue_key, kind, source_id, and occurred_at exists.</returns>
        Task<bool> SavingsEntryExistsAsync(string queueKey, string kind, long sourceId, DateTimeOffset occurredAt, CancellationToken cancellationToken);

        /// <summary>
        /// Returns existing composite keys from the savings table matching the given entries.
        /// </summary>
        Task<HashSet<(string QueueKey, string Kind, long SourceId, DateTimeOffset OccurredAt)>> GetExistingSavingsKeysAsync(
            IReadOnlyList<SavingsEntryDto> entries, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously saves all changes made in the current context to the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.
        /// The task result contains the number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously checks if a connection to the database can be established.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result contains a boolean value indicating whether the connection attempt succeeded.</returns>
        Task<bool> CanConnectAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the database provider being used by the application.
        /// </summary>
        /// <returns>A string representing the name of the database provider.</returns>
        string GetDbProvider();

        /// <summary>
        /// Asynchronously retrieves the database version.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result contains the version string of the database.</returns>
        Task<string> GetDbVersionAsync(CancellationToken cancellationToken);
    }
}