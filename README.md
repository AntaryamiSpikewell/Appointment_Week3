# 📅 Appointment Management API

An ASP.NET Core Web API for managing appointments with role-based access control, JWT authentication, and SQL Server as the backend.

---

## 🚀 Features

➜ User Registration and Login with JWT Authentication  
➜ Role-based Authorization (Admin vs User)  
➜ Appointment CRUD Operations  
➜ Filter Appointments by Date and Requestor  
➜ Secure Password Hashing  
➜ AutoMapper Integration  
➜ Swagger UI for API Testing  

---

## 🛠️ Project Setup Instructions

### 🔃 Clone the Repository

```bash
git clone https://github.com/AntaryamiSpikewell/AppointmentManagement
cd AppointmentManagementAPI
```

### 🔧 Setup SQL Server Database

Run the following SQL script to create the required tables:

```sql
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    Role NVARCHAR(50) NOT NULL CHECK (Role IN ('User', 'Admin')),
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Appointments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ScheduledDate DATETIME NOT NULL,
    Status NVARCHAR(50) NOT NULL CHECK (Status IN ('Scheduled', 'Rescheduled', 'Completed', 'Cancelled')),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

### ⚙️ Update `appsettings.json`

Make sure your `appsettings.json` contains the correct database connection and JWT secret:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=AppointmentDB;Trusted_Connection=True;"
},
"Jwt": {
  "Secret": "YOUR_SUPER_SECRET_KEY"
}
```

### 📦 Install Dependencies

```bash
dotnet restore
```

### ▶️ Run the Project

```bash
dotnet run
```

Visit: `https://localhost:5001/swagger` or `http://localhost:5000/swagger` for Swagger UI.

---

## 🧩 Folder Structure

```bash
AppointmentManagementAPI/
├── Controllers/
│   ├── AppointmentsController.cs
│   └── AuthController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── DTOs/
│   ├── AppointmentDto.cs
│   ├── LoginDto.cs
│   ├── RescheduleRequestDto.cs
│   └── UserDto.cs
├── Mappings/
│   └── MappingProfile.cs
├── Models/
│   ├── Appointment.cs
│   └── User.cs
├── Repositories/
│   ├── Interfaces/
│   │   └── IAppointmentRepository.cs
│   └── UserRepository.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IAppointmentService.cs
│   │   └── IAuthService.cs
│   ├── AppointmentService.cs
│   ├── AuthService.cs
│   └── JwtService.cs
├── appsettings.json
├── AppointmentManagementAPI.http
└── Program.cs
```

---

## 🔐 Auth Endpoints

| Method | Endpoint            | Description            |
|--------|---------------------|------------------------|
| POST   | `/api/auth/signup`  | Register a new user    |
| POST   | `/api/auth/login`   | Login and get JWT token|

---

## 📋 Appointment Endpoints

| Method | Endpoint                                   | Description                          |
|--------|--------------------------------------------|--------------------------------------|
| GET    | `/api/Appointments`                        | Get all appointments                 |
| POST   | `/api/Appointments`                        | Create a new appointment             |
| GET    | `/api/Appointments/{id}`                   | Get appointment by ID                |
| PUT    | `/api/Appointments/{id}`                   | Update appointment by ID             |
| DELETE | `/api/Appointments/{id}`                   | Delete appointment by ID             |
| GET    | `/api/Appointments/by-date/{date}`         | Get appointments by date             |
| GET    | `/api/Appointments/by-requestor/{name}`    | Get appointments by requestor name   |
| PUT    | `/api/Appointments/{id}/reschedule`        | Reschedule an appointment            |
| PUT    | `/api/Appointments/{id}/complete`          | Mark appointment as complete         |
| PUT    | `/api/Appointments/{id}/cancel`            | Cancel appointment                   |

---

## 🔐 Role-Based Access for Appointment Endpoints

| HTTP Method | Endpoint                                | User Access        | Admin Access     |
|-------------|------------------------------------------|--------------------|------------------|
| GET         | `/api/Appointments`                      | ❌                 | ✅               |
| POST        | `/api/Appointments`                      | ✅                 | ✅               |
| GET         | `/api/Appointments/{id}`                 | ✅ (own)           | ✅ (all)         |
| PUT         | `/api/Appointments/{id}`                 | ✅ (own)           | ✅ (all)         |
| DELETE      | `/api/Appointments/{id}`                 | ❌                 | ✅               |
| GET         | `/api/Appointments/by-date/{date}`       | ✅                 | ✅               |
| GET         | `/api/Appointments/by-requestor/{name}`  | ✅ (own)           | ✅ (all)         |
| PUT         | `/api/Appointments/{id}/reschedule`      | ✅ (own)           | ✅ (all)         |
| PUT         | `/api/Appointments/{id}/complete`        | ❌                 | ✅               |
| PUT         | `/api/Appointments/{id}/cancel`          | ✅ (own)           | ✅ (all)         |

---

## 📄 License

MIT License. Feel free to use and contribute.

---

## 💬 Questions or Feedback?

Feel free to raise an issue or connect with me.
