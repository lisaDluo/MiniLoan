MiniLoan
A minimal loan application built with .NET 9 Web API + lightweight frontend (vanilla HTML/JS) for creating users, creating loans, and viewing amortization schedules, demonstrating clean architecture, dependency injection, repository pattern, and unit testing with xUnit and Moq, Dockerfile for containerization.

---------------

Features

• User management (create, list users)
•	Loan creation with amount, interest, term, and frequency
• Loan schedule calculation (amortization-style payments)
• Summary reporting per user
•	Repositories for Users and Loans (In-Memory for demo).
•	Dependency Injection (DI) setup in Program.cs.
•	Unit Tests validating core business logic with xUnit.
•	REST API endpoints for loan operations (ASP.NET Core).
• Built-in logging to console (extensible to file logging later)
• Swagger UI for interactive API exploration
• Dockerfile included for easy container builds

---------------

Prerequisites
•	.NET 9 SDK or later
•	Git
• Docker (optional, for container builds)
---------------

Project Structure

MiniLoan/
│
├── src/
│   └── MiniLoan.Api/          # Main API project
│       ├── Controllers/       # API controllers
│       ├── Models/            # Entities & enums (Loan, User, PaymentFrequency)
│       ├── Repositories/      # Interfaces & in-memory repositories
│       ├── Services/          # LoanService and business logic
│       └── Program.cs         # ASP.NET Core entry point
│       ├── Dockerfile         # Container build file 
└── tests/
|    └── MiniLoan.Tests/        # Unit tests using xUnit & Moq
└── README.md
---------------

Getting Started (Visual Studio Code)
1. Clone the repository

  git clone <your-repo-url>
  cd MiniLoan.Api

2. Restore dependencies

  dotnet restore

3. Build the solution

  dotnet build

4. Run the API

  dotnet run

By default, the API will be available at:

http://localhost:5000

https://localhost:7001

---------------

Running Tests

cd MiniLoan.Tests

dotnet test

---------------

API Endpoints (examples)
Users

• POST /api/users → create a user
• GET /api/users/{userId}/loans → list loans for a user

Loans

• POST /api/loans → create a loan for a user
• GET /api/loans/{loanId}/schedule → get loan schedule
• GET /api/loans/{loanId}/summary → get loan summary

---------------

Logging

• Currently configured to log to console via ILogger<T>.
• Easy to swap to file logging with log4net or other providers later.
• Logging configuration is controlled in appsettings.json.

---------------

Swagger (API Documentation)

• Swagger is enabled by default for this API:
• Provides interactive API documentation.
• Allows developers to explore and test endpoints directly from the browser.
• Ensures that endpoints and request/response models are clearly described.
• Reduces onboarding time for new developers and helps validate APIs without needing tools like Postman.

Access it at:
https://localhost:5000/swagger

---------------

Docker

The project includes a Dockerfile. To build and run:
  # from project root
  docker build -t miniloan-api -f src/MiniLoan.Api/Dockerfile .  
  docker run -d -p 8080:8080 --name miniloan-container miniloan-api

Now the API is accessible at http://localhost:8080/swagger

---------------

Notes

This project is for demo/learning purposes and not production-ready.
Authentication/authorization, persistence (DB), and advanced validation are left out for simplicity.

---------------

Created by Lisa Luo 8/19/2025