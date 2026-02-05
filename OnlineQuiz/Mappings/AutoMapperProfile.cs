using AutoMapper;
using OnlineQuiz.DTOs;
using OnlineQuiz.Models;

namespace OnlineQuiz.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<UserModel, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name)));
            CreateMap<CreateUserDto, UserModel>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"));
            CreateMap<UpdateUserDto, UserModel>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


            CreateMap<CourseModel, CourseDTO.CourseDto>()
                .ForMember(dest => dest.InstructorName,
                    opt => opt.MapFrom(src =>
                        src.Instructor != null && src.Instructor.User != null
                            ? src.Instructor.User.FullName
                            : "N/A"))
                .ForMember(dest => dest.CreatedByName,
                    opt => opt.MapFrom(src =>
                        src.Creator != null
                            ? src.Creator.FullName
                            : "N/A"));

            // Create DTO → Entity
            CreateMap<CourseDTO.CreateCourseDto, CourseModel>()
                .ForMember(dest => dest.CourseId, opt => opt.Ignore())
                .ForMember(dest => dest.Instructor, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Update DTO → Entity
            CreateMap<CourseDTO.UpdateCourseDto, CourseModel>()
                .ForMember(dest => dest.CourseId, opt => opt.Ignore())
                .ForMember(dest => dest.Instructor, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) =>
                        srcMember != null && !string.IsNullOrEmpty(srcMember?.ToString())));

            // Instructor mappings
            CreateMap<InstructorModel, TeacherDto>();
            CreateMap<CreateTeacherDto, InstructorModel>();
            
            // Student mappings
            CreateMap<StudentModel, StudentDto>();
            CreateMap<CreateStudentDto, StudentModel>();

            // Quiz mappings
            CreateMap<QuizModel, QuizDTO.QuizDto>()
                .ForMember(dest => dest.CourseName,
                    opt => opt.MapFrom(src =>
                        src.Course != null
                            ? src.Course.Name
                            : "N/A"))
                .ForMember(dest => dest.QuestionsCount,
                    opt => opt.MapFrom(src => src.Questions.Count))
                .ForMember(dest => dest.AttemptsCount,
                    opt => opt.MapFrom(src => src.Attempts.Count));

            // Create DTO → Entity
            CreateMap<QuizDTO.CreateQuizDto, QuizModel>()
                .ForMember(dest => dest.QuizId, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Questions, opt => opt.Ignore())
                .ForMember(dest => dest.Attempts, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Update DTO → Entity
            CreateMap<QuizDTO.UpdateQuizDto, QuizModel>()
                .ForMember(dest => dest.QuizId, opt => opt.Ignore())
                .ForMember(dest => dest.CourseId, opt => opt.Ignore())
                .ForMember(dest => dest.Course, opt => opt.Ignore())
                .ForMember(dest => dest.Questions, opt => opt.Ignore())
                .ForMember(dest => dest.Attempts, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) =>
                        srcMember != null && !string.IsNullOrEmpty(srcMember?.ToString())));

            // Question mappings
            CreateMap<QuestionModel, QuestionDTO.QuestionDto>()
                .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Body))
                .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.OrderIndex, opt => opt.MapFrom(src => src.SortOrder))
                .ForMember(dest => dest.Choices, opt => opt.MapFrom(src => src.Choices));

            CreateMap<QuestionDTO.CreateQuestionDto, QuestionModel>()
                .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
                .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.QuestionText))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.QuestionType))
                .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.OrderIndex))
                .ForMember(dest => dest.Quiz, opt => opt.Ignore())
                .ForMember(dest => dest.QuizId, opt => opt.Ignore())
                .ForMember(dest => dest.Choices, opt => opt.Ignore())
                .ForMember(dest => dest.AttemptAnswers, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<QuestionDTO.UpdateQuestionDto, QuestionModel>()
                .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
                .ForMember(dest => dest.QuizId, opt => opt.Ignore())
                .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.QuestionText))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.QuestionType))
                .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.OrderIndex))
                .ForMember(dest => dest.Quiz, opt => opt.Ignore())
                .ForMember(dest => dest.Choices, opt => opt.Ignore())
                .ForMember(dest => dest.AttemptAnswers, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) =>
                        srcMember != null && !string.IsNullOrEmpty(srcMember?.ToString())));

            // Choice mappings
            CreateMap<ChoiceModel, QuestionDTO.ChoiceDto>()
                .ForMember(dest => dest.ChoiceText, opt => opt.MapFrom(src => src.Body))
                .ForMember(dest => dest.OrderIndex, opt => opt.MapFrom(src => src.OrderIndex));

            CreateMap<QuestionDTO.CreateChoiceDto, ChoiceModel>()
                .ForMember(dest => dest.ChoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.ChoiceText))
                .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
                .ForMember(dest => dest.Question, opt => opt.Ignore())
                .ForMember(dest => dest.AttemptAnswers, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Role mappings
            CreateMap<RoleModel, string>().ConvertUsing(src => src.Name);
        }
    }
}