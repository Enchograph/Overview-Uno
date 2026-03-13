using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Overview.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSqlInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_chat_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Message = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RequestType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LinkedItemIds = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_chat_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    IsImportant = table.Column<bool>(type: "boolean", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    ReminderConfig = table.Column<string>(type: "jsonb", nullable: false),
                    RepeatRule = table.Column<string>(type: "jsonb", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceDeviceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlannedStartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlannedEndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeadlineAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpectedDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    TargetDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sync_changes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ItemSnapshot = table.Column<string>(type: "jsonb", nullable: true),
                    SettingsSnapshot = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_changes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Language = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ThemeMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ThemePreset = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    WeekStartDay = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    HomeViewMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DayPlanStartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    TimeBlockDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    TimeBlockGapMinutes = table.Column<int>(type: "integer", nullable: false),
                    TimeBlockCount = table.Column<int>(type: "integer", nullable: false),
                    ListPageDefaultTab = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ListPageSortBy = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ListPageTheme = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AiBaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AiApiKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AiModel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SyncServerBaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NotificationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    WidgetPreferences = table.Column<string>(type: "jsonb", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceDeviceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsEmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_chat_messages_UserId_OccurredOn_CreatedAt",
                table: "ai_chat_messages",
                columns: new[] { "UserId", "OccurredOn", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_items_UserId_DeletedAt",
                table: "items",
                columns: new[] { "UserId", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_items_UserId_LastModifiedAt",
                table: "items",
                columns: new[] { "UserId", "LastModifiedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_items_UserId_Type",
                table: "items",
                columns: new[] { "UserId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_sync_changes_UserId_CreatedAt",
                table: "sync_changes",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_sync_changes_UserId_EntityType_EntityId",
                table: "sync_changes",
                columns: new[] { "UserId", "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_sync_changes_UserId_SyncedAt",
                table: "sync_changes",
                columns: new[] { "UserId", "SyncedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_user_settings_UserId",
                table: "user_settings",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_settings_UserId_LastModifiedAt",
                table: "user_settings",
                columns: new[] { "UserId", "LastModifiedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_chat_messages");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "sync_changes");

            migrationBuilder.DropTable(
                name: "user_settings");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
