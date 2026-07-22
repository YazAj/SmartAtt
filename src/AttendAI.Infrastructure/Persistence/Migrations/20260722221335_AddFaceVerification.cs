using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFaceVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FaceVerificationAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FaceTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VerificationPurpose = table.Column<int>(type: "int", nullable: false),
                    Outcome = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Score = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Threshold = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    ScoreMetric = table.Column<int>(type: "int", nullable: false),
                    EngineName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EngineVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TemplateFormatVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    AttemptedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ClientRequestId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SafeDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ImageWidth = table.Column<int>(type: "int", nullable: true),
                    ImageHeight = table.Column<int>(type: "int", nullable: true),
                    DetectedFaceCount = table.Column<int>(type: "int", nullable: true),
                    QualityScore = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    ProcessingDurationMilliseconds = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceVerificationAttempts", x => x.Id);
                    table.CheckConstraint("CK_FaceVerificationAttempts_DetectedFaceCount", "[DetectedFaceCount] IS NULL OR [DetectedFaceCount] >= 0");
                    table.CheckConstraint("CK_FaceVerificationAttempts_ImageHeight", "[ImageHeight] IS NULL OR [ImageHeight] >= 0");
                    table.CheckConstraint("CK_FaceVerificationAttempts_ImageWidth", "[ImageWidth] IS NULL OR [ImageWidth] >= 0");
                    table.CheckConstraint("CK_FaceVerificationAttempts_ProcessingDuration", "[ProcessingDurationMilliseconds] IS NULL OR [ProcessingDurationMilliseconds] >= 0");
                    table.CheckConstraint("CK_FaceVerificationAttempts_QualityScore", "[QualityScore] IS NULL OR ([QualityScore] >= 0 AND [QualityScore] <= 1)");
                    table.CheckConstraint("CK_FaceVerificationAttempts_Score", "[Score] IS NULL OR [Score] >= 0");
                    table.CheckConstraint("CK_FaceVerificationAttempts_Threshold", "[Threshold] >= 0");
                    table.ForeignKey(
                        name: "FK_FaceVerificationAttempts_StudentFaceTemplates_FaceTemplateId",
                        column: x => x.FaceTemplateId,
                        principalTable: "StudentFaceTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaceVerificationAttempts_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaceVerificationAttempts_FaceTemplateId",
                table: "FaceVerificationAttempts",
                column: "FaceTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_FaceVerificationAttempts_Outcome_AttemptedAtUtc",
                table: "FaceVerificationAttempts",
                columns: new[] { "Outcome", "AttemptedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FaceVerificationAttempts_StudentId_AttemptedAtUtc",
                table: "FaceVerificationAttempts",
                columns: new[] { "StudentId", "AttemptedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_FaceVerificationAttempts_StudentId_ClientRequestId",
                table: "FaceVerificationAttempts",
                columns: new[] { "StudentId", "ClientRequestId" },
                unique: true,
                filter: "[ClientRequestId] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaceVerificationAttempts");
        }
    }
}
