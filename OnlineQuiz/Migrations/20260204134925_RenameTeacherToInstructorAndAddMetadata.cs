using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineQuiz.Migrations
{
    /// <inheritdoc />
    public partial class RenameTeacherToInstructorAndAddMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Teachers_Instructor_UserId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Questions_QuizId",
                table: "Questions");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeletedBy",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPersonName",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Students",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EnrollmentStatus",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GuardianContactNumber",
                table: "Students",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuardianName",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Program",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Students",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "YearLevelString",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "RefreshTokens",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedAt",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RevokedBy",
                table: "RefreshTokens",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevokedReason",
                table: "RefreshTokens",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableFrom",
                table: "Quizzes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableUntil",
                table: "Quizzes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Quizzes",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Quizzes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                table: "Quizzes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PassingScore",
                table: "Quizzes",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "ShowCorrectAnswers",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowScoreImmediately",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShuffleChoices",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShuffleQuestions",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Questions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Explanation",
                table: "Questions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Questions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Questions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ActionText",
                table: "Notifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActionUrl",
                table: "Notifications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Notifications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ErrorLog",
                table: "ExportImportLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "ExportImportLogs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "ExportImportLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcessingTimeMs",
                table: "ExportImportLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecordsFailed",
                table: "ExportImportLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecordsProcessed",
                table: "ExportImportLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecordsSucceeded",
                table: "ExportImportLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ExportImportLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ExportImportLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DropReason",
                table: "Enrollments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DroppedAt",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalGrade",
                table: "Enrollments",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPassed",
                table: "Enrollments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LetterGrade",
                table: "Enrollments",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuizzesCompleted",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Enrollments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalQuizzes",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Enrollments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "AcademicYear",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Courses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Courses",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Courses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Semester",
                table: "Courses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Courses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Units",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Choices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Explanation",
                table: "Choices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "Choices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "FlaggedForReview",
                table: "Attempts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "GradedAt",
                table: "Attempts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GradedBy",
                table: "Attempts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructorFeedback",
                table: "Attempts",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPassed",
                table: "Attempts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Percentage",
                table: "Attempts",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "Attempts",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Attempts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TabSwitchCount",
                table: "Attempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Attempts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "AnsweredAt",
                table: "AttemptAnswers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "AttemptAnswers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSkipped",
                table: "AttemptAnswers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PointsEarned",
                table: "AttemptAnswers",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PointsPossible",
                table: "AttemptAnswers",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeSpentSeconds",
                table: "AttemptAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ErrorCode",
                table: "ActivityLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "ActivityLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HttpMethod",
                table: "ActivityLogs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestPath",
                table: "ActivityLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResponseTimeMs",
                table: "ActivityLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "ActivityLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "ActivityLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Instructors",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OfficeLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OfficePhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ConsultationHours = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instructors", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Instructors_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: (short)2,
                column: "Name",
                value: "Instructor");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsDeleted",
                table: "Users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastLoginAt",
                table: "Users",
                column: "LastLoginAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Status",
                table: "Users",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Students_EnrollmentStatus",
                table: "Students",
                column: "EnrollmentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Program",
                table: "Students",
                column: "Program");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Year_Level",
                table: "Students",
                column: "Year_Level");

            // Update existing Students records to have valid EnrollmentStatus before adding constraint
            migrationBuilder.Sql("UPDATE Students SET EnrollmentStatus = 'Active' WHERE EnrollmentStatus = '' OR EnrollmentStatus IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Students_EnrollmentStatus",
                table: "Students",
                sql: "EnrollmentStatus IN ('Active','OnLeave','Graduated','Withdrawn','Suspended')");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_RevokedAt",
                table: "RefreshTokens",
                column: "RevokedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_AvailableFrom_AvailableUntil",
                table: "Quizzes",
                columns: new[] { "AvailableFrom", "AvailableUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_Due_At",
                table: "Quizzes",
                column: "Due_At");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_Is_Published",
                table: "Quizzes",
                column: "Is_Published");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_IsDeleted",
                table: "Quizzes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuizId_Sort_Order",
                table: "Questions",
                columns: new[] { "QuizId", "Sort_Order" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Type",
                table: "Questions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ExpiresAt",
                table: "Notifications",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsArchived",
                table: "Notifications",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Priority",
                table: "Notifications",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_ExportImportLogs_Status",
                table: "ExportImportLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_EnrolledAt",
                table: "Enrollments",
                column: "EnrolledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_Status",
                table: "Enrollments",
                column: "Status");

            // Update existing Enrollments records to have valid Status before adding constraint
            migrationBuilder.Sql("UPDATE Enrollments SET Status = 'Active' WHERE Status = '' OR Status IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Enrollments_Status",
                table: "Enrollments",
                sql: "Status IN ('Active','Dropped','Completed','Withdrawn')");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_AcademicYear",
                table: "Courses",
                column: "AcademicYear");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_IsDeleted",
                table: "Courses",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_IsPublished",
                table: "Courses",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Semester",
                table: "Courses",
                column: "Semester");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_StartDate_EndDate",
                table: "Courses",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Status",
                table: "Courses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_FlaggedForReview",
                table: "Attempts",
                column: "FlaggedForReview");

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_Status",
                table: "Attempts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_SubmittedAt",
                table: "Attempts",
                column: "SubmittedAt");

            // Update existing Attempts records to have valid Status before adding constraint
            migrationBuilder.Sql("UPDATE Attempts SET Status = 'InProgress' WHERE Status = '' OR Status IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Attempts_Status",
                table: "Attempts",
                sql: "Status IN ('InProgress','Submitted','Abandoned','Graded')");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Action_Entity",
                table: "ActivityLogs",
                columns: new[] { "Action", "Entity" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_CreatedAt",
                table: "ActivityLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Severity",
                table: "ActivityLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Instructors_Department",
                table: "Instructors",
                column: "Department");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Instructors_Instructor_UserId",
                table: "Courses",
                column: "Instructor_UserId",
                principalTable: "Instructors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Instructors_Instructor_UserId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "Instructors");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsDeleted",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastLoginAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Status",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Students_EnrollmentStatus",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_Program",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_Year_Level",
                table: "Students");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Students_EnrollmentStatus",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_RevokedAt",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_AvailableFrom_AvailableUntil",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_Due_At",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_Is_Published",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_IsDeleted",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Questions_QuizId_Sort_Order",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_Type",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ExpiresAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_IsArchived",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Priority",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_ExportImportLogs_Status",
                table: "ExportImportLogs");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_EnrolledAt",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_Status",
                table: "Enrollments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Enrollments_Status",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Courses_AcademicYear",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_IsDeleted",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_IsPublished",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Semester",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_StartDate_EndDate",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Status",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Attempts_FlaggedForReview",
                table: "Attempts");

            migrationBuilder.DropIndex(
                name: "IX_Attempts_Status",
                table: "Attempts");

            migrationBuilder.DropIndex(
                name: "IX_Attempts_SubmittedAt",
                table: "Attempts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Attempts_Status",
                table: "Attempts");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_Action_Entity",
                table: "ActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_CreatedAt",
                table: "ActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_Severity",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPersonName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EnrollmentStatus",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "GuardianContactNumber",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "GuardianName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Program",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "YearLevelString",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "LastUsedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "RevokedBy",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "RevokedReason",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "AvailableFrom",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "AvailableUntil",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "PassingScore",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ShowCorrectAnswers",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ShowScoreImmediately",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ShuffleChoices",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ShuffleQuestions",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Explanation",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ActionText",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ActionUrl",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ErrorLog",
                table: "ExportImportLogs");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "ExportImportLogs");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "ExportImportLogs");

            migrationBuilder.DropColumn(
                name: "ProcessingTimeMs",
                table: "ExportImportLogs");

            migrationBuilder.DropColumn(
                name: "RecordsFailed",
                table: "ExportImportLogs");

            migrationBuilder.DropColumn(
                name: "RecordsProcessed",
                table: "ExportImportLogs");

            migrationBuilder.DropColumn(
                name: "RecordsSucceeded",
                table: "ExportImportLogs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ExportImportLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ExportImportLogs");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "DropReason",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "DroppedAt",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "FinalGrade",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "IsPassed",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "LetterGrade",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "QuizzesCompleted",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "TotalQuizzes",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Units",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Choices");

            migrationBuilder.DropColumn(
                name: "Explanation",
                table: "Choices");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "Choices");

            migrationBuilder.DropColumn(
                name: "FlaggedForReview",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "GradedAt",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "GradedBy",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "InstructorFeedback",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "IsPassed",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "TabSwitchCount",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "AnsweredAt",
                table: "AttemptAnswers");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "AttemptAnswers");

            migrationBuilder.DropColumn(
                name: "IsSkipped",
                table: "AttemptAnswers");

            migrationBuilder.DropColumn(
                name: "PointsEarned",
                table: "AttemptAnswers");

            migrationBuilder.DropColumn(
                name: "PointsPossible",
                table: "AttemptAnswers");

            migrationBuilder.DropColumn(
                name: "TimeSpentSeconds",
                table: "AttemptAnswers");

            migrationBuilder.DropColumn(
                name: "ErrorCode",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "HttpMethod",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "RequestPath",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "ResponseTimeMs",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "ActivityLogs");

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Teachers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: (short)2,
                column: "Name",
                value: "Teacher");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuizId",
                table: "Questions",
                column: "QuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Teachers_Instructor_UserId",
                table: "Courses",
                column: "Instructor_UserId",
                principalTable: "Teachers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
