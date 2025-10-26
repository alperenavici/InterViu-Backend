# Interviu - AI-Powered Interview Platform 🎯

## 📋 Project Overview

**Interviu** is an intelligent interview platform that leverages artificial intelligence to conduct personalized job interviews. The system analyzes candidates' CVs based on their applied positions, generates relevant interview questions using AI, and provides comprehensive feedback on their performance including voice analysis.

### Key Features

- 🤖 **AI-Driven Question Generation**: Automatically generates interview questions tailored to the candidate's CV and target position
- 📄 **CV Analysis**: Uploads and extracts text from PDF resumes for intelligent processing
- 🎤 **Voice & Answer Analysis**: Evaluates candidates' responses including voice characteristics
- 📊 **Performance Feedback**: Provides detailed feedback on what candidates need to improve
- 🔐 **Secure Authentication**: JWT-based authentication with role management
- 👥 **Multi-Role Support**: Admin, Interviewer, and Candidate roles
- 📈 **Interview Tracking**: Monitors interview status and generates comprehensive summaries

---

## 🛠️ Technology Stack

### Backend Framework
- **.NET 8** - Modern, high-performance web API framework
- **ASP.NET Core Web API** - RESTful API development
- **Entity Framework Core** - ORM for database operations

### Database
- **PostgreSQL** - Robust relational database
- **Entity Framework Core Migrations** - Database schema management

### Security & Authentication
- **ASP.NET Core Identity** - User management
- **JWT (JSON Web Tokens)** - Stateless authentication
- **Role-Based Authorization** - Access control

### Architecture & Design Patterns
- **Clean Architecture** - Separation of concerns
- **Repository Pattern** - Data access abstraction
- **Unit of Work Pattern** - Transaction management
- **Dependency Injection** - Loose coupling
- **AutoMapper** - Object-to-object mapping

### Additional Tools
- **Docker** - Containerization
- **Swagger/OpenAPI** - API documentation
- **CORS** - Cross-origin resource sharing

---

## 📁 Project Structure

The solution follows a clean architecture approach with clear separation of concerns:

```
Interviu/
│
├── Interviu.Entity/          # Domain Layer
│   ├── Entities/             # Domain models
│   │   ├── ApplicationUser.cs
│   │   ├── CV.cs
│   │   ├── Interview.cs
│   │   ├── InterviewQuestion.cs
│   │   └── Question.cs
│   └── Enums/
│       ├── Difficulty.cs     # EASY, MEDIUM, HARD
│       └── InterviewStatus.cs # ONGOING, COMPLETED, CANCELLED
│
├── Interviu.Core/            # Application Layer
│   └── DTOs/                 # Data Transfer Objects
│       ├── LoginDto.cs
│       ├── RegisterDto.cs
│       ├── CvDto.cs
│       ├── InterviewDto.cs
│       ├── QuestionDto.cs
│       ├── SubmitAnswerDto.cs
│       └── ...
│
├── Interviu.Data/            # Infrastructure Layer
│   ├── Context/
│   │   └── ApplicationDbContext.cs
│   ├── IRepositories/        # Repository interfaces
│   ├── Repositories/         # Repository implementations
│   ├── UnitOfWork/
│   ├── Migrations/           # EF Core migrations
│   └── Seed/                 # Database seeding
│
├── Interviu.Service/         # Business Logic Layer
│   ├── IServices/            # Service interfaces
│   ├── Services/             # Service implementations
│   │   ├── UserService.cs
│   │   ├── CvService.cs
│   │   ├── QuestionService.cs
│   │   └── InterviewService.cs
│   ├── Mappings/             # AutoMapper profiles
│   └── Exceptions/           # Custom exceptions
│
└── Interviu.WebApi/          # Presentation Layer
    ├── Controllers/          # API endpoints
    │   ├── AuthController.cs
    │   ├── UserController.cs
    │   ├── CvController.cs
    │   ├── QuestionController.cs
    │   └── InterviewController.cs
    ├── Program.cs            # Application entry point
    ├── appsettings.json      # Configuration
    └── PrivateFiles/uploads/ # CV file storage
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/) or [Docker](https://www.docker.com/)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Interviu
   ```

2. **Configure Database Connection**
   
   Update the connection string in `Interviu.WebApi/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=Interviu;User Id=postgres;Password=yourpassword"
     }
   }
   ```

3. **Configure JWT Settings**
   
   Update JWT settings in `appsettings.json`:
   ```json
   {
     "Jwt": {
       "Key": "YourSecretKeyMustBeAtLeast32CharactersLong!",
       "Issuer": "InterviumAPI",
       "Audience": "InterviumClient"
     }
   }
   ```

4. **Apply Database Migrations**
   ```bash
   cd Interviu.WebApi
   dotnet ef database update --project ../Interviu.Data
   ```

5. **Run the Application**
   ```bash
   dotnet run
   ```

   The API will be available at `https://localhost:5001` or `http://localhost:5000`

### Docker Deployment

1. **Using Docker Compose**
   ```bash
   docker-compose up --build
   ```

   The API will be available at `http://localhost:8080`

2. **Using Docker Manually**
   ```bash
   docker build -t interviu-api .
   docker run -p 8080:8080 interviu-api
   ```

---

## 📡 API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register new user | ❌ |
| POST | `/api/auth/login` | User login | ❌ |

### User Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/user` | Get all users | ✅ Admin |
| GET | `/api/user/{id}` | Get user by ID | ✅ |
| GET | `/api/user/{id}/with-cv` | Get user with CV | ✅ |
| POST | `/api/user/assign-role` | Assign role to user | ✅ Admin |

### CV Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/cv/upload` | Upload CV (PDF) | ✅ |
| GET | `/api/cv/{id}` | Get CV by ID | ✅ |
| GET | `/api/cv/user/{userId}` | Get user's CVs | ✅ |
| DELETE | `/api/cv/{id}` | Delete CV | ✅ |

### Question Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/question` | Get all questions | ✅ |
| GET | `/api/question/{id}` | Get question by ID | ✅ |
| POST | `/api/question` | Create question | ✅ Admin |
| PUT | `/api/question/{id}` | Update question | ✅ Admin |
| DELETE | `/api/question/{id}` | Delete question | ✅ Admin |

### Interview Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/interview/start` | Start new interview | ✅ |
| POST | `/api/interview/submit-answer` | Submit answer | ✅ |
| POST | `/api/interview/complete` | Complete interview | ✅ |
| GET | `/api/interview/{id}` | Get interview details | ✅ |
| GET | `/api/interview/user/{userId}` | Get user's interviews | ✅ |
| GET | `/api/interview/{id}/summary` | Get interview summary | ✅ |

---

## 🗄️ Database Schema

### Key Entities

#### ApplicationUser
- Identity user with custom properties
- Relationships: CVs, Interviews

#### CV
- **Id**: Unique identifier
- **FileName**: Original file name
- **FilePath**: Storage path
- **ExtractedText**: Parsed CV content
- **UploadDate**: Upload timestamp
- **UserId**: Owner reference

#### Question
- **Id**: Unique identifier
- **Text**: Question content
- **Category**: Question category (e.g., "Technical", "Behavioral")
- **Difficulty**: EASY, MEDIUM, or HARD

#### Interview
- **Id**: Unique identifier
- **Position**: Target job position
- **Status**: ONGOING, COMPLETED, or CANCELLED
- **OverallScore**: Final interview score
- **OverallFeedback**: AI-generated feedback
- **StartedAt**: Interview start time
- **CompletedAt**: Interview completion time
- **UserId**: Candidate reference
- **CvId**: Associated CV (optional)

#### InterviewQuestion (Junction Table)
- **Id**: Unique identifier
- **AnswerText**: Candidate's answer
- **Score**: Answer score
- **Feedback**: Question-specific feedback
- **InterviewId**: Interview reference
- **QuestionId**: Question reference

---

## 🔧 Development

### Adding a New Migration

```bash
cd Interviu.Data
dotnet ef migrations add MigrationName --startup-project ../Interviu.WebApi
dotnet ef database update --startup-project ../Interviu.WebApi
```

### Running Tests

```bash
dotnet test
```



## 🌟 Features in Detail

### 1. CV Analysis
- Upload PDF resumes
- Automatic text extraction
- Storage in PostgreSQL for easy querying
- Association with user profiles

### 2. AI-Powered Interviews
- Questions generated based on:
  - Target position
  - CV content
  - Difficulty level
- Dynamic question selection
- Real-time answer submission

### 3. Comprehensive Feedback
- Per-question scoring and feedback
- Overall interview score
- Actionable improvement suggestions
- Voice analysis integration (future enhancement)

### 4. Role-Based Access Control
- **Admin**: Full system access
- **Interviewer**: Question management, interview monitoring
- **Candidate**: CV upload, interview participation

---

## 🔒 Security Features

- **JWT Authentication**: Secure, stateless authentication
- **Password Hashing**: ASP.NET Core Identity with secure password storage
- **Role-Based Authorization**: Fine-grained access control
- **CORS Configuration**: Controlled cross-origin requests
- **File Upload Validation**: Secure PDF file handling

---

## 📝 Configuration

### Environment Variables

The application can be configured using environment variables:

- `ASPNETCORE_ENVIRONMENT`: Development, Staging, or Production
- `ASPNETCORE_URLS`: Server URLs (e.g., `http://0.0.0.0:8080`)

### appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "PostgreSQL connection string"
  },
  "Jwt": {
    "Key": "Secret key (min 32 characters)",
    "Issuer": "API issuer",
    "Audience": "API audience"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is proprietary and confidential.

---

## 👥 Authors

- **Development Team** - Initial work

---

## 🙏 Acknowledgments

- ASP.NET Core Team for the excellent framework
- Entity Framework Core Team for the robust ORM
- AutoMapper contributors
- PostgreSQL community

---

## 📞 Support

For support, please open an issue in the repository or contact the development team.

---

**Made with ❤️ using .NET 8**

