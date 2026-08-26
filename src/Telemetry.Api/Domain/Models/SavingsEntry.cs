using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson.Serialization.Attributes;
using Telemetry.Api.Domain.Constants;

namespace Telemetry.Api.Domain.Models
{
    /// <summary>
    ///     Represents a time-saved entry from automated Revit task processing.
    ///     Each entry records manual effort avoided by running a payload or model operation.
    /// </summary>
    [Table("savings")]
    public class SavingsEntry
    {
        /// <summary>
        ///     Unique identifier for this entry.
        /// </summary>
        [Key]
        [Column(PropertyNames.Id)]
        public Guid Id { get; init; }

        /// <summary>
        ///     Unique key identifying the source queue directory.
        ///     Derived from the queue directory path hash.
        /// </summary>
        [MaxLength(100)]
        [Column(SavingsPropertyNames.QueueKey)]
        public required string QueueKey { get; init; }

        /// <summary>
        ///     Type of savings entry: "payload" for completed tasks or "op" for model operations.
        /// </summary>
        [MaxLength(20)]
        [Column(SavingsPropertyNames.Kind)]
        public required string Kind { get; init; }

        /// <summary>
        ///     Source row ID from the originating task_queue.db (tasks.id or model_operations.id).
        /// </summary>
        [Column(SavingsPropertyNames.SourceId)]
        public long SourceId { get; init; }

        /// <summary>
        ///     When the work occurred (tasks.completed_at or model_operations.timestamp).
        /// </summary>
        [Column(SavingsPropertyNames.OccurredAt)]
        public DateTimeOffset OccurredAt { get; init; }

        /// <summary>
        ///     Descriptive label: payload name for tasks, operation name for model ops.
        /// </summary>
        [MaxLength(250)]
        [Column(SavingsPropertyNames.Label)]
        public required string Label { get; init; }

        /// <summary>
        ///     Manual minutes saved by this entry.
        /// </summary>
        [Column(SavingsPropertyNames.Minutes)]
        public double Minutes { get; init; }

        /// <summary>
        ///     When this entry was recorded in the ledger.
        /// </summary>
        [Column(SavingsPropertyNames.RecordedAt)]
        public DateTimeOffset RecordedAt { get; init; }

        /// <summary>
        ///     Hostname of the machine that ran the work.
        /// </summary>
        [MaxLength(100)]
        [Column(SavingsPropertyNames.Hostname)]
        public string? Hostname { get; init; }

        /// <summary>
        ///     Windows username of the user who ran the work.
        /// </summary>
        [MaxLength(100)]
        [Column(SavingsPropertyNames.Username)]
        public string? Username { get; init; }
    }
}
