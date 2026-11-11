using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartBuilding.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class TrackOccupancySessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ux_occupancy_sessions_user_floor_open",
                table: "occupancy_sessions",
                columns: new[] { "user_id", "floor_id" },
                unique: true,
                filter: "exited_at IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_occupancy_sessions_exit_not_before_entry",
                table: "occupancy_sessions",
                sql: "exited_at IS NULL OR exited_at >= entered_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_occupancy_sessions_user_floor_open",
                table: "occupancy_sessions");

            migrationBuilder.DropCheckConstraint(
                name: "ck_occupancy_sessions_exit_not_before_entry",
                table: "occupancy_sessions");
        }
    }
}
