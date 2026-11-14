# Room & Coworking Booking System (Astana)

**Authors:** Adilet Duisekenov & Deniz Akhmadjanov 

**Technology Stack:** .NET 10, Blazor Server, SignalR, Entity Framework Core, SQLite, Clean Architecture, xUnit

A real-time web application for booking university campus rooms and verified coworking spaces in Astana.

---

## Problem Statement & Practical Significance

Students, researchers, and faculty members often face fragmented room scheduling, double bookings, and limited information about available equipment across university campuses and partner coworking spaces.

These locations include:

- Astana Hub
- National Academic Library of the Republic of Kazakhstan
- iHUB
- Forte Kulanshi Art Space

This platform brings campus and city workspaces together in one real-time booking system. It provides role-based access control (RBAC), conflict-free multi-hour reservations, and administrative analytics.

---

## Technology Stack & Design Patterns

- **Framework & Runtime:** .NET 10, ASP.NET Core Blazor Server (Interactive Server Components)
- **Real-Time Communication:** ASP.NET Core SignalR (`BookingHub` & `BookingHubClient`)
- **Database & ORM:** Entity Framework Core with SQLite (`roombooking.db`)
- **Architecture:** Clean Architecture
- **Design Patterns:** Repository Pattern, Unit of Work, Result Pattern (`Result<T>`), Centralized State Container (`BookingStateContainer`)
- **Automated Testing:** xUnit (`RoomBooking.Tests`)

---

## Project Structure

The project follows Clean Architecture principles, separating business logic, infrastructure, and presentation layers.

```text
astana-coworking-den-adilet/
├── src/
│   ├── RoomBooking.Domain/
│   │   └── Core entities, enums, and domain rules
│   ├── RoomBooking.Application/
│   │   └── Business logic, DTOs, policies, and interfaces
│   ├── RoomBooking.Infrastructure/
│   │   └── EF Core DbContext, repositories, UnitOfWork, and database seeder
│   └── RoomBooking.Web/
│       └── Blazor UI, SignalR Hub, DemoAuthService, and responsive CSS
│
└── tests/
    └── RoomBooking.Tests/
        └── Automated xUnit tests
```

**Dependency direction:**

`Web → Infrastructure → Application → Domain`

---

## Key Features

### 1. Real-Time Schedule Synchronization

Powered by SignalR, available time slots are updated immediately across connected browser sessions without requiring a page reload.

### 2. Role-Based Access Control (RBAC)

The system supports three user roles:

- **Student:** Access to coworking hubs, computer labs, and study spaces.
- **Teacher:** Access to student spaces and restricted university lecture halls.
- **Admin:** Full platform access, including workspace management, reservation moderation, activation/deactivation of spaces, and analytics.

### 3. Multi-Hour Reservations & Conflict Prevention

- Supports consecutive reservations from 1 to 4 hours.
- Prevents overlapping bookings.
- Uses transactional conflict checks through `HasOverlapAsync`.

### 4. Multi-Criteria Filtering

Users can filter workspaces by:

- Date
- Location zone (University Campus or Astana City Hubs)
- Workspace category
- Access tariff (Free or Paid)
- Minimum capacity
- Available equipment (Wi-Fi, Projector, Coffee Point, etc.)

### 5. Admin Analytics Dashboard

The administrator dashboard provides key performance indicators (KPIs):

- Bookings Today
- Most Popular Space
- Peak Hour
- Cancellation Rate

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Installation & Running

1. Clone the repository:

```bash
git clone <repository-url>
```

2. Navigate to the project directory:

```bash
cd astana-coworking-den-adilet
```

3. Restore dependencies:

```bash
dotnet restore
```

4. Run the application:

```bash
dotnet run --project src/RoomBooking.Web
```

5. Open the application in your browser:

```text
http://localhost:5033
```

The SQLite database (`roombooking.db`) is automatically created and seeded with initial demo data on the first startup.

### Running Automated Tests

To run the automated test suite, execute:

```bash
dotnet test tests/RoomBooking.Tests
```

---

## Demo Accounts

The application provides the following demo accounts for testing different user roles:

| Role | Full Name | Email | Password |
|---|---|---|---|
| **Student** | Deniz Akhmadjanov | `student@roombooking.kz` | `Student123!` |
| **Teacher** | Tr. Adilet | `teacher@roombooking.kz` | `Teacher123!` |
| **Admin** | System Administrator | `admin@roombooking.kz` | `Admin123!` |

---

## Project Goals

The main goal of this project is to simplify workspace reservations in Astana by providing a centralized, user-friendly, and real-time booking platform for students, teachers, and administrators.
