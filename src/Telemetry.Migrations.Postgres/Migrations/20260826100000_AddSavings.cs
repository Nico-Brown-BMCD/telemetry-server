using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telemetry.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddSavings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "savings",
                columns: table => new
                {
                    _id = table.Column<Guid>(type: "uuid", nullable: false),
                    queue_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    source_id = table.Column<long>(type: "bigint", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    label = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    minutes = table.Column<double>(type: "double precision", nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    hostname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_savings", x => x._id);
                });

            // Unique constraint for idempotent upsert (matches task-queue's dedup logic)
            migrationBuilder.CreateIndex(
                name: "UQ_savings_entry",
                table: "savings",
                columns: new[] { "queue_key", "kind", "source_id", "occurred_at" },
                unique: true);

            // Fast aggregation by queue
            migrationBuilder.CreateIndex(
                name: "IX_savings_queue_key",
                table: "savings",
                column: "queue_key");

            // Fast time-range queries
            migrationBuilder.CreateIndex(
                name: "IX_savings_occurred_at",
                table: "savings",
                column: "occurred_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "savings");
        }
    }
}
