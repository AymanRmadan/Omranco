Project Overview & Technical Documentation
To Run the Project follow these steps to set up and run the project on your local machine
git clone https://github.com/AymanRmadan/E-Learning-Platform.git
cd E-Learning-Platform 
Then open appsettings.json and update the DefaultConnection string with your SQL Server then Apply Migrations and Update-Database

User Roles & Authentication
Admin: Responsible for creating courses. Requires authentication via JWT token.
Credentials: admin@gmail.com / P@ssword123
Manager: Responsible for approving or rejecting enrollment requests. Requires authentication.
Credentials: manager@gmail.com / P@ssword123
Learner: Registered users who can browse and enroll in courses.

Learner Registration & Identity
The system uses ASP.NET Core Identity.
When a user registers, an ApplicationUser is created, and a corresponding Learner profile is generated automatically.
We established a One-to-One relationship between ApplicationUser and Learner to ensure data integrity and prevent profile duplication.
Upon successful registration, the Learner role is assigned by default.


Technical Implementation & Best Practices
Clean Architecture: The project is structured into 4 layers to ensure strict Separation of Concerns.
Repository Pattern & UoW: Implemented Generic Repository and Unit of Work to manage database transactions and improve code reusability.
Result Pattern: Used to standardize API responses and replace standard try-catch blocks, providing better control over the execution flow.
Validation: Leveraged FluentValidation with specific configurations for each entity to ensure robust data integrity.
Error Handling: Implemented a global exception handling strategy for consistent error reporting.
Security: Integrated JWT (JSON Web Token) for secure authentication and authorization.
Mapping: Used Mapster for efficient and clean object mapping between Requests, Entities, and Responses


System Life Cycle
Create Course: Admin logs in -> Obtains Token -> Accesses POST /api/courses.
Enrollment Decision: Manager logs in -> Obtains Token -> Accesses Enrollment endpoints.
Student Enrollment: User registers (Learner created) -> Logs in -> Enrolls in a course.


