# Vehicle Parts Selling & Inventory Management System

A comprehensive full-stack ASP.NET Core web application for managing vehicle parts inventory, sales, customer interactions, and appointments.

## 🚀 Features

### Admin & Staff Management
- Full administrative dashboard with financial and inventory overviews.
- Vendor and supplier management for easy restocking.
- Detailed report generation (Sales, Inventory, and Financial reports).
- Invoice generation for in-store purchases.

### Inventory Control
- Real-time stock tracking and updates.
- Centralized vehicle part catalog categorized by make and model.
- Low-stock alerts and history tracking.

### Customer Portal
- Seamless registration and authentication system.
- Browse and search vehicle parts catalog.
- Book and manage service appointments.
- Review and rate purchased parts.

## 🛠️ Technology Stack
- **Backend Framework:** ASP.NET Core Web API (C#)
- **Frontend:** ASP.NET Core MVC / Razor Pages (HTML, CSS, Bootstrap, JS)
- **Database:** PostgreSQL / SQLite
- **ORM:** Entity Framework Core
- **Authentication:** JWT (JSON Web Tokens)

## 📁 Project Structure
The solution consists of two main layers:
- `src/VehiclePartsSystem.API/` - The backend API managing the database and business logic.
- `src/VehiclePartsSystem.Web/` - The frontend web application presenting the user interface.

## ⚙️ How to Run Locally
1. Clone the repository.
2. Open the solution file `VehiclePartsSystem.sln` in Visual Studio.
3. Set the startup project to **Multiple startup projects** (Both Web and API).
4. Run the application. The system will automatically apply database migrations on startup.

Alternatively, via CLI:
```bash
# Terminal 1 - Run the API
cd src/VehiclePartsSystem.API
dotnet run

# Terminal 2 - Run the Web Client
cd src/VehiclePartsSystem.Web
dotnet run
```

## 📄 Documentation
Comprehensive documentation, including UML diagrams (ER, Use Case, Class, Activity) and a full test execution guide, are available in the repository root files.
