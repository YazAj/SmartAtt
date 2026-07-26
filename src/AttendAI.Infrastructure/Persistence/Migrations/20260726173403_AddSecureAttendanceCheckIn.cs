using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSecureAttendanceCheckIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Classrooms_Latitude",
                table: "Classrooms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Classrooms_Longitude",
                table: "Classrooms");

            migrationBuilder.AddColumn<bool>(
                name: "AttendanceCheckInEnabled",
                table: "LectureSessions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AttendanceLatitude",
                table: "LectureSessions",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AttendanceLongitude",
                table: "LectureSessions",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FaceVerificationRequired",
                table: "LectureSessions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "LocationVerificationRequired",
                table: "LectureSessions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "MaximumAcceptedAccuracyMeters",
                table: "LectureSessions",
                type: "int",
                nullable: false,
                defaultValue: 75);

            migrationBuilder.CreateTable(
                name: "AttendanceAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LectureSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FaceVerificationAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Outcome = table.Column<int>(type: "int", nullable: false),
                    FailureReason = table.Column<int>(type: "int", nullable: false),
                    LocationOutcome = table.Column<int>(type: "int", nullable: false),
                    AttemptedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IdempotencyKeyHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ChallengeTokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SafeDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DistanceMeters = table.Column<decimal>(type: "decimal(9,3)", nullable: true),
                    BrowserAccuracyMeters = table.Column<decimal>(type: "decimal(9,3)", nullable: true),
                    AllowedRadiusMeters = table.Column<int>(type: "int", nullable: true),
                    MaximumAcceptedAccuracyMeters = table.Column<int>(type: "int", nullable: true),
                    ProcessingDurationMilliseconds = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceAttempts", x => x.Id);
                    table.CheckConstraint("CK_AttendanceAttempts_AllowedRadiusMeters", "[AllowedRadiusMeters] IS NULL OR [AllowedRadiusMeters] >= 0");
                    table.CheckConstraint("CK_AttendanceAttempts_BrowserAccuracyMeters", "[BrowserAccuracyMeters] IS NULL OR [BrowserAccuracyMeters] >= 0");
                    table.CheckConstraint("CK_AttendanceAttempts_DistanceMeters", "[DistanceMeters] IS NULL OR [DistanceMeters] >= 0");
                    table.CheckConstraint("CK_AttendanceAttempts_MaximumAcceptedAccuracyMeters", "[MaximumAcceptedAccuracyMeters] IS NULL OR [MaximumAcceptedAccuracyMeters] >= 0");
                    table.CheckConstraint("CK_AttendanceAttempts_ProcessingDuration", "[ProcessingDurationMilliseconds] IS NULL OR [ProcessingDurationMilliseconds] >= 0");
                    table.ForeignKey(
                        name: "FK_AttendanceAttempts_FaceVerificationAttempts_FaceVerificationAttemptId",
                        column: x => x.FaceVerificationAttemptId,
                        principalTable: "FaceVerificationAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceAttempts_LectureSessions_LectureSessionId",
                        column: x => x.LectureSessionId,
                        principalTable: "LectureSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceAttempts_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceChallenges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LectureSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IssuedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ConsumedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ConsumedByAttendanceAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceChallenges", x => x.Id);
                    table.CheckConstraint("CK_AttendanceChallenges_ExpiresAfterIssued", "[ExpiresAtUtc] > [IssuedAtUtc]");
                    table.ForeignKey(
                        name: "FK_AttendanceChallenges_LectureSessions_LectureSessionId",
                        column: x => x.LectureSessionId,
                        principalTable: "LectureSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceChallenges_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LectureSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttendanceAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FaceVerificationAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedInAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllowedRadiusMeters = table.Column<int>(type: "int", nullable: false),
                    MaximumAcceptedAccuracyMeters = table.Column<int>(type: "int", nullable: false),
                    DistanceMeters = table.Column<decimal>(type: "decimal(9,3)", nullable: false),
                    BrowserAccuracyMeters = table.Column<decimal>(type: "decimal(9,3)", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                    table.CheckConstraint("CK_AttendanceRecords_AllowedRadiusMeters", "[AllowedRadiusMeters] >= 0");
                    table.CheckConstraint("CK_AttendanceRecords_BrowserAccuracyMeters", "[BrowserAccuracyMeters] >= 0");
                    table.CheckConstraint("CK_AttendanceRecords_DistanceMeters", "[DistanceMeters] >= 0");
                    table.CheckConstraint("CK_AttendanceRecords_MaximumAcceptedAccuracyMeters", "[MaximumAcceptedAccuracyMeters] >= 0");
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_AttendanceAttempts_AttendanceAttemptId",
                        column: x => x.AttendanceAttemptId,
                        principalTable: "AttendanceAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_FaceVerificationAttempts_FaceVerificationAttemptId",
                        column: x => x.FaceVerificationAttemptId,
                        principalTable: "FaceVerificationAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_LectureSessions_LectureSessionId",
                        column: x => x.LectureSessionId,
                        principalTable: "LectureSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_LectureSessions_AttendanceCoordinatePair",
                table: "LectureSessions",
                sql: "([AttendanceLatitude] IS NULL AND [AttendanceLongitude] IS NULL) OR ([AttendanceLatitude] IS NOT NULL AND [AttendanceLongitude] IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LectureSessions_AttendanceLatitude",
                table: "LectureSessions",
                sql: "[AttendanceLatitude] IS NULL OR (CAST([AttendanceLatitude] AS REAL) BETWEEN -90 AND 90)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LectureSessions_AttendanceLongitude",
                table: "LectureSessions",
                sql: "[AttendanceLongitude] IS NULL OR (CAST([AttendanceLongitude] AS REAL) BETWEEN -180 AND 180)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LectureSessions_MaximumAcceptedAccuracyMeters",
                table: "LectureSessions",
                sql: "[MaximumAcceptedAccuracyMeters] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Classrooms_Latitude",
                table: "Classrooms",
                sql: "[Latitude] IS NULL OR (CAST([Latitude] AS REAL) BETWEEN -90 AND 90)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Classrooms_Longitude",
                table: "Classrooms",
                sql: "[Longitude] IS NULL OR (CAST([Longitude] AS REAL) BETWEEN -180 AND 180)");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceAttempts_FaceVerificationAttemptId",
                table: "AttendanceAttempts",
                column: "FaceVerificationAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceAttempts_Session_AttemptedAtUtc",
                table: "AttendanceAttempts",
                columns: new[] { "LectureSessionId", "AttemptedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceAttempts_Student_AttemptedAtUtc",
                table: "AttendanceAttempts",
                columns: new[] { "StudentId", "AttemptedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_AttendanceAttempts_Student_Session_Idempotency",
                table: "AttendanceAttempts",
                columns: new[] { "StudentId", "LectureSessionId", "IdempotencyKeyHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceChallenges_LectureSessionId",
                table: "AttendanceChallenges",
                column: "LectureSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceChallenges_Student_Session_Expires",
                table: "AttendanceChallenges",
                columns: new[] { "StudentId", "LectureSessionId", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_AttendanceChallenges_TokenHash",
                table: "AttendanceChallenges",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_ClassroomId",
                table: "AttendanceRecords",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_Session_Status",
                table: "AttendanceRecords",
                columns: new[] { "LectureSessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_StudentId",
                table: "AttendanceRecords",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "UX_AttendanceRecords_AttendanceAttemptId",
                table: "AttendanceRecords",
                column: "AttendanceAttemptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_AttendanceRecords_FaceVerificationAttemptId",
                table: "AttendanceRecords",
                column: "FaceVerificationAttemptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_AttendanceRecords_Session_Student",
                table: "AttendanceRecords",
                columns: new[] { "LectureSessionId", "StudentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceChallenges");

            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "AttendanceAttempts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LectureSessions_AttendanceCoordinatePair",
                table: "LectureSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LectureSessions_AttendanceLatitude",
                table: "LectureSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LectureSessions_AttendanceLongitude",
                table: "LectureSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LectureSessions_MaximumAcceptedAccuracyMeters",
                table: "LectureSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Classrooms_Latitude",
                table: "Classrooms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Classrooms_Longitude",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "AttendanceCheckInEnabled",
                table: "LectureSessions");

            migrationBuilder.DropColumn(
                name: "AttendanceLatitude",
                table: "LectureSessions");

            migrationBuilder.DropColumn(
                name: "AttendanceLongitude",
                table: "LectureSessions");

            migrationBuilder.DropColumn(
                name: "FaceVerificationRequired",
                table: "LectureSessions");

            migrationBuilder.DropColumn(
                name: "LocationVerificationRequired",
                table: "LectureSessions");

            migrationBuilder.DropColumn(
                name: "MaximumAcceptedAccuracyMeters",
                table: "LectureSessions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Classrooms_Latitude",
                table: "Classrooms",
                sql: "[Latitude] IS NULL OR ([Latitude] BETWEEN -90 AND 90)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Classrooms_Longitude",
                table: "Classrooms",
                sql: "[Longitude] IS NULL OR ([Longitude] BETWEEN -180 AND 180)");
        }
    }
}
