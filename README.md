# HR Platform API

A RESTful API for managing candidates and their skills, built with ASP.NET Core and Entity Framework Core.

## Features

- **Candidate Management**: Create, read, update, and delete candidates
- **Skills Management**: Associate multiple skills with candidates
- **Search Functionality**: Search candidates by name and/or skills
- **Data Validation**: Email and phone number uniqueness validation
- **Comprehensive Testing**: Unit tests for controllers and services using xUnit and Moq

## Technologies

- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core 9.0
- **Testing**: xUnit, Moq
- **API Documentation**: Swagger/OpenAPI

## Database Schema

### Tables
- **Candidates**: Stores candidate information (name, email, phone, birthday)
- **Skills**: Stores available skills
- **CandidateSkills**: Junction table for many-to-many relationship

## Setup Instructions

### 1. Clone the Repository
```bash
git clone <repository-url>
cd HR_Platform_App
```

### 2. Configure Database Connection
Update the connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": ""
}
```

### 3. Apply Database Migrations
```bash
cd HR_Platform
dotnet ef database update
```

### 4. Run the Application
```bash
dotnet run
```

The API will be available at `http://localhost:5018`

### 5. Access Swagger UI
Navigate to `http://localhost:5018/swagger` to explore and test the API endpoints.

## API Endpoints

### Candidates

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/candidates` | Get all candidates |
| GET | `/api/candidates/{id}` | Get candidate by ID |
| GET | `/api/candidates/search?name={name}&skillIds={ids}` | Search candidates |
| POST | `/api/candidates` | Create new candidate |
| POST | `/api/candidates/with-skills` | Create candidate with skills |
| PUT | `/api/candidates/{id}` | Update candidate |
| DELETE | `/api/candidates/{id}` | Delete candidate |
| POST | `/api/candidates/{candidateId}/skills/batch` | Add skills to candidate |
| DELETE | `/api/candidates/{candidateId}/skills/{skillId}` | Remove skill from candidate |

## Project Structure

```
HR_Platform_App/
├── HR_Platform/                    # Main API project
│   ├── Controllers/                # API controllers
│   ├── DTOs/                       # Data Transfer Objects
│   ├── Models/                     # Database entities
│   ├── Services/                   # Business logic layer
│   ├── Repositories/               # Data access layer
│   ├── Data/                       # DbContext and migrations
│   └── Helpers/                    # Utility classes
└── HR_Platform.Tests/              # Unit tests
    ├── ControllerTests/
    └── ServiceTests/
```

## Architecture

The project follows a **layered architecture**:

1. **Controllers**: Handle HTTP requests/responses
2. **Services**: Contain business logic and validation
3. **Repositories**: Manage database operations
4. **DTOs**: Define data contracts for API communication
5. **Models**: Represent database entities

## Sample Data

The database is seeded with:
- 5 sample candidates
- 8 predefined skills (C#, JavaScript, SQL, English, Database Design, Project Management, Russian, German)
- Multiple candidate-skill associations

## Running Tests

```bash
cd HR_Platform.Tests
dotnet test
```

Test coverage includes:
- Controller actions with various scenarios (success, not found, conflicts)
- Service methods with business logic validation

## Validation Rules

- **Email**: Must be unique and valid format
- **Phone Number**: Must be unique and valid format
- **Birthday**: Cannot be in the future
- **Name**: 2-100 characters
- **Skills**: Cannot add duplicate skills to a candidate

## Error Handling

The API returns appropriate HTTP status codes:
- `200 OK`: Successful operation
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid input data
- `404 Not Found`: Resource not found
- `409 Conflict`: Duplicate resource (email/phone/skill)
- `500 Internal Server Error`: Server-side error