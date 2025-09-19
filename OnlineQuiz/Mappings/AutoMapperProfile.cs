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
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"));
            CreateMap<UpdateUserDto, UserModel>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // // Course mappings
            // CreateMap<CourseModel, CourseDto>()
            //     .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor.User.FullName))
            //     .ForMember(dest => dest.EnrollmentCount, opt => opt.MapFrom(src => src.Enrollments.Count))
            //     .ForMember(dest => dest.QuizCount, opt => opt.MapFrom(src => src.Quizzes.Count));
            // CreateMap<CreateCourseDto, CourseModel>()
            //     .ForMember(dest => dest.CourseId, opt => opt.Ignore());
            // CreateMap<UpdateCourseDto, CourseModel>()
            //     .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // // Enrollment mappings
            // CreateMap<EnrollmentModel, EnrollmentDto>()
            //     .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
            //     .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
            //     .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name));
            // CreateMap<CreateEnrollmentDto, EnrollmentModel>()
            //     .ForMember(dest => dest.EnrollmentId, opt => opt.Ignore())
            //     .ForMember(dest => dest.EnrolledAt, opt => opt.Ignore());

            // // Quiz mappings
            // CreateMap<QuizModel, QuizDto>()
            //     .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name));
            // CreateMap<CreateQuizDto, QuizModel>()
            //     .ForMember(dest => dest.QuizId, opt => opt.Ignore())
            //     .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            //     .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            // CreateMap<UpdateQuizDto, QuizModel>()
            //     .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // // Question mappings
            // CreateMap<QuestionModel, QuestionDto>();
            // CreateMap<CreateQuestionDto, QuestionModel>()
            //     .ForMember(dest => dest.QuestionId, opt => opt.Ignore());

            // // Choice mappings
            // CreateMap<ChoiceModel, ChoiceDto>();
            // CreateMap<CreateChoiceDto, ChoiceModel>()
            //     .ForMember(dest => dest.ChoiceId, opt => opt.Ignore())
            //     .ForMember(dest => dest.QuestionId, opt => opt.Ignore());

            // Role mappings
            CreateMap<RoleModel, string>().ConvertUsing(src => src.Name);
        }
    }
}