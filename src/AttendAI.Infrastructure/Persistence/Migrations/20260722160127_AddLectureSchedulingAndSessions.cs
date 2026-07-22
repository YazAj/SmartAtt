using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLectureSchedulingAndSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LectureSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstructorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: false),
                    DefaultLateThresholdMinutes = table.Column<int>(type: "int", nullable: false),
                    DefaultAllowedRadiusMeters = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LectureSchedules", x => x.Id);
                    table.CheckConstraint("CK_LectureSchedules_AllowedRadius", "[DefaultAllowedRadiusMeters] >= 0");
                    table.CheckConstraint("CK_LectureSchedules_EffectiveRange", "[EffectiveFrom] <= [EffectiveTo]");
                    table.CheckConstraint("CK_LectureSchedules_LateThreshold", "[DefaultLateThresholdMinutes] >= 0");
                    table.CheckConstraint("CK_LectureSchedules_TimeRange", "[StartTime] < [EndTime]");
                    table.ForeignKey(
                        name: "FK_LectureSchedules_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LectureSchedules_Instructors_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LectureSchedules_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LectureSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LectureScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstructorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ScheduledStartUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ScheduledEndUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActualStartUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActualEndUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SessionCodeHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ProtectedSessionCode = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    SessionCodeExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SessionCodeVersion = table.Column<int>(type: "int", nullable: false),
                    LateThresholdMinutes = table.Column<int>(type: "int", nullable: false),
                    AllowedRadiusMeters = table.Column<int>(type: "int", nullable: false),
                    StartedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EndedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CancelledByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    EndReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LectureSessions", x => x.Id);
                    table.CheckConstraint("CK_LectureSessions_AllowedRadius", "[AllowedRadiusMeters] >= 0");
                    table.CheckConstraint("CK_LectureSessions_LateThreshold", "[LateThresholdMinutes] >= 0");
                    table.CheckConstraint("CK_LectureSessions_ScheduledRange", "[ScheduledStartUtc] < [ScheduledEndUtc]");
                    table.ForeignKey(
                        name: "FK_LectureSessions_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LectureSessions_Instructors_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LectureSessions_LectureSchedules_LectureScheduleId",
                        column: x => x.LectureScheduleId,
                        principalTable: "LectureSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LectureSessions_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LectureSessionEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LectureSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PerformedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PreviousStatus = table.Column<int>(type: "int", nullable: true),
                    NewStatus = table.Column<int>(type: "int", nullable: true),
                    SafeDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LectureSessionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LectureSessionEvents_LectureSessions_LectureSessionId",
                        column: x => x.LectureSessionId,
                        principalTable: "LectureSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LectureSchedules_ClassroomId_DayOfWeek_IsActive_EffectiveFrom_EffectiveTo_StartTime_EndTime",
                table: "LectureSchedules",
                columns: new[] { "ClassroomId", "DayOfWeek", "IsActive", "EffectiveFrom", "EffectiveTo", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_LectureSchedules_InstructorId_DayOfWeek_IsActive_EffectiveFrom_EffectiveTo_StartTime_EndTime",
                table: "LectureSchedules",
                columns: new[] { "InstructorId", "DayOfWeek", "IsActive", "EffectiveFrom", "EffectiveTo", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_LectureSchedules_SectionId_DayOfWeek_IsActive_EffectiveFrom_EffectiveTo_StartTime_EndTime",
                table: "LectureSchedules",
                columns: new[] { "SectionId", "DayOfWeek", "IsActive", "EffectiveFrom", "EffectiveTo", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_LectureSessionEvents_LectureSessionId_OccurredAtUtc",
                table: "LectureSessionEvents",
                columns: new[] { "LectureSessionId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LectureSessions_ClassroomId_Status",
                table: "LectureSessions",
                columns: new[] { "ClassroomId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LectureSessions_InstructorId_Status",
                table: "LectureSessions",
                columns: new[] { "InstructorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LectureSessions_LectureScheduleId_SessionDate",
                table: "LectureSessions",
                columns: new[] { "LectureScheduleId", "SessionDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LectureSessions_SectionId_Status",
                table: "LectureSessions",
                columns: new[] { "SectionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LectureSessions_SessionCodeExpiresAtUtc",
                table: "LectureSessions",
                column: "SessionCodeExpiresAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LectureSessionEvents");

            migrationBuilder.DropTable(
                name: "LectureSessions");

            migrationBuilder.DropTable(
                name: "LectureSchedules");
        }
    }
}
