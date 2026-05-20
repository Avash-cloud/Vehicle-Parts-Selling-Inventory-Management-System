# System Diagrams - Mermaid Code

You can copy and paste these codes into the [Mermaid Live Editor](https://mermaid.live/) to generate images (PNG/SVG) which you can then download and insert into your Word document.

---

## 1.1 ER Diagram (Entity Relationship Diagram)

```mermaid
erDiagram
    CUSTOMER {
        int CustomerID PK
        string FirstName
        string LastName
        string Email
        string Phone
        string Address
    }
    
    VEHICLE {
        int VehicleID PK
        int CustomerID FK
        string Make
        string Model
        int Year
        string LicensePlate
    }
    
    STAFF {
        int StaffID PK
        string FullName
        string Role
        string Email
        string Phone
    }
    
    VENDOR {
        int VendorID PK
        string CompanyName
        string ContactPerson
        string Email
        string Phone
    }
    
    INVENTORY {
        int PartID PK
        int VendorID FK
        string PartName
        string Category
        decimal Price
        int StockQuantity
    }
    
    SALES_INVOICE {
        int InvoiceID PK
        int CustomerID FK
        int StaffID FK
        date InvoiceDate
        decimal TotalAmount
        string Status
    }
    
    PURCHASE_INVOICE {
        int PurchaseID PK
        int VendorID FK
        int StaffID FK
        date PurchaseDate
        decimal TotalAmount
    }
    
    APPOINTMENT {
        int AppointmentID PK
        int CustomerID FK
        int VehicleID FK
        date AppointmentDate
        string ServiceType
        string Status
    }
    
    REVIEW {
        int ReviewID PK
        int CustomerID FK
        int PartID FK
        int Rating
        string Comment
        date ReviewDate
    }

    CUSTOMER ||--o{ VEHICLE : "owns"
    CUSTOMER ||--o{ SALES_INVOICE : "receives"
    CUSTOMER ||--o{ APPOINTMENT : "books"
    CUSTOMER ||--o{ REVIEW : "writes"
    
    STAFF ||--o{ SALES_INVOICE : "generates"
    STAFF ||--o{ PURCHASE_INVOICE : "creates"
    
    VENDOR ||--o{ INVENTORY : "supplies"
    VENDOR ||--o{ PURCHASE_INVOICE : "receives"
    
    INVENTORY ||--o{ REVIEW : "receives"
    INVENTORY }|--o{ SALES_INVOICE : "included in"
    
    VEHICLE ||--o{ APPOINTMENT : "requires"
```

---

## 1.2 Use Case Diagram

```mermaid
usecaseDiagram
    actor Admin
    actor Staff
    actor Customer

    package "Vehicle Parts & Inventory System" {
        usecase "Staff Management" as UC1
        usecase "Report Generation" as UC2
        usecase "Vendor Management" as UC3
        usecase "Inventory Management" as UC4
        usecase "Invoice Generation" as UC5
        usecase "Customer Search" as UC6
        usecase "Parts Selling" as UC7
        usecase "Customer Registration" as UC8
        usecase "Appointment Booking" as UC9
        usecase "Review Submission" as UC10
    }

    Admin --> UC1
    Admin --> UC2
    Admin --> UC3
    Admin --> UC4
    Admin --> UC5
    Admin --> UC6

    Staff --> UC3
    Staff --> UC4
    Staff --> UC5
    Staff --> UC6
    Staff --> UC7

    Customer --> UC7
    Customer --> UC8
    Customer --> UC9
    Customer --> UC10
```
*(Note: If your mermaid viewer doesn't support the experimental `usecaseDiagram`, you can use this alternative standard graph format which works everywhere):*
```mermaid
graph LR
    subgraph Users
        A[Admin]
        S[Staff]
        C[Customer]
    end

    subgraph System Features
        UC1(Staff Management)
        UC2(Report Generation)
        UC3(Vendor Management)
        UC4(Inventory Management)
        UC5(Invoice Generation)
        UC6(Customer Search)
        UC7(Parts Selling)
        UC8(Customer Registration)
        UC9(Appointment Booking)
        UC10(Review Submission)
    end

    A --> UC1
    A --> UC2
    A --> UC3
    A --> UC4
    A --> UC5
    A --> UC6

    S --> UC3
    S --> UC4
    S --> UC5
    S --> UC6
    S --> UC7

    C --> UC7
    C --> UC8
    C --> UC9
    C --> UC10
```

---

## 1.3 Activity Diagram (Vehicle Parts Selling & Invoice Process)

```mermaid
stateDiagram-v2
    [*] --> Login
    Login --> SearchParts : Authentication Success
    SearchParts --> AddToCart : Part Available
    SearchParts --> UpdateInventory : Part Out of Stock (Staff action)
    
    AddToCart --> Checkout
    Checkout --> ProcessPayment
    
    state ProcessPayment {
        [*] --> VerifyDetails
        VerifyDetails --> PaymentGateway
        PaymentGateway --> Success
        PaymentGateway --> Failed
    }
    
    ProcessPayment --> GenerateInvoice : Success
    ProcessPayment --> Checkout : Failed (Retry)
    
    GenerateInvoice --> DeductInventory
    DeductInventory --> ProvideReceipt
    ProvideReceipt --> [*]
```

---

## 1.4 Class Diagram

```mermaid
classDiagram
    class Customer {
        +int CustomerID
        +string Name
        +string Email
        +string Phone
        +Register()
        +BookAppointment()
        +BuyPart()
        +SubmitReview()
    }

    class Vehicle {
        +int VehicleID
        +string Make
        +string Model
        +string LicensePlate
        +GetVehicleDetails()
    }

    class Staff {
        +int StaffID
        +string Name
        +string Role
        +ManageInventory()
        +GenerateInvoice()
    }

    class Vendor {
        +int VendorID
        +string CompanyName
        +string ContactInfo
        +SupplyParts()
    }

    class Inventory {
        +int PartID
        +string PartName
        +decimal Price
        +int StockQuantity
        +UpdateStock()
        +CheckAvailability()
    }

    class Invoice {
        +int InvoiceID
        +date Date
        +decimal TotalAmount
        +string Status
        +PrintInvoice()
        +CalculateTotal()
    }

    class Appointment {
        +int AppointmentID
        +date Date
        +string ServiceType
        +string Status
        +Schedule()
        +Cancel()
    }

    class Review {
        +int ReviewID
        +int Rating
        +string Comment
        +date Date
        +AddReview()
    }

    Customer "1" -- "*" Vehicle : owns
    Customer "1" -- "*" Appointment : books
    Customer "1" -- "*" Invoice : receives
    Customer "1" -- "*" Review : writes
    
    Staff "1" -- "*" Invoice : generates
    Vendor "1" -- "*" Inventory : supplies
    Inventory "1" -- "*" Review : has
    Appointment "*" -- "1" Vehicle : for
```
