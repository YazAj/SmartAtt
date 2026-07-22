using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBiometricEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BiometricConsents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsentVersion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ConsentTextHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AcceptedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    WithdrawnAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AcceptedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    WithdrawnByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiometricConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BiometricConsents_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentFaceTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BiometricConsentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProtectedTemplate = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TemplateFingerprint = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EngineName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EngineVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TemplateFormatVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EmbeddingDimension = table.Column<int>(type: "int", nullable: false),
                    QualityScore = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    CaptureCount = table.Column<int>(type: "int", nullable: false),
                    TemplateVersion = table.Column<int>(type: "int", nullable: false),
                    EnrolledAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RevokedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RevocationReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RequiresReEnrollment = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFaceTemplates", x => x.Id);
                    table.CheckConstraint("CK_StudentFaceTemplates_CaptureCount", "[CaptureCount] > 0");
                    table.CheckConstraint("CK_StudentFaceTemplates_EmbeddingDimension", "[EmbeddingDimension] > 0");
                    table.CheckConstraint("CK_StudentFaceTemplates_QualityScore", "[QualityScore] >= 0 AND [QualityScore] <= 1");
                    table.CheckConstraint("CK_StudentFaceTemplates_TemplateVersion", "[TemplateVersion] > 0");
                    table.ForeignKey(
                        name: "FK_StudentFaceTemplates_BiometricConsents_BiometricConsentId",
                        column: x => x.BiometricConsentId,
                        principalTable: "BiometricConsents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentFaceTemplates_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FaceEnrollmentEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FaceTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BiometricConsentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PerformedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EngineName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EngineVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SafeDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceEnrollmentEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaceEnrollmentEvents_BiometricConsents_BiometricConsentId",
                        column: x => x.BiometricConsentId,
                        principalTable: "BiometricConsents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaceEnrollmentEvents_StudentFaceTemplates_FaceTemplateId",
                        column: x => x.FaceTemplateId,
                        principalTable: "StudentFaceTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaceEnrollmentEvents_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_BiometricConsents_StudentId_Active",
                table: "BiometricConsents",
                column: "StudentId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_FaceEnrollmentEvents_BiometricConsentId",
                table: "FaceEnrollmentEvents",
                column: "BiometricConsentId");

            migrationBuilder.CreateIndex(
                name: "IX_FaceEnrollmentEvents_FaceTemplateId",
                table: "FaceEnrollmentEvents",
                column: "FaceTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_FaceEnrollmentEvents_StudentId_OccurredAtUtc",
                table: "FaceEnrollmentEvents",
                columns: new[] { "StudentId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentFaceTemplates_BiometricConsentId",
                table: "StudentFaceTemplates",
                column: "BiometricConsentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFaceTemplates_Status",
                table: "StudentFaceTemplates",
                columns: new[] { "StudentId", "IsActive", "RequiresReEnrollment" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentFaceTemplates_TemplateFingerprint",
                table: "StudentFaceTemplates",
                column: "TemplateFingerprint");

            migrationBuilder.CreateIndex(
                name: "UX_StudentFaceTemplates_StudentId_Active",
                table: "StudentFaceTemplates",
                column: "StudentId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "UX_StudentFaceTemplates_StudentId_TemplateVersion",
                table: "StudentFaceTemplates",
                columns: new[] { "StudentId", "TemplateVersion" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaceEnrollmentEvents");

            migrationBuilder.DropTable(
                name: "StudentFaceTemplates");

            migrationBuilder.DropTable(
                name: "BiometricConsents");
        }
    }
}
