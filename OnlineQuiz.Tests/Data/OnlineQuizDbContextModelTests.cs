using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OnlineQuiz.Data;
using OnlineQuiz.Models;
using Xunit;

namespace OnlineQuiz.Tests.Data
{
    public class OnlineQuizDbContextModelTests
    {
        private static OnlineQuizDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OnlineQuizDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new OnlineQuizDbContext(options);
        }

        [Fact]
        public void UserRole_CompositeKey_IsConfigured()
        {
            using var db = CreateDbContext(nameof(UserRole_CompositeKey_IsConfigured));
            var entity = db.Model.FindEntityType(typeof(UserRoleModel));
            Assert.NotNull(entity);

            var pk = entity!.FindPrimaryKey();
            Assert.NotNull(pk);
            var names = pk!.Properties.Select(p => p.Name).ToArray();
            Assert.Contains("UserId", names);
            Assert.Contains("RoleId", names);
        }

        [Fact]
        public void ForeignKeys_DeleteBehavior_IsConfigured()
        {
            using var db = CreateDbContext(nameof(ForeignKeys_DeleteBehavior_IsConfigured));

            // Enrollment -> User (Restrict)
            var enrollmentEntity = db.Model.FindEntityType(typeof(EnrollmentModel));
            var enrollmentFk = enrollmentEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "UserId"));
            Assert.Equal(DeleteBehavior.Restrict, enrollmentFk.DeleteBehavior);

            // Attempt -> User (Restrict)
            var attemptEntity = db.Model.FindEntityType(typeof(AttemptModel));
            var attemptFk = attemptEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "UserId"));
            Assert.Equal(DeleteBehavior.Restrict, attemptFk.DeleteBehavior);

            // Notification -> User (Restrict)
            var notificationEntity = db.Model.FindEntityType(typeof(NotificationModel));
            var notificationFk = notificationEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "UserId"));
            Assert.Equal(DeleteBehavior.Restrict, notificationFk.DeleteBehavior);

            // Teacher -> User (Cascade)
            var teacherEntity = db.Model.FindEntityType(typeof(TeacherModel));
            var teacherFk = teacherEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "UserId"));
            Assert.Equal(DeleteBehavior.Cascade, teacherFk.DeleteBehavior);

            // Student -> User (Cascade)
            var studentEntity = db.Model.FindEntityType(typeof(StudentModel));
            var studentFk = studentEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "UserId"));
            Assert.Equal(DeleteBehavior.Cascade, studentFk.DeleteBehavior);

            // Course -> Teacher (Restrict)
            var courseEntity = db.Model.FindEntityType(typeof(CourseModel));
            var courseFk = courseEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "InstructorUserId"));
            Assert.Equal(DeleteBehavior.Restrict, courseFk.DeleteBehavior);

            // Enrollment -> Course (Restrict)
            var enrollmentCourseFk = enrollmentEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "CourseId"));
            Assert.Equal(DeleteBehavior.Restrict, enrollmentCourseFk.DeleteBehavior);

            // Quiz -> Course (Restrict)
            var quizEntity = db.Model.FindEntityType(typeof(QuizModel));
            var quizFk = quizEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "CourseId"));
            Assert.Equal(DeleteBehavior.Restrict, quizFk.DeleteBehavior);

            // Question -> Quiz (Cascade)
            var questionEntity = db.Model.FindEntityType(typeof(QuestionModel));
            var questionFk = questionEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "QuizId"));
            Assert.Equal(DeleteBehavior.Cascade, questionFk.DeleteBehavior);

            // Choice -> Question (Cascade)
            var choiceEntity = db.Model.FindEntityType(typeof(ChoiceModel));
            var choiceFk = choiceEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "QuestionId"));
            Assert.Equal(DeleteBehavior.Cascade, choiceFk.DeleteBehavior);

            // Attempt -> Quiz (Restrict)
            var attemptQuizFk = attemptEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "QuizId"));
            Assert.Equal(DeleteBehavior.Restrict, attemptQuizFk.DeleteBehavior);

            // AttemptAnswer -> Attempt (Cascade)
            var attemptAnswerEntity = db.Model.FindEntityType(typeof(AttemptAnswerModel));
            var aaAttemptFk = attemptAnswerEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "AttemptId"));
            Assert.Equal(DeleteBehavior.Cascade, aaAttemptFk.DeleteBehavior);

            // AttemptAnswer -> Question (Restrict)
            var aaQuestionFk = attemptAnswerEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "QuestionId"));
            Assert.Equal(DeleteBehavior.Restrict, aaQuestionFk.DeleteBehavior);

            // AttemptAnswer -> Choice (Restrict)
            var aaChoiceFk = attemptAnswerEntity!.GetForeignKeys().First(fk => fk.Properties.Any(p => p.Name == "ChoiceId"));
            Assert.Equal(DeleteBehavior.Restrict, aaChoiceFk.DeleteBehavior);
        }

        [Fact]
        public void UniqueIndexes_AreConfigured_OnKeyEntities()
        {
            using var db = CreateDbContext(nameof(UniqueIndexes_AreConfigured_OnKeyEntities));

            var userEntity = db.Model.FindEntityType(typeof(UserModel));
            var emailIndex = userEntity!.GetIndexes().First(i => i.Properties.Any(p => p.Name == "Email"));
            Assert.True(emailIndex.IsUnique);

            var roleEntity = db.Model.FindEntityType(typeof(RoleModel));
            var roleNameIndex = roleEntity!.GetIndexes().First(i => i.Properties.Any(p => p.Name == "Name"));
            Assert.True(roleNameIndex.IsUnique);

            var studentEntity = db.Model.FindEntityType(typeof(StudentModel));
            var studentNumberIndex = studentEntity!.GetIndexes().First(i => i.Properties.Any(p => p.Name == "StudentNumber"));
            Assert.True(studentNumberIndex.IsUnique);

            var courseEntity = db.Model.FindEntityType(typeof(CourseModel));
            var courseCodeIndex = courseEntity!.GetIndexes().First(i => i.Properties.Any(p => p.Name == "Code"));
            Assert.True(courseCodeIndex.IsUnique);

            var enrollmentEntity = db.Model.FindEntityType(typeof(EnrollmentModel));
            var compositeEnrollmentIndex = enrollmentEntity!.GetIndexes().First(i => i.Properties.Select(p => p.Name).OrderBy(n => n).SequenceEqual(new[] { "CourseId", "UserId" }));
            Assert.True(compositeEnrollmentIndex.IsUnique);
        }

        [Fact]
        public void DecimalPrecision_IsConfigured()
        {
            using var db = CreateDbContext(nameof(DecimalPrecision_IsConfigured));

            var questionEntity = db.Model.FindEntityType(typeof(QuestionModel));
            var pointsProp = questionEntity!.FindProperty("Points");
            Assert.Equal(5, pointsProp!.GetPrecision());
            Assert.Equal(2, pointsProp!.GetScale());

            var attemptEntity = db.Model.FindEntityType(typeof(AttemptModel));
            var scoreProp = attemptEntity!.FindProperty("Score");
            Assert.Equal(10, scoreProp!.GetPrecision());
            Assert.Equal(2, scoreProp!.GetScale());
        }

        [Fact]
        public void SeededRoles_ArePresent()
        {
            using var db = CreateDbContext(nameof(SeededRoles_ArePresent));
            db.Database.EnsureCreated();
            var roles = db.Roles.OrderBy(r => r.RoleId).ToList();
            Assert.Equal(3, roles.Count);
            Assert.Equal("Admin", roles[0].Name);
            Assert.Equal("Teacher", roles[1].Name);
            Assert.Equal("Student", roles[2].Name);
        }
    }
}