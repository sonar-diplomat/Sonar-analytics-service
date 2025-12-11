using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserIdTrackIdContextIdToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop indexes first
            migrationBuilder.DropIndex(
                name: "idx_user_events_user",
                table: "user_events");

            migrationBuilder.DropIndex(
                name: "idx_user_events_track",
                table: "user_events");

            // Clear existing data since UUID cannot be converted to integer
            // If you need to preserve data, you'll need to export it first and reimport after migration
            migrationBuilder.Sql("DELETE FROM user_events;");

            // Alter columns from uuid to integer using raw SQL
            // For nullable columns, we can use USING NULL
            migrationBuilder.Sql(@"
                ALTER TABLE user_events 
                ALTER COLUMN track_id TYPE integer USING NULL;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE user_events 
                ALTER COLUMN context_id TYPE integer USING NULL;
            ");

            // For NOT NULL column, we need to use a default value or drop NOT NULL first
            // Since we cleared the data, we can change type directly
            migrationBuilder.Sql(@"
                ALTER TABLE user_events 
                ALTER COLUMN user_id DROP NOT NULL;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE user_events 
                ALTER COLUMN user_id TYPE integer USING 0;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE user_events 
                ALTER COLUMN user_id SET NOT NULL;
            ");

            // Recreate indexes
            migrationBuilder.CreateIndex(
                name: "idx_user_events_user",
                table: "user_events",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_events_track",
                table: "user_events",
                column: "track_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "idx_user_events_user",
                table: "user_events");

            migrationBuilder.DropIndex(
                name: "idx_user_events_track",
                table: "user_events");

            // Clear existing data since integer cannot be converted to UUID
            migrationBuilder.Sql("DELETE FROM user_events;");

            // Revert columns back to uuid using raw SQL
            migrationBuilder.Sql(@"
                ALTER TABLE user_events 
                ALTER COLUMN track_id TYPE uuid USING NULL;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE user_events 
                ALTER COLUMN context_id TYPE uuid USING NULL;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE user_events 
                ALTER COLUMN user_id DROP NOT NULL;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE user_events 
                ALTER COLUMN user_id TYPE uuid USING gen_random_uuid();
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE user_events 
                ALTER COLUMN user_id SET NOT NULL;
            ");

            // Recreate indexes
            migrationBuilder.CreateIndex(
                name: "idx_user_events_user",
                table: "user_events",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_events_track",
                table: "user_events",
                column: "track_id");
        }
    }
}

