using CenterManagement.Application.Interfaces;
using CenterManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CenterManagement.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IFileUploadService, FileUploadService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IInstructorService, InstructorService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IQrService, QrService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<ISystemSettingService, SystemSettingService>();
            // NotificationService is already registered — implementation replaced in-place for Phase 6
            return services;
        }
    }
}
