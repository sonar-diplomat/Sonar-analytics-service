using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    track_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    context_type = table.Column<int>(type: "integer", nullable: false),
                    context_id = table.Column<Guid>(type: "uuid", nullable: true),
                    position_ms = table.Column<int>(type: "integer", nullable: true),
                    duration_ms = table.Column<int>(type: "integer", nullable: true),
                    timestamp_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_user_events_event_type",
                table: "user_events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "idx_user_events_track",
                table: "user_events",
                column: "track_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_events_ts",
                table: "user_events",
                column: "timestamp_utc");

            migrationBuilder.CreateIndex(
                name: "idx_user_events_user",
                table: "user_events",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_events");
        }
    }
}
