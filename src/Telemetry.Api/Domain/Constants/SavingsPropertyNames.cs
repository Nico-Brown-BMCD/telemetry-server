namespace Telemetry.Api.Domain.Constants
{
    /// <summary>
    ///     Property names for savings-related data.
    /// </summary>
    public static class SavingsPropertyNames
    {
        /// <summary>Queue key identifier.</summary>
        public const string QueueKey = "queue_key";

        /// <summary>Entry kind (payload or op).</summary>
        public const string Kind = "kind";

        /// <summary>Source row ID from task_queue.db.</summary>
        public const string SourceId = "source_id";

        /// <summary>When the work occurred.</summary>
        public const string OccurredAt = "occurred_at";

        /// <summary>Descriptive label (payload name or operation name).</summary>
        public const string Label = "label";

        /// <summary>Manual minutes saved.</summary>
        public const string Minutes = "minutes";

        /// <summary>When the entry was recorded.</summary>
        public const string RecordedAt = "recorded_at";

        /// <summary>Machine hostname.</summary>
        public const string Hostname = "hostname";

        /// <summary>Windows username.</summary>
        public const string Username = "username";
    }
}
