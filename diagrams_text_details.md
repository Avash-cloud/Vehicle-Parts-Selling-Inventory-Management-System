# Project Details for Documentation

If you need the text representation to put into tables or paragraphs in your documentation before showing the diagrams, you can copy and paste the details below.

---

## 1. ER Diagram & Class Diagram Details (Data Dictionary)

Here are the exact attributes and classes you can describe for your database design (ERD) and Object-Oriented design (Class Diagram).

### 1. Customer
*   **Attributes/Fields:** `CustomerID` (Primary Key), `FirstName`, `LastName`, `Email`, `Phone`, `Address`
*   **Methods/Operations:** `Register()`, `BookAppointment()`, `BuyPart()`, `SubmitReview()`

### 2. Vehicle
*   **Attributes/Fields:** `VehicleID` (Primary Key), `CustomerID` (Foreign Key), `Make`, `Model`, `Year`, `LicensePlate`
*   **Methods/Operations:** `GetVehicleDetails()`, `AddVehicle()`, `UpdateVehicle()`

### 3. Staff
*   **Attributes/Fields:** `StaffID` (Primary Key), `FullName`, `Role` (Admin/Employee), `Email`, `Phone`
*   **Methods/Operations:** `ManageInventory()`, `GenerateInvoice()`, `UpdateStock()`

### 4. Vendor
*   **Attributes/Fields:** `VendorID` (Primary Key), `CompanyName`, `ContactPerson`, `Email`, `Phone`
*   **Methods/Operations:** `SupplyParts()`, `UpdateVendorDetails()`

### 5. Inventory (Vehicle Parts)
*   **Attributes/Fields:** `PartID` (Primary Key), `VendorID` (Foreign Key), `PartName`, `Category`, `Price`, `StockQuantity`
*   **Methods/Operations:** `UpdateStock()`, `CheckAvailability()`, `AddPart()`

### 6. Sales Invoice
*   **Attributes/Fields:** `InvoiceID` (Primary Key), `CustomerID` (Foreign Key), `StaffID` (Foreign Key), `InvoiceDate`, `TotalAmount`, `Status` (Paid/Pending)
*   **Methods/Operations:** `PrintInvoice()`, `CalculateTotal()`, `UpdateStatus()`

### 7. Purchase Invoice
*   **Attributes/Fields:** `PurchaseID` (Primary Key), `VendorID` (Foreign Key), `StaffID` (Foreign Key), `PurchaseDate`, `TotalAmount`
*   **Methods/Operations:** `GeneratePurchaseRecord()`

### 8. Appointment
*   **Attributes/Fields:** `AppointmentID` (Primary Key), `CustomerID` (Foreign Key), `VehicleID` (Foreign Key), `AppointmentDate`, `ServiceType`, `Status` (Scheduled/Completed/Cancelled)
*   **Methods/Operations:** `Schedule()`, `Cancel()`, `UpdateStatus()`

### 9. Review
*   **Attributes/Fields:** `ReviewID` (Primary Key), `CustomerID` (Foreign Key), `PartID` (Foreign Key), `Rating`, `Comment`, `ReviewDate`
*   **Methods/Operations:** `AddReview()`, `DeleteReview()`

---

## 2. Use Case Diagram Details

If you need to define the Actors and the specific Use Cases they can perform in a list or table:

### Actors:
1.  **Admin:** Manages the entire system, staffs, reports, and overall inventory.
2.  **Staff:** Handles day-to-day operations like inventory updates, selling parts, and generating invoices.
3.  **Customer:** Browses parts, makes purchases, books appointments, and leaves reviews.

### Use Cases:
*   **Staff Management:** (Actor: Admin) Add, edit, or remove staff accounts.
*   **Inventory Management:** (Actors: Admin, Staff) Add new parts, update stock quantity, delete old parts.
*   **Customer Registration:** (Actor: Customer) Create a new account in the system.
*   **Parts Selling:** (Actors: Customer, Staff) Process a transaction for vehicle parts.
*   **Invoice Generation:** (Actors: Staff, Admin) Generate billing receipts after a sale.
*   **Appointment Booking:** (Actor: Customer) Schedule a service or consultation.
*   **Vendor Management:** (Actors: Admin, Staff) Manage supplier details and purchase records.
*   **Report Generation:** (Actor: Admin) Generate financial and sales reports.
*   **Customer Search:** (Actors: Staff, Admin) Look up customer history and details.
*   **Review Submission:** (Actor: Customer) Leave a rating and comment on purchased parts.

---

## 3. Milestone Chart Details (Project Timeline)

If you need to create a table for section "1.7 Milestone Progress Chart", you can use these exact phases, durations, and status markers:

| Milestone Phase | Description | Estimated Duration | Status |
| :--- | :--- | :--- | :--- |
| **1. Project Planning** | Requirement gathering, team role assignment, and defining system scope. | 1 Week | Completed |
| **2. Database Design** | Creating ER diagrams, schema design, and setting up PostgreSQL. | 1 Week | Completed |
| **3. Frontend Development** | UI/UX design, creating responsive dashboards using HTML, CSS, Bootstrap. | 2 Weeks | Completed |
| **4. Backend Development** | Setting up ASP.NET Core API, implementing controllers, and services. | 3 Weeks | Completed |
| **5. API Integration** | Connecting the frontend views to the backend database endpoints. | 2 Weeks | Completed |
| **6. Testing & Debugging** | Unit testing, Swagger API testing, form validation, and fixing bugs. | 1.5 Weeks | Completed |
| **7. Documentation** | Writing reports, creating UML diagrams, and formatting manuals. | 1 Week | Completed |
| **8. Final Review** | Final system deployment, code cleanup, and project submission. | 3 Days | Completed |
