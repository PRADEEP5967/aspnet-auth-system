# ASP.NET Core MVC Authentication System

A complete, production-ready authentication system built with ASP.NET Core 10 MVC using ASP.NET Core Identity with role-based access control.

## 🎯 Features

✅ **User Authentication**
- User Registration
- User Login with Remember Me
- Secure Logout
- Password Reset Functionality
- Account Lockout on Failed Attempts

✅ **Role-Based Access Control**
- **Admin** - Full system access and administration
- **Teacher** - Create and manage courses, view enrolled students
- **Student** - Browse and enroll in courses

✅ **Course Management**
- Teachers can create, edit, and delete courses
- Students can browse available courses and enroll
- Track student enrollments

✅ **Security Features**
- ASP.NET Core Identity with password hashing
- CSRF protection with anti-forgery tokens
- Secure authentication cookies
- Database-level data validation
- Authorization attributes on controllers

✅ **Database**
- SQL Server integration
- Entity Framework Core with DbContext
- Automatic migrations support

✅ **Repository Pattern**
- Generic repository implementation
- Specific repository for course operations
- Clean separation of concerns

## 📋 Prerequisites

- .NET 10 SDK
- SQL Server (Local or Express)
- Visual Studio 2022 or Visual Studio Code

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/PRADEEP5967/aspnet-auth-system.git
cd aspnet-auth-system
```

### 2. Configure Database Connection

Edit `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=AspNetAuthSystemDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Create Database & Run Migrations

```bash
dotnet ef database update
```

This will:
- Create the database
- Create all tables
- Seed initial data with demo users

### 4. Run the Application

```bash
dotnet run
```

The application will be available at `https://localhost:7000` (or the port shown in console)

## 👥 Demo Users

Use these credentials to test the application:

| Role    | Username | Password      |
|---------|----------|---------------|
| Admin   | admin    | Admin@123     |
| Teacher | teacher  | Teacher@123   |
| Student | student  | Student@123   |

## 📁 Project Structure

```
AspNetAuthSystem/
├── Controllers/
│   ├── AccountController.cs       # Authentication endpoints
│   ├── DashboardController.cs     # Role-based dashboards
│   ├── CourseController.cs        # Course management
│   └── HomeController.cs          # Home pages
├── Models/
│   ├── User.cs                    # Extended IdentityUser
│   ├── Role.cs                    # Extended IdentityRole
│   ├── Course.cs                  # Course entity
│   └── StudentCourse.cs           # Enrollment junction table
├── DTOs/
│   ├── RegisterDTO.cs             # Registration form data
│   ├── LoginDTO.cs                # Login form data
│   ├── ResetPasswordDTO.cs        # Password reset form data
│   ├── CourseDTO.cs               # Course transfer object
│   └── UserDTO.cs                 # User transfer object
├── Data/
│   └── ApplicationDbContext.cs    # Entity Framework DbContext
├── Repository/
│   ├── IRepository.cs             # Generic repository interface
│   ├── Repository.cs              # Generic repository implementation
│   ├── ICourseRepository.cs       # Course repository interface
│   └── CourseRepository.cs        # Course repository implementation
├── Views/
│   ├── Account/                   # Authentication views
│   ├── Dashboard/                 # Role-based dashboard views
│   ├── Course/                    # Course management views
│   ├── Home/                      # Home page views
│   └── Shared/                    # Shared layout and partials
├── wwwroot/
│   ├── css/                       # Custom stylesheets
│   └── js/                        # JavaScript files
├── appsettings.json               # Configuration settings
└── Program.cs                     # Application startup configuration
```

## 🔐 Security Features

1. **Password Policy**
   - Minimum 6 characters
   - No complexity requirements (can be customized)

2. **Account Lockout**
   - 5 failed login attempts trigger lockout
   - 15-minute lockout duration

3. **Authentication Cookie**
   - HttpOnly flag enabled
   - Sliding expiration (1 hour)
   - HTTPS required in production

4. **Authorization**
   - Role-based access control on all endpoints
   - [Authorize] attributes on protected controllers
   - Access Denied handling

## 📚 API Endpoints

### Authentication
- `GET /Account/Register` - Registration page
- `POST /Account/Register` - Process registration
- `GET /Account/Login` - Login page
- `POST /Account/Login` - Process login
- `POST /Account/Logout` - Logout
- `GET /Account/ForgotPassword` - Forgot password page
- `POST /Account/ForgotPassword` - Send reset email
- `GET /Account/ResetPassword` - Reset password page
- `POST /Account/ResetPassword` - Process password reset

### Dashboard
- `GET /Dashboard/Admin` - Admin dashboard (Admin only)
- `GET /Dashboard/Teacher` - Teacher dashboard (Teacher only)
- `GET /Dashboard/Student` - Student dashboard (Student only)
- `GET /Dashboard/Profile` - User profile (All authenticated)

### Courses
- `GET /Course/Index` - Browse all courses
- `GET /Course/MyCourses` - My enrolled courses (Student only)
- `GET /Course/Create` - Create course form (Teacher only)
- `POST /Course/Create` - Create course (Teacher only)
- `GET /Course/Edit/{id}` - Edit course form (Teacher only)
- `POST /Course/Edit/{id}` - Update course (Teacher only)
- `POST /Course/Delete/{id}` - Delete course (Teacher only)
- `GET /Course/Details/{id}` - View course details
- `GET /Course/Enroll/{id}` - Enroll in course (Student only)
- `GET /Course/TeacherCourses` - View my courses (Teacher only)

## 🔄 Database Schema

### Users Table
- Extended from AspNetUsers
- FirstName, LastName, CreatedAt, UpdatedAt, IsActive

### Roles Table
- Extended from AspNetRoles
- Description, CreatedAt

### Courses Table
- Id (PK)
- Title
- Description
- TeacherId (FK -> Users)
- CreatedAt

### StudentCourses Table
- Id (PK)
- StudentId (FK -> Users)
- CourseId (FK -> Courses)
- EnrolledAt

### ASP.NET Identity Tables
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetUserLogins
- AspNetRoleClaims
- AspNetUserTokens

## 🛠️ Customization

### Add Custom Properties to User

Edit `Models/User.cs` and add properties as needed, then create a migration:

```bash
dotnet ef migrations add AddCustomUserProperties
dotnet ef database update
```

### Change Password Policy

Edit `Program.cs` in the Identity configuration section:

```csharp
options.Password.RequiredLength = 8;
options.Password.RequireDigit = true;
options.Password.RequireUppercase = true;
```

### Add Email Sending

Implement email service and inject into AccountController:

```csharp
// In Program.cs
builder.Services.AddScoped<IEmailSender, EmailSender>();

// In AccountController
private readonly IEmailSender _emailSender;
```

## 📝 Migration Commands

```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Update database to latest migration
dotnet ef database update

# Revert to previous migration
dotnet ef database update PreviousMigrationName

# Remove last migration
dotnet ef migrations remove
```

## 🧪 Testing

Test with the demo users provided. Try:

1. Registering a new account
2. Logging in with different roles
3. Creating courses as a teacher
4. Enrolling in courses as a student
5. Attempting unauthorized access
6. Password reset functionality

## 📦 Dependencies

- ASP.NET Core Identity EntityFrameworkCore
- ASP.NET Core Identity UI
- Entity Framework Core
- Entity Framework Core SQL Server
- Bootstrap (CDN)
- jQuery Validation (via CDN)

## 🚨 Production Checklist

- [ ] Update database connection string for production
- [ ] Implement email sending for password reset
- [ ] Enable HTTPS only
- [ ] Configure CORS if needed
- [ ] Implement logging
- [ ] Add input validation on client side
- [ ] Configure exception handling middleware
- [ ] Enable security headers
- [ ] Set strong password policy
- [ ] Review and adjust lockout policy
- [ ] Implement audit logging
- [ ] Set up database backups

## 📖 Documentation

- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 💬 Support

For issues and questions, please open an issue on GitHub.

## 👨‍💻 Author

**Pradeep**
- GitHub: [@PRADEEP5967](https://github.com/PRADEEP5967)

## 🎓 Learning Resources

This project implements best practices for:
- ASP.NET Core Identity integration
- Role-based authorization
- Repository pattern
- Entity Framework Core
- MVC architecture
- Secure authentication flows

---

**Happy Coding! 🚀**
