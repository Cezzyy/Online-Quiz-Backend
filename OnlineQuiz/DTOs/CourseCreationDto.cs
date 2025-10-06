using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Identity.Client;

namespace OnlineQuiz.DTOs
{
    public class CourseCreationDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int InstructorUserId { get; set; }
    }
}
