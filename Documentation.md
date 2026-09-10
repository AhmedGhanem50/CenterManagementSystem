# Center Management System

## STEP 0 — FULL INVENTORY

Below is the exhaustive file tree for the Center Management System repository, excluding compiled artifacts (`bin/`, `obj/`, `.git/`, and `.vs/`) but including all source code, configurations, documentation, and static web assets.

```text
D:\ITI\BACKEND\MVC\Project\CenterManagementSystem
|   .gitignore
|   Architecture.md
|   Business_Logic.md
|   CenterManagementSystem.slnx
|   commands.md
|   phase-1-auth-scaffolding.md
|   phase-2-student-management.md
|   phase-3-academic-core.md
|   phase-4-attendance-qr.md
|   phase-5-payments.md
|   phase-6-dashboards-analytics.md
|   tree.txt
|   walkthroughPhase1.md
|   
+---CenterManagement.Application
|   |   CenterManagement.Application.csproj
|   |   
|   +---DependencyInjection
|   |       ApplicationServiceRegistration.cs
|   |       
|   +---DTOs       
|   |   +---Analytics
|   |   |       AnalyticsKpiDto.cs
|   |   |       AttendanceTrendPointDto.cs
|   |   |       GradeDistributionDto.cs
|   |   |       SmartInsightDto.cs
|   |   |       SubjectRevenueDto.cs
|   |   |       TopTeacherDto.cs
|   |   |       
|   |   +---Attendance
|   |   |       AttendanceListItemDto.cs
|   |   |       AttendanceSessionSummaryDto.cs
|   |   |       ManualMarkDto.cs
|   |   |       ScanResultDto.cs
|   |   |       
|   |   +---Common
|   |   |       PagedResult.cs
|   |   |       StudentListFilter.cs
|   |   |       
|   |   +---Course
|   |   |       CourseDto.cs
|   |   |       CreateCourseDto.cs
|   |   |       UpdateCourseDto.cs
|   |   |       
|   |   +---Dashboard
|   |   |       ActiveSessionDto.cs
|   |   |       DashboardKpiDto.cs
|   |   |       
|   |   +---Group
|   |   |       CreateGroupDto.cs
|   |   |       GroupDetailDto.cs
|   |   |       GroupListItemDto.cs
|   |   |       UpdateGroupDto.cs
|   |   |       
|   |   +---Instructor
|   |   |       CreateInstructorDto.cs
|   |   |       InstructorListItemDto.cs
|   |   |       InstructorProfileDto.cs
|   |   |       UpdateInstructorDto.cs
|   |   |       
|   |   +---Notification
|   |   |       NotificationDto.cs
|   |   |       
|   |   +---Payment
|   |   |       CreateSessionPaymentDto.cs
|   |   |       OutstandingStudentDto.cs
|   |   |       PaymentKpiDto.cs
|   |   |       RecordPaymentDto.cs
|   |   |       SessionPaymentDto.cs
|   |   |       StudentFinancialSummaryDto.cs
|   |   |       TransactionFilter.cs
|   |   |       TransactionListItemDto.cs
|   |   |       UnbilledEnrollmentDto.cs
|   |   |       
|   |   +---Session
|   |   |       CreateSessionDto.cs
|   |   |       SessionDetailDto.cs
|   |   |       SessionListItemDto.cs
|   |   |       SessionScheduleDto.cs
|   |   |       
|   |   +---Student
|   |           CreateStudentDto.cs
|   |           EnrollmentDto.cs
|   |           SessionScheduleDto.cs
|   |           StudentAttendanceDto.cs
|   |           StudentCoursePaymentSummaryDto.cs
|   |           StudentListItemDto.cs
|   |           StudentProfileDto.cs
|   |           UpdateStudentDto.cs
|   |           
|   +---Interfaces
|   |       IAnalyticsService.cs
|   |       IAttendanceService.cs
|   |       IAuditLogService.cs
|   |       ICourseService.cs
|   |       IDashboardService.cs
|   |       IFileUploadService.cs
|   |       IGroupService.cs
|   |       IInstructorService.cs
|   |       INotificationService.cs
|   |       IPaymentService.cs
|   |       IQrService.cs
|   |       ISessionService.cs
|   |       IStudentService.cs
|   |       
|   +---Services
|           AnalyticsService.cs
|           AttendanceService.cs
|           AuditLogService.cs
|           CourseService.cs
|           DashboardService.cs
|           FileUploadService.cs
|           GroupService.cs
|           InstructorService.cs
|           NotificationService.cs
|           PaymentService.cs
|           QrService.cs
|           SessionService.cs
|           StudentService.cs
|           
+---CenterManagement.Domain
|   |   CenterManagement.Domain.csproj
|   |   CenterManagement.Domain.csproj.Backup.tmp
|   |   
|   +---Common
|   |       BaseEntity.cs
|   |       
|   +---Entities
|   |       ApplicationUser.cs
|   |       AuditLog.cs
|   |       Course.cs
|   |       Enrollment.cs
|   |       GradeLevel.cs
|   |       Group.cs
|   |       InstructorAttendance.cs
|   |       InstructorProfile.cs
|   |       Notification.cs
|   |       PaymentTransaction.cs
|   |       QrCodeLog.cs
|   |       Session.cs
|   |       SessionPayment.cs
|   |       StudentAttendance.cs
|   |       StudentCoursePayment.cs
|   |       StudentProfile.cs
|   |       Subject.cs
|   |       
+---CenterManagement.Infrastructure
|   |   CenterManagement.Infrastructure.csproj
|   |   
|   +---DependencyInjection
|   |       InfrastructureServiceRegistration.cs
|   |       
|   +---Migrations
|   |       20260526054039_InitialCreate.cs
|   |       20260526054039_InitialCreate.Designer.cs
|   |       20260603100717_Phase1_Auth.cs
|   |       20260603100717_Phase1_Auth.Designer.cs
|   |       20260604014619_Phase3_Academic.cs
|   |       20260604014619_Phase3_Academic.Designer.cs
|   |       20260605124954_Phase4_Attendance.cs
|   |       20260605124954_Phase4_Attendance.Designer.cs
|   |       20260605141733_Phase4_Attendance2.cs
|   |       20260605141733_Phase4_Attendance2.Designer.cs
|   |       20260605151920_Phase5_Payments.cs
|   |       20260605151920_Phase5_Payments.Designer.cs
|   |       20260605155914_Phase6_Analytics.cs
|   |       20260605155914_Phase6_Analytics.Designer.cs
|   |       20260606093936_prouction.cs
|   |       20260606093936_prouction.Designer.cs
|   |       CenterManagementDbContextModelSnapshot.cs
|   |       
|   +---Persistence
|   |       CenterManagementDbContext.cs
|   |       
|   +---Seed
|   |       IdentitySeeder.cs
|   |       
+---CenterManagement.Web
|   |   appsettings.Development.json
|   |   appsettings.json
|   |   CenterManagement.Web.csproj
|   |   CenterManagement.Web.csproj.user
|   |   Program.cs
|   |   
|   +---Controllers
|   |       AnalyticsController.cs
|   |       AttendanceController.cs
|   |       AuditLogController.cs
|   |       AuthController.cs
|   |       CourseController.cs
|   |       DashboardController.cs
|   |       GradeLevelController.cs
|   |       GroupController.cs
|   |       HomeController.cs
|   |       InstructorController.cs
|   |       NotificationController.cs
|   |       PaymentController.cs
|   |       SessionController.cs
|   |       StudentController.cs
|   |       SubjectController.cs
|   |       
|   +---Properties
|   |       launchSettings.json
|   |       
|   +---ViewModels
|   |   |   ErrorViewModel.cs
|   |   |   
|   |   +---Auth
|   |   |       LoginViewModel.cs
|   |   |       
|   |   +---Course
|   |   |       CreateCourseViewModel.cs
|   |   |       UpdateCourseViewModel.cs
|   |   |       
|   |   +---Group
|   |   |       CreateGroupViewModel.cs
|   |   |       UpdateGroupViewModel.cs
|   |   |       
|   |   +---Instructor
|   |   |       CreateInstructorViewModel.cs
|   |   |       UpdateInstructorViewModel.cs
|   |   |       
|   |   +---Payment
|   |   |       PaymentIndexViewModel.cs
|   |   |       
|   |   +---Session
|   |   |       CreateSessionViewModel.cs
|   |   |       SessionDetailViewModel.cs
|   |   |       
|   |   +---Student
|   |   |       CreateStudentViewModel.cs
|   |   |       StudentListViewModel.cs
|   |   |       TransferStudentViewModel.cs
|   |   |       UpdateStudentViewModel.cs
|   |   |       
|   +---Views
|   |   |   _ViewImports.cshtml
|   |   |   _ViewStart.cshtml
|   |   |   
|   |   +---Analytics
|   |   |       Index.cshtml
|   |   |       
|   |   +---AuditLog
|   |   |       Index.cshtml
|   |   |       
|   |   +---Auth
|   |   |       AccessDenied.cshtml
|   |   |       Login.cshtml
|   |   |       
|   |   +---Course
|   |   |       Create.cshtml
|   |   |       Delete.cshtml
|   |   |       Edit.cshtml
|   |   |       Index.cshtml
|   |   |       
|   |   +---Dashboard
|   |   |       Index.cshtml
|   |   |       
|   |   +---GradeLevel
|   |   |       Create.cshtml
|   |   |       Edit.cshtml
|   |   |       Index.cshtml
|   |   |       
|   |   +---Group
|   |   |       Create.cshtml
|   |   |       Edit.cshtml
|   |   |       Index.cshtml
|   |   |       
|   |   +---Home
|   |   |       Index.cshtml
|   |   |       Privacy.cshtml
|   |   |       
|   |   +---Instructor
|   |   |       Create.cshtml
|   |   |       Edit.cshtml
|   |   |       Index.cshtml
|   |   |       Profile.cshtml
|   |   |       
|   |   +---Payment
|   |   |       Index.cshtml
|   |   |       
|   |   +---Session
|   |   |       Create.cshtml
|   |   |       Detail.cshtml
|   |   |       Index.cshtml
|   |   |       
|   |   +---Shared
|   |   |       Error.cshtml
|   |   |       _Layout.cshtml
|   |   |       _Layout.cshtml.css
|   |   |       _LayoutEmpty.cshtml
|   |   |       _ValidationScriptsPartial.cshtml
|   |   |       
|   |   +---Student
|   |   |       Create.cshtml
|   |   |       Edit.cshtml
|   |   |       Index.cshtml
|   |   |       Profile.cshtml
|   |   |       Transfer.cshtml
|   |   |       
|   |   +---Subject
|   |   |       Create.cshtml
|   |   |       Edit.cshtml
|   |   |       Index.cshtml
|   |   |       
|   +---wwwroot
|   |   |   favicon.ico
|   |   |   
|   |   +---css
|   |   |       site.css
|   |   |       
|   |   +---images
|   |   |       avatar-default.png
|   |   |       
|   |   +---js
|   |   |       site.js
|   |   |       
|   |   +---lib
|   |   |   +---bootstrap
|   |   |   |   |   LICENSE
|   |   |   |   \---dist
|   |   |   |       +---css
|   |   |   |       |       bootstrap-grid.css ... (min/rtl/map variations)
|   |   |   |       |       bootstrap-reboot.css ... (min/rtl/map variations)
|   |   |   |       |       bootstrap-utilities.css ... (min/rtl/map variations)
|   |   |   |       |       bootstrap.css ... (min/rtl/map variations)
|   |   |   |       \---js
|   |   |   |               bootstrap.bundle.js ... (min/map variations)
|   |   |   |               bootstrap.esm.js ... (min/map variations)
|   |   |   |               bootstrap.js ... (min/map variations)
|   |   |   +---jquery
|   |   |   |   |   LICENSE.txt
|   |   |   |   \---dist
|   |   |   |           jquery.js ... (min/map variations)
|   |   |   +---jquery-validation
|   |   |   |   |   LICENSE.md
|   |   |   |   \---dist
|   |   |   |           additional-methods.js ... (min variations)
|   |   |   |           jquery.validate.js ... (min variations)
|   |   |   +---jquery-validation-unobtrusive
|   |   |           jquery.validate.unobtrusive.js ... (min variations)
|   |   |           LICENSE.txt
|   |   |           
|   |   +---uploads
|   |       |   .gitkeep
|   |       +---instructors
|   |       |       2e5fb53e-4d32-4534-835e-987ed69f8e63.png
|   |       |       5d0e30bb-b0ac-46c6-80ee-49c0ba0b078b.png
|   |       |       d033bd5d-5f3e-4144-b371-465c6e9aa3e4.png
|   |       +---students
|   |               8e624193-edb0-4ffd-8118-36a807131554.png
|   |               b34d7f91-f1a0-4723-b3e2-ac21b794306c.png
|   |               dc655e3b-43ca-4bc6-9280-12eafe7e29a7.png
|   |               f51a9944-cbb5-4e17-bb0a-f5f3e3992999.jpeg
|   |               
+---frontend
        add-newStudent.html
        DESIGN.md
        insights.html
        payments-financial.html
        receptioneestDash.html
        sessionDeatails.html
        studentmangment.html
        studentProfile.html
```

---

## STEP 1 — FILE-BY-FILE ANALYSIS

### PART 1: DOMAIN LAYER

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Domain\Common\BaseEntity.cs`**
- **Layer**: Domain
- **Responsibility**: Provides the universal blueprint for database entities, enforcing tracking capabilities (Id, timestamps) and ensuring a global soft-delete mechanism.
- **All classes inside file**: `BaseEntity`
- **All methods/functions inside each class**: (None. Contains only auto-properties).
- **Dependency injection usage**: None.
- **External dependencies used**: `System`
- **Business logic contained**: Defaults `CreatedAt` to `DateTime.UtcNow`. Defaults `IsDeleted` to `false`.
- **Side effects**: None independently.
- **Who calls this file**: Inherited by almost every other entity in the system (Course, Group, Enrollment, etc.). Queried by EF Core `OnModelCreating` to apply Global Query Filters dynamically.
- **What this file depends on**: Nothing. It is the core base type.

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Domain\Entities\ApplicationUser.cs`**
- **Layer**: Domain
- **Responsibility**: Extends the default Microsoft Identity User to include application-specific properties and navigation links to profiles and financial/audit logs.
- **All classes inside file**: `ApplicationUser`
- **All methods/functions inside each class**: (None. Contains only auto-properties).
- **Dependency injection usage**: None.
- **External dependencies used**: `Microsoft.AspNetCore.Identity`
- **Business logic contained**: Sets `IsActive` to `true` by default. Sets `CreatedAt` to `DateTime.UtcNow`. Initializes collection properties to empty lists to prevent NullReferenceExceptions during relationship mapping.
- **Side effects**: None independently.
- **Who calls this file**: `CenterManagementDbContext` (inherits `IdentityDbContext<ApplicationUser>`), Identity `UserManager<ApplicationUser>`, `StudentService`, `InstructorService`, Authentication Controllers.
- **What this file depends on**: `IdentityUser`, `StudentProfile`, `InstructorProfile`, `PaymentTransaction`, `SessionPayment`, `Notification`, `AuditLog`, `QrCodeLog`.

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Domain\Entities\Course.cs`**
- **Layer**: Domain
- **Responsibility**: Represents an academic course tied to a specific subject and grade level.
- **All classes inside file**: `Course`
- **All methods/functions inside each class**: (None. Contains only auto-properties).
- **Dependency injection usage**: None.
- **External dependencies used**: `CenterManagement.Domain.Common`
- **Business logic contained**: Domain relationships linking Subjects and Grade levels to Groups and Student Payments.
- **Side effects**: None independently.
- **Who calls this file**: `CourseService`, `StudentService`, `CenterManagementDbContext`.
- **What this file depends on**: `BaseEntity`, `Subject`, `GradeLevel`, `Group`, `StudentCoursePayment`.

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Domain\Entities\Enrollment.cs`**
- **Layer**: Domain
- **Responsibility**: Acts as a join table/domain model representing a student's active or inactive participation within a specific Group.
- **All classes inside file**: `Enrollment`
- **All methods/functions inside each class**: (None. Contains only auto-properties).
- **Dependency injection usage**: None.
- **External dependencies used**: `CenterManagement.Domain.Common`
- **Business logic contained**: Defaults `EnrollmentDate` to `DateTime.UtcNow`. Defaults `IsActive` to `true`.
- **Side effects**: None independently.
- **Who calls this file**: `StudentService` (creates/deactivates these during transfers/additions), `CenterManagementDbContext`.
- **What this file depends on**: `BaseEntity`, `StudentProfile`, `Group`.

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Domain\Entities\Group.cs`**
- **Layer**: Domain
- **Responsibility**: Defines a specific cohort of students taking a `Course` under a specific `InstructorProfile`.
- **All classes inside file**: `Group`
- **All methods/functions inside each class**: (None. Contains only auto-properties).
- **Dependency injection usage**: None.
- **External dependencies used**: `CenterManagement.Domain.Common`
- **Business logic contained**: Defaults `IsActive` to `true`. Initializes `Sessions` and `Enrollments` lists to prevent NullReferenceExceptions.
- **Side effects**: None independently.
- **Who calls this file**: `GroupService`, `SessionService`, `CenterManagementDbContext`.
- **What this file depends on**: `BaseEntity`, `Course`, `InstructorProfile`, `Session`, `Enrollment`.

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Domain\Entities\GradeLevel.cs`**
- **Layer**: Domain
- **Responsibility**: Defines academic grades (e.g., "1st Secondary", "2nd Secondary") to categorize students and courses.
- **All classes inside file**: `GradeLevel` (Inferred from DbContext)
- **All methods/functions inside each class**: (None. Auto-properties).
- **Dependency injection usage**: None.
- **External dependencies used**: `CenterManagement.Domain.Common`
- **Business logic contained**: None (Pure lookup table mapping).
- **Side effects**: None.
- **Who calls this file**: `CenterManagementDbContext`, `StudentProfile`, `Course`.
- **What this file depends on**: `BaseEntity`.

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Domain\Entities\Subject.cs`**
- **Layer**: Domain
- **Responsibility**: Defines the actual academic subject (e.g., "Physics", "Math") that instructors teach and courses cover.
- **All classes inside file**: `Subject` (Inferred from DbContext)
- **All methods/functions inside each class**: (None. Auto-properties).
- **Dependency injection usage**: None.
- **External dependencies used**: `CenterManagement.Domain.Common`
- **Business logic contained**: None.
- **Side effects**: None.
- **Who calls this file**: `CenterManagementDbContext`, `InstructorProfile`, `Course`.
- **What this file depends on**: `BaseEntity`.

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Domain\Entities\StudentProfile.cs`**
- **Layer**: Domain
- **Responsibility**: Holds the student-specific domain data tied 1:1 to an `ApplicationUser`.
- **All classes inside file**: `StudentProfile` (Inferred from `StudentService` & `DbContext`)
- **All methods/functions inside each class**: (None. Auto-properties).
- **Dependency injection usage**: None.
- **External dependencies used**: `CenterManagement.Domain.Common`
- **Business logic contained**: Navigation rules connecting the user to grade levels, enrollments, attendance, and payments.
- **Side effects**: None.
- **Who calls this file**: `StudentService`, `CenterManagementDbContext`, `PaymentService`.
- **What this file depends on**: `BaseEntity`, `ApplicationUser`, `GradeLevel`, `Enrollment`, `StudentAttendance`, `StudentCoursePayment`, `SessionPayment`.

*(Note: The remaining Domain Entities—`InstructorProfile`, `InstructorAttendance`, `StudentAttendance`, `Session`, `StudentCoursePayment`, `SessionPayment`, `PaymentTransaction`, `AuditLog`, `QrCodeLog`, `Notification`—all strictly adhere to this exact same pure, anemic domain model pattern inheriting from `BaseEntity` without injected services or behavioral methods. They are mapped in EF Core).*

### PART 2: INFRASTRUCTURE LAYER

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Infrastructure\DependencyInjection\InfrastructureServiceRegistration.cs`**
- **Layer**: Infrastructure
- **Responsibility**: Encapsulates the registration of all infrastructure-related dependencies into the ASP.NET Core `IServiceCollection`. This keeps the `Program.cs` in the Web layer clean.
- **All classes inside file**: `InfrastructureServiceRegistration` (static)
- **All methods/functions inside each class**: 
  - `AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)`
- **Dependency injection usage**: Acts as the injector itself. It registers the `CenterManagementDbContext` with a scoped lifetime using the `DefaultConnection` string from `IConfiguration`.
- **External dependencies used**: `Microsoft.EntityFrameworkCore`, `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.DependencyInjection`.
- **Business logic contained**: None. Pure setup logic.
- **Side effects**: None directly, but establishes the DI pipeline capable of database I/O.
- **Who calls this file**: `Program.cs` in `CenterManagement.Web`.
- **What this file depends on**: `CenterManagementDbContext`.

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Infrastructure\Persistence\CenterManagementDbContext.cs`**
- **Layer**: Infrastructure
- **Responsibility**: The core database context for the entire application. It acts as the Unit of Work, defining DB Sets, relationship constraints, decimal precision, unique indexes, and overriding the global soft-delete mechanism.
- **All classes inside file**: `CenterManagementDbContext`
- **All methods/functions inside each class**:
  - `CenterManagementDbContext(DbContextOptions options)`: Constructor.
  - `OnModelCreating(ModelBuilder builder)`: Configures the Fluent API for database constraints and foreign keys.
  - `ApplySoftDeleteFilter(ModelBuilder builder)`: Uses reflection to apply the `.IsDeleted == false` query filter globally.
  - `SaveChanges()` & `SaveChangesAsync(CancellationToken)`: Overridden to intercept saving and automatically populate the `UpdatedAt` timestamp.
  - `UpdateTimestamps()`: Helper method to update `UpdatedAt`.
- **Dependency injection usage**: Expects `DbContextOptions` via constructor injection (provided by `InfrastructureServiceRegistration`).
- **External dependencies used**: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore`.
- **Business logic contained**: Enforces referential integrity by overriding the default cascade delete (setting `DeleteBehavior.Restrict`). It also strictly mandates precision for all financial entities (`18, 2`).
- **Side effects**: Directly interacts with the SQL Database to perform CRUD and schema definition.
- **Who calls this file**: Every service in the Application layer (`StudentService`, `PaymentService`, etc.) as well as Identity seeding and Migration scripts.
- **What this file depends on**: Practically every Domain entity (`ApplicationUser`, `StudentProfile`, `Course`, `Session`, etc.) and `BaseEntity`.

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Infrastructure\Seed\IdentitySeeder.cs`**
- **Layer**: Infrastructure
- **Responsibility**: Populates the database with required default data on application startup if it doesn't already exist.
- **All classes inside file**: `IdentitySeeder` (static)
- **All methods/functions inside each class**:
  - `SeedRolesAndAdminAsync(UserManager, RoleManager, DbContext)`
- **Dependency injection usage**: Expects `UserManager`, `RoleManager`, and `CenterManagementDbContext` to be passed as arguments (resolved manually via service scope in `Program.cs`).
- **External dependencies used**: `Microsoft.AspNetCore.Identity`, `Microsoft.EntityFrameworkCore`.
- **Business logic contained**: 
  - Seeds the 3 core Identity Roles (`Admin`, `Instructor`, `Student`).
  - Creates the super-user admin account (`admin@center.com` / `Admin@123`) and assigns the `Admin` role.
  - Seeds default lookup data: 5 Grade Levels and 5 Subjects.
- **Side effects**: Commits transactions to the `AspNetRoles`, `AspNetUsers`, `GradeLevels`, and `Subjects` tables.
- **Who calls this file**: `Program.cs` in `CenterManagement.Web`.
- **What this file depends on**: `CenterManagementDbContext`, `ApplicationUser`.

**FILES: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Infrastructure\Migrations\*.cs`**
- **Layer**: Infrastructure
- **Responsibility**: Stores the Entity Framework Core migration history tracking incremental schema changes across the 6 development phases.
- **All classes inside file**: Auto-generated Migration classes (e.g., `InitialCreate`, `Phase1_Auth`, `Phase5_Payments`) and `CenterManagementDbContextModelSnapshot`.
- **All methods/functions inside each class**: `Up()`, `Down()`, `BuildModel()`.
- **Dependency injection usage**: None.
- **External dependencies used**: `Microsoft.EntityFrameworkCore.Migrations`.
- **Business logic contained**: None. Contains only raw schema translations (Table creations, index creation, foreign key bindings).
- **Side effects**: Modifies SQL Server schema when `dotnet ef database update` is executed.
- **Who calls this file**: The EF Core tooling.
- **What this file depends on**: Nothing explicitly; derived from `CenterManagementDbContext`.

### PART 3: APPLICATION LAYER - CORE SERVICES

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Application\DependencyInjection\ApplicationServiceRegistration.cs`**
- **Layer**: Application
- **Responsibility**: Centralizes the registration of all Application-layer services into the DI container to keep the Web layer decoupled from concrete implementations.
- **All classes inside file**: `ApplicationServiceRegistration` (static)
- **All methods/functions inside each class**: `AddApplicationServices(this IServiceCollection services)`
- **Dependency injection usage**: Binds all `IService` interfaces to their concrete classes using `AddScoped<>`.
- **External dependencies used**: `Microsoft.Extensions.DependencyInjection`.
- **Business logic contained**: None. Pure configuration.
- **Side effects**: Dictates the object lifetime (Scoped) for all business services per HTTP request.
- **Who calls this file**: `Program.cs` in `CenterManagement.Web`.
- **What this file depends on**: All interfaces and concrete services in the Application layer (e.g., `IAuditLogService`, `AuditLogService`).

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Application\Services\AuditLogService.cs`**
- **Layer**: Application
- **Responsibility**: Provides a centralized, asynchronous way to record system actions (creations, updates, deletions) into the database for security and tracking.
- **All classes inside file**: `AuditLogService`
- **All methods/functions inside each class**:
  - `LogAsync(userId, action, entityName, entityId, oldValues, newValues)`
- **Dependency injection usage**: Injects `CenterManagementDbContext`.
- **External dependencies used**: `CenterManagement.Domain.Entities`, `CenterManagement.Infrastructure.Persistence`.
- **Business logic contained**: Automatically attaches `ActionDate` and `CreatedAt` to `DateTime.UtcNow`. Swallows exceptions (`catch (Exception ex)`) to ensure that a failure in the audit logging system does *not* roll back or crash the primary business transaction.
- **Side effects**: Writes records to the `AuditLogs` SQL table. Logs errors to `Console.WriteLine` if the DB write fails.
- **Who calls this file**: `StudentService`, `PaymentService`, `CourseService`, `InstructorService`, etc.
- **What this file depends on**: `CenterManagementDbContext`.

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Application\Services\FileUploadService.cs`**
- **Layer**: Application
- **Responsibility**: Manages the physical storage of uploaded files (like profile photos) on the server.
- **All classes inside file**: `FileUploadService`
- **All methods/functions inside each class**:
  - `UploadPhotoAsync(IFormFile file, string subfolder)`
  - `DeleteFile(string relativePath)`
- **Dependency injection usage**: Injects `IWebHostEnvironment` to resolve physical paths.
- **External dependencies used**: `Microsoft.AspNetCore.Hosting`, `Microsoft.AspNetCore.Http`, `System.IO`.
- **Business logic contained**: 
  - **Security Rule**: Strictly filters allowed extensions (`.jpg`, `.jpeg`, `.png`, `.webp`). Throws `InvalidOperationException` if violated.
  - **Resource Rule**: Limits file size to 5 MB.
  - **Uniqueness**: Generates a random `Guid` for every uploaded filename to prevent collisions.
  - **Graceful Degradation**: Catches `IOException` and `UnauthorizedAccessException` during deletion so locked files don't crash updates.
- **Side effects**: Performs File System I/O (creates directories, writes streams, deletes files).
- **Who calls this file**: `StudentService`, `InstructorService`.
- **What this file depends on**: `IWebHostEnvironment`.

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Application\Services\StudentService.cs`**
- **Layer**: Application
- **Responsibility**: Orchestrates the entire lifecycle of a student, including identity creation, profile management, group enrollments, and soft deletion.
- **All classes inside file**: `StudentService`
- **All methods/functions inside each class**:
  - `CreateStudentAsync`
  - `GetStudentListAsync`
  - `GetStudentProfileAsync`
  - `UpdateStudentAsync`
  - `SoftDeleteAsync`
  - `ToggleActiveAsync`
  - `TransferStudentAsync`
  - `AddToGroupAsync`
  - `RemoveFromGroupAsync`
  - `SearchStudentsAsync`
  - `MapToListItemDto` (private helper)
- **Dependency injection usage**: Injects `CenterManagementDbContext`, `UserManager<ApplicationUser>`, `IAuditLogService`, `IFileUploadService`.
- **External dependencies used**: `Microsoft.AspNetCore.Identity`, `Microsoft.EntityFrameworkCore`, `System.Text.Json`.
- **Business logic contained**:
  - **Transactional Boundaries**: Uses `await _db.Database.BeginTransactionAsync()` during creation to ensure Identity User, Student Profile, and initial Enrollments all succeed or all fail together.
  - **Rollback Compensation**: If the transaction fails, it manually searches for and deletes the orphaned Identity User because `UserManager` has its own internal commit scope.
  - **Enrollment Rules**: `TransferStudentAsync` verifies active enrollment in the origin group and prevents duplicate enrollments in the target group before executing a transfer.
  - **Pagination & Filtering**: Calculates dynamic payment statuses (Paid, Partial, Unpaid) in memory *after* pagination.
- **Side effects**: Mutates database state across multiple tables. Triggers file system uploads. Triggers audit logs.
- **Who calls this file**: `StudentController`.
- **What this file depends on**: EF Core DbContext, ASP.NET Identity, FileUpload, and Audit layers.

*(Note: The Application layer also contains the matching Interfaces for these services (`IAuditLogService.cs`, `IStudentService.cs`, etc.), which strictly contain method signatures without business logic, acting as DI contracts. There are also over 40 Data Transfer Objects (DTOs) organized by feature (e.g., `CreateStudentDto.cs`), which are pure POCO classes used to move data between the Web and Application layers without exposing Domain Entities.)*

### PART 4: WEB LAYER - ENTRY & CONTROLLERS

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Web\Program.cs`**
- **Layer**: Web / Presentation (Entry Point)
- **Responsibility**: Bootstraps the application, registers layers into the Dependency Injection container, configures the HTTP pipeline (Middleware), and applies authentication/authorization rules.
- **All classes inside file**: (Top-level statements).
- **All methods/functions inside each class**: (Main entry point execution).
- **Dependency injection usage**: Consumes `IServiceCollection` to inject `ControllersWithViews`, `InfrastructureServices`, `ApplicationServices`, `Identity`, and configures cookies/policies.
- **External dependencies used**: `Microsoft.AspNetCore.Builder`, `Microsoft.AspNetCore.Identity`, `CenterManagement.Domain.Entities`, `CenterManagement.Infrastructure.Persistence`, `CenterManagement.Infrastructure.Seed`.
- **Business logic contained**:
  - Sets sliding cookie expiration to 8 hours.
  - Registers two core Authorization Policies (`AdminOnly`, `AdminOrInstructor`).
  - Automatically invokes the `IdentitySeeder` upon startup by creating a service scope.
- **Side effects**: Launches the web server. Connects to the database to seed initial data.
- **Who calls this file**: The .NET Core Host / Kestrel.
- **What this file depends on**: Infrastructure Layer configurations and Application Layer Service Registrations.

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Web\Controllers\StudentController.cs`**
- **Layer**: Web / Presentation
- **Responsibility**: Orchestrates HTTP requests regarding student management, formatting output for Razor views, and validating inputs before invoking the Application Layer.
- **All classes inside file**: `StudentController`, `AddRemoveGroupRequest`
- **All methods/functions inside each class**:
  - `Index(StudentListFilter filter)`
  - `Create()`, `CreatePost(CreateStudentViewModel)`
  - `Profile(int id)`
  - `Edit(int id)`, `EditPost(int id, UpdateStudentViewModel)`
  - `Delete(int id)`
  - `ToggleActive(int id)`
  - `Transfer(int id)`, `TransferPost(TransferStudentViewModel)`
  - `AddToGroup(AddRemoveGroupRequest)`
  - `RemoveFromGroup(AddRemoveGroupRequest)`
  - `Search(string q)`
  - Helpers: `GetGradeLevelSelectList`, `GetGroupSelectList`, `GetCourseSelectList`
- **Dependency injection usage**: Injects `IStudentService` and `CenterManagementDbContext`. *(Note: Injecting DbContext into the Controller directly to populate Dropdown SelectLists is a pragmatic shortcut bypassing the Application layer for read-only lookups).*
- **External dependencies used**: `Microsoft.AspNetCore.Mvc`, `Microsoft.AspNetCore.Authorization`.
- **Business logic contained**: None. Strictly handles UI binding logic. Traps `InvalidOperationException` and maps them to `ModelState.AddModelError` to display business logic failures (like duplicate enrollments) beautifully to the user.
- **Side effects**: Returns HTML views. Sets `TempData` for success/error alerts.
- **Who calls this file**: HTTP Router from User Browser.
- **What this file depends on**: `IStudentService`, `CenterManagementDbContext`, ViewModels.

**FILE: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Web\Controllers\AuthController.cs`**
- **Layer**: Web / Presentation
- **Responsibility**: Handles user login, logout, and access denial routing using Microsoft Identity.
- **All classes inside file**: `AuthController`
- **All methods/functions inside each class**:
  - `Login()`, `Login(LoginViewModel)`
  - `Logout()`
  - `AccessDenied()`
- **Dependency injection usage**: Injects `UserManager<ApplicationUser>`, `SignInManager<ApplicationUser>`, and `IAuditLogService`.
- **External dependencies used**: `Microsoft.AspNetCore.Identity`.
- **Business logic contained**:
  - Validates `!user.IsActive` and blocks login if the account is deactivated.
  - Triggers an `AuditLog` explicitly upon a successful login.
  - Role-based redirection (Admins go to Dashboard, Instructors go to Instructor profile view).
- **Side effects**: Issues authentication cookies to the browser. Records to the Audit Log table.
- **Who calls this file**: HTTP Router.
- **What this file depends on**: `SignInManager`, `UserManager`, `IAuditLogService`, `LoginViewModel`.

*(Note: The Web layer continues with `CourseController`, `PaymentController`, `GroupController`, `SessionController`, `AnalyticsController`, `AttendanceController`, etc., which all follow the exact same architectural pattern: validating ModelState, trapping `InvalidOperationException` thrown by the Application Services, and rendering Razor ViewModels.)*

---

## STEP 2 — DEPENDENCY GRAPH

Below is the dependency mapping based on the exhaustive file analysis. It illustrates the exact direction of dependencies and highlights structural couplings.

#### Layer Dependency Graph
1. **`CenterManagement.Web`**
   - Depends on: `CenterManagement.Application` (Interfaces and DTOs)
   - Depends on: `CenterManagement.Infrastructure` (DbContext and DI extensions)
2. **`CenterManagement.Infrastructure`**
   - Depends on: `CenterManagement.Application` (Interfaces, e.g., to implement Infrastructure services if any, though currently strictly provides data access).
   - Depends on: `CenterManagement.Domain` (Entities for `CenterManagementDbContext`).
3. **`CenterManagement.Application`**
   - Depends on: `CenterManagement.Domain` (Entities and `BaseEntity`).
   - *Architecture Violation Notice*: Also depends on `CenterManagement.Infrastructure.Persistence` (Injecting `CenterManagementDbContext` directly).
4. **`CenterManagement.Domain`**
   - Depends on: Nothing. (Pure).

#### File-Level Dependency Map (Critical Pathways)

- **The Database Nexus**: 
  - `CenterManagementDbContext.cs` is the most tightly coupled file in the system. 
  - **Depended on by**: `StudentService.cs`, `PaymentService.cs`, `CourseService.cs`, `AttendanceService.cs`, `StudentController.cs`, `GroupController.cs`, `IdentitySeeder.cs`, `InfrastructureServiceRegistration.cs`.
- **The Identity Nexus**:
  - `ApplicationUser.cs` is the anchor for all profiles.
  - **Depended on by**: `StudentProfile.cs`, `InstructorProfile.cs`, `CenterManagementDbContext.cs`, `AuthController.cs`, `StudentService.cs`.
- **The Transactional Flow (Example: `StudentService.cs`)**:
  - `StudentController.cs` -> injects -> `IStudentService.cs`
  - `StudentService.cs` -> injects -> `CenterManagementDbContext.cs`
  - `StudentService.cs` -> injects -> `UserManager<ApplicationUser>`
  - `StudentService.cs` -> injects -> `IAuditLogService.cs`

#### Tight Coupling & Architectural Risks Identified
1. **Tight Coupling: Application Layer -> EF Core**:
   Every service inside `CenterManagement.Application\Services\` (e.g., `StudentService.cs`, `PaymentService.cs`) directly imports `CenterManagement.Infrastructure.Persistence` and injects `CenterManagementDbContext`. This permanently couples the business logic to Entity Framework Core SQL Server, meaning you cannot easily switch databases or mock the repository layer without using an EF Core In-Memory database.
2. **Tight Coupling: Web Layer -> EF Core**:
   Several controllers in `CenterManagement.Web\Controllers\` (e.g., `StudentController.cs`) inject `CenterManagementDbContext` directly to populate ViewModels (e.g., `GetGradeLevelSelectList()`). This allows the Web layer to bypass the Application layer, creating an Infrastructure leak into the Presentation layer.

#### Circular Dependencies
- **None detected.** The ASP.NET Core DI container and project references correctly flow top-down. The `.csproj` files enforce this: Web references Application and Infrastructure. Application references Domain. Infrastructure references Domain.

---

## STEP 3 — BUSINESS LOGIC MAPPING

Business logic defines *how* data can be mutated and the rules surrounding those mutations. Below is the extraction of business rules and exactly where they live.

#### 1. Student Enrollment Validation Rule
- **The Rule**: A student cannot have multiple active enrollments in the same group. When transferring, the old enrollment must be deactivated safely.
- **Where it lives**: `CenterManagement.Application\Services\StudentService.cs` (Methods: `TransferStudentAsync`, `AddToGroupAsync`).
- **Placement Validation**: Correct. This is pure business logic appropriately enclosed in the Application Layer. It throws an `InvalidOperationException` if violated.

#### 2. Soft Deletion Rule
- **The Rule**: Deleted entities must never be returned in queries, but their historical data must remain for financial/audit integrity.
- **Where it lives**: `CenterManagement.Infrastructure\Persistence\CenterManagementDbContext.cs` (Method: `ApplySoftDeleteFilter(ModelBuilder builder)`).
- **Placement Validation**: Correct, but pragmatic. Instead of forcing every Application service to remember `.Where(x => !x.IsDeleted)`, the Infrastructure layer uses EF Core Global Query Filters to abstract this rule away from the business layer entirely.

#### 3. Financial Precision & Referential Integrity Rules
- **The Rule**: Financial transactions must not lose fractional cents, and academic/financial records cannot be cascade-deleted.
- **Where it lives**: `CenterManagement.Infrastructure\Persistence\CenterManagementDbContext.cs` (Method: `OnModelCreating`). Uses `HasPrecision(18,2)` and `DeleteBehavior.Restrict`.
- **Placement Validation**: Correct. This is a database-level schema enforcement rule properly living in the Infrastructure layer.

#### 4. File Upload Constraints
- **The Rule**: Uploaded photos must only be `jpg/jpeg/png/webp` and must be under 5MB.
- **Where it lives**: `CenterManagement.Application\Services\FileUploadService.cs` (Method: `UploadPhotoAsync`).
- **Placement Validation**: Correct. It is abstracted behind `IFileUploadService`.

#### 5. Authentication Authorization Rules
- **The Rule**: Only active users can log in. Users are redirected based on role.
- **Where it lives**: `CenterManagement.Web\Controllers\AuthController.cs` (Method: `Login(LoginViewModel model)`).
- **Placement Validation**: **WARNING - Domain Logic Leakage**. The rule `if (!user.IsActive)` causing a login block is currently executed in the Controller. Ideally, this should be handled by a custom `IAuthenticationService` in the Application layer, keeping the controller entirely ignorant of what constitutes a "valid" login beyond the HTTP response.

#### 6. UI Dropdown Lookups (Domain Leakage)
- **The Rule**: Filtering active vs inactive courses for dropdown selection.
- **Where it lives**: `CenterManagement.Web\Controllers\StudentController.cs` (Method: `GetCourseSelectList()`).
- **Placement Validation**: **WARNING - Infrastructure Leakage**. The Web layer queries the database directly (`_db.Courses.Where(c => !c.IsDeleted)`) instead of asking `ICourseService` for a list of active courses. This leaks data-access logic into the UI Controller.

---

## STEP 4 — CLEAN ARCHITECTURE VALIDATION

A forensic evaluation of how strictly this repository adheres to Clean Architecture constraints (Domain -> Application -> Infrastructure -> Web). 

#### 1. Domain Purity Violations
- **Status**: **PASS (Perfect compliance)**
- **Evidence**: Looking at `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Domain\Entities\StudentProfile.cs` and `ApplicationUser.cs`, there are absolutely zero using statements pointing to `Microsoft.EntityFrameworkCore` or `CenterManagement.Application`. The domain models are "anemic" (pure data containers with relationships), which prevents infrastructure logic from contaminating enterprise rules.

#### 2. Application Layer Violations
- **Status**: **FAIL (Pragmatic Trade-off)**
- **Evidence**: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Application\Services\StudentService.cs` contains `using CenterManagement.Infrastructure.Persistence;` and directly injects `CenterManagementDbContext`. 
- **Why this fails Clean Architecture**: The Application Layer is supposed to be completely ignorant of the database technology. It should inject an `IStudentRepository` or `IUnitOfWork` defined in the Application layer, which the Infrastructure layer implements. By directly injecting the EF Core DbContext, the Application Layer is permanently glued to Entity Framework Core.
- **Verdict**: This is a known, widespread industry shortcut (treating DbContext as the Unit of Work). It works flawlessly in production but breaks theoretical architectural purity.

#### 3. Infrastructure Leaks into Web/Presentation
- **Status**: **FAIL**
- **Evidence**: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Web\Controllers\StudentController.cs` directly injects `CenterManagementDbContext _db` alongside `IStudentService`. 
- **Why this fails Clean Architecture**: The Controller queries `await _db.GradeLevels.AsNoTracking().ToListAsync()` to build Dropdown Menus. The Presentation layer is bypassing the Application Use Cases and talking directly to the database. If you change database providers, you have to rewrite your UI Controllers.

#### 4. Controller Fatness Issues
- **Status**: **PASS (Mostly Thin)**
- **Evidence**: The POST actions in `StudentController.cs` (e.g., `CreatePost` and `TransferPost`) do not contain database mutation logic. They simply map the ViewModel to a DTO, wrap the call to `await _studentService.CreateStudentAsync(dto)` in a `try/catch` block looking for `InvalidOperationException`, and return redirects. The Controllers properly delegate heavy lifting to the Application layer.

---

## STEP 5 — SOLID ANALYSIS (WITH REAL REFERENCES)

An evaluation of the five SOLID principles based on exact code paths.

#### 1. SRP: Single Responsibility Principle
- **Compliance Level**: **High**
- **Evidence of Compliance**: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Application\Services\FileUploadService.cs`. This class has exactly one reason to change: if the physical mechanism for saving images to the disk changes. It does not validate student data, it does not update the database, it strictly handles File I/O.
- **Evidence of Violation**: `CenterManagementDbContext` is slightly overloaded, acting as both a database configuration mapper (`OnModelCreating`) and an audit-trigger (`UpdateTimestamps()` overridden in `SaveChanges`).

#### 2. OCP: Open/Closed Principle
- **Compliance Level**: **Medium**
- **Evidence of Compliance**: The notification and logging systems are abstracted via `INotificationService` and `IAuditLogService`. If the center decides to switch from Database Audit Logs to Azure App Insights, they simply create a new `AzureAuditLogService : IAuditLogService`, swap it in `ApplicationServiceRegistration.cs`, and `StudentService` remains completely unchanged (closed for modification, open for extension).

#### 3. LSP: Liskov Substitution Principle
- **Compliance Level**: **High**
- **Evidence of Compliance**: `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Domain\Entities\ApplicationUser.cs` extends `IdentityUser`. Because Microsoft's `UserManager<T>` is built on generics, the application successfully substitutes `ApplicationUser` wherever the underlying framework expects a standard user, without breaking authentication behaviors.

#### 4. ISP: Interface Segregation Principle
- **Compliance Level**: **High**
- **Evidence of Compliance**: The Application Layer separates contracts aggressively. Instead of one massive `ICenterManagementService`, we have `D:\ITI\BACKEND\MVC\Project\CenterManagementSystem\CenterManagement.Application\Interfaces\IStudentService.cs`, `ICourseService.cs`, and `IPaymentService.cs`. The `StudentController` only injects `IStudentService`, ensuring it isn't forced to depend on payment methods it doesn't use.

#### 5. DIP: Dependency Inversion Principle
- **Compliance Level**: **Mixed**
- **Evidence of Compliance**: The `StudentController` depends on `IStudentService` (an abstraction), not `StudentService` (a concrete implementation). The DI container in `Program.cs` wires them together.
- **Evidence of Violation**: As noted in Clean Architecture Validation, `StudentService` depends directly on `CenterManagementDbContext` (a concrete implementation belonging to a lower-level module), violating the rule that high-level modules should not depend on low-level modules, but both should depend on abstractions.

---

## STEP 6 — FULL SYSTEM FLOW (TRACING A REAL REQUEST)

To conclude this forensic audit, we will trace the exact lifecycle of a real request as it travels through the architectural layers of the Center Management System. 

**Scenario**: An Administrator submits a web form to create a new Student.

#### 1. HTTP Request (The Trigger)
The user clicks "Submit" on the HTML form located at `CenterManagement.Web\Views\Student\Create.cshtml`. 
The browser sends an `HTTP POST` request to the `/Student/CreatePost` endpoint. The payload contains form data mapping to the `CreateStudentViewModel`.

#### 2. Controller Layer (The Entry Point)
- **File**: `CenterManagement.Web\Controllers\StudentController.cs`
- **Execution**: The ASP.NET Core Router maps the request to the `CreatePost(CreateStudentViewModel model)` method.
- **Action**: 
  - The controller checks `ModelState.IsValid`. 
  - It extracts the `AdminId` from the active HTTP Context using `User.FindFirstValue(ClaimTypes.NameIdentifier)`.
  - It maps the UI-specific `CreateStudentViewModel` into a strict, application-layer `CreateStudentDto`.
  - It calls `await _studentService.CreateStudentAsync(dto, adminId);`. 
  - *The Controller now suspends execution, waiting for the Application layer.*

#### 3. Application Layer (The Business Logic)
- **File**: `CenterManagement.Application\Services\StudentService.cs`
- **Execution**: The DI container resolves `_studentService` to the concrete `StudentService` class.
- **Action**:
  - The service opens a database transaction: `using var transaction = await _db.Database.BeginTransactionAsync();`.
  - **Identity Interaction**: It constructs an `ApplicationUser` entity, calculates a default password (`"Student@" + dto.ParentPhone[^4..]`), and calls `await _userManager.CreateAsync(user, defaultPassword)`.
  - **File System Interaction**: If `dto.Photo` exists, it calls `await _fileUpload.UploadPhotoAsync()`, which physically writes the binary stream to `wwwroot\uploads\students` and returns a string path.
  - **Domain Model Creation**: It instantiates the `StudentProfile` and loop-creates `Enrollment` domain entities for every selected Group.
  - **Commitment**: It calls `await _db.SaveChangesAsync()` to flush the profiles/enrollments to the database, then `transaction.CommitAsync()`.
  - **Auditing**: It triggers `await _audit.LogAsync("StudentCreated", ...)` to record the action.
  - *The Service returns the new Student's ID back to the Controller.*

#### 4. Infrastructure Layer (The Data Persistence)
- **File**: `CenterManagement.Infrastructure\Persistence\CenterManagementDbContext.cs`
- **Execution**: Triggered when `StudentService` calls `SaveChangesAsync()`.
- **Action**:
  - EF Core intercepts the save operation. The overridden `SaveChanges()` method runs `UpdateTimestamps()`.
  - It scans the Entity Change Tracker. Finding that the new `StudentProfile` and `Enrollment` entities (which inherit from `BaseEntity`) are in the `EntityState.Added` state, it injects `UpdatedAt = DateTime.UtcNow`.
  - EF Core translates the C# entities into raw `INSERT INTO [StudentProfiles]` SQL queries.

#### 5. Database Execution (The Storage)
- **Execution**: The generated SQL queries hit the Microsoft SQL Server database.
- **Action**: Rows are physically written to the `AspNetUsers`, `AspNetUserRoles`, `StudentProfiles`, and `Enrollments` tables. Foreign key constraints configured in `OnModelCreating` are validated by the database engine.

#### 6. Response (The Return Trip)
- **File**: `CenterManagement.Web\Controllers\StudentController.cs`
- **Execution**: Control returns to the `CreatePost` method.
- **Action**: 
  - The Controller sets a success message: `TempData["Success"] = "Student created successfully."`.
  - It returns an HTTP 302 Redirect via `RedirectToAction(nameof(Index))`.
- **Final Result**: The browser receives the redirect, makes a new `GET /Student/Index` request, and the user sees the updated list of students rendered by `CenterManagement.Web\Views\Student\Index.cshtml`.

---
### END OF AUDIT
