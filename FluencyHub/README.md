# FluencyHub - Language Learning Platform

FluencyHub is a comprehensive language learning platform built with ASP.NET Core 9.0, following Clean Architecture principles, Domain-Driven Design (DDD), and Command Query Responsibility Segregation (CQRS) patterns.

## Project Structure

The application is organized into the following projects:

- **FluencyHub.Domain** - Contains the enterprise business rules and entities, representing the core domain model.
- **FluencyHub.Application** - Contains application business rules and use cases, implementing the CQRS pattern with MediatR.
- **FluencyHub.Infrastructure** - Contains adapters and implementations for external concerns like databases, identity, etc.
- **FluencyHub.API** - ASP.NET Core Web API project that serves as the entry point for all HTTP requests.
- **FluencyHub.Tests** - Contains unit and integration tests for the application.

## Bounded Contexts

The application is divided into three main bounded contexts:

1. **Content Management** - Responsible for managing courses, lessons, and all educational content.
2. **Student Management** - Responsible for student registration, enrollment, and progress tracking.
3. **Payment Processing** - Responsible for handling payments, refunds, and integration with payment gateways.

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or Visual Studio Code

### Setup

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/FluencyHub.git
   ```

2. Navigate to the project directory:
   ```
   cd FluencyHub
   ```

3. Restore dependencies:
   ```
   dotnet restore
   ```

4. Build the solution:
   ```
   dotnet build
   ```

5. Run the API:
   ```
   cd FluencyHub.API
   dotnet run
   ```

The API will be available at `https://localhost:5001` and `http://localhost:5000`.

## API Documentation

Swagger documentation is available at `/swagger` when running the application in development mode.

### Main Endpoints

#### Authentication

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and get a JWT token

#### Courses

- `GET /api/courses` - Get all courses
- `GET /api/courses/{id}` - Get a specific course by ID
- `POST /api/courses` - Create a new course (requires Administrator role)
- `PUT /api/courses/{id}` - Update a course (requires Administrator role)
- `DELETE /api/courses/{id}` - Delete a course (requires Administrator role)

#### Students

- `GET /api/students` - Get all students (requires Administrator role)
- `GET /api/students/{id}` - Get a specific student by ID
- `GET /api/students/me` - Get the current student's information
- `PUT /api/students/{id}` - Update a student (requires Administrator role or ownership)

#### Enrollments

- `POST /api/enrollments` - Enroll in a course
- `GET /api/enrollments` - Get all enrollments for the current student
- `GET /api/enrollments/{id}` - Get a specific enrollment by ID

#### Payments

- `POST /api/payments` - Process a payment for an enrollment
- `GET /api/payments` - Get all payments for the current student
- `POST /api/payments/{id}/refund` - Request a refund for a payment

## Authentication and Authorization

The application uses JWT (JSON Web Tokens) for authentication. All requests to protected endpoints must include an `Authorization` header with a valid JWT token:

```
Authorization: Bearer <token>
```

## Testing

To run the tests:

```
dotnet test
```

## License

This project is licensed under the MIT License - see the LICENSE file for details. 