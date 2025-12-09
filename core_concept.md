# **Service Provider Management System — Core Concept**

## **Overview**

The Service Provider Management System (SPMS) is an internal line-of-business application designed to support organisations that deliver social, health, or community services. It provides a unified platform for managing staff, service users, programmes, training compliance, documents, and operational tasks.

This project serves as a practical demonstration of modern .NET development using **Blazor**, **ASP.NET Core**, **Entity Framework Core**, **SQL Server**, and **Azure App Services**.
It also showcases secure web application practices, role-based access, business logic, dashboards, and cloud-ready monitoring.

---

## **Goals of the System**

* Provide a clean, modern, secure internal tool for operational staff.
* Demonstrate data-driven app design with relational modelling.
* Support daily organisational workflows such as tracking staff, services, compliance, and tasks.
* Offer dashboards and reporting capabilities for management and oversight.
* Serve as an end-to-end demonstration of a full .NET application lifecycle, including testing, logging, and cloud deployment.

This system is intentionally scoped to be realistic but manageable, enabling incremental extension.

---

## **Primary Actors**

### **Staff**

Employees who use the system to manage services, record training, upload documents, and coordinate tasks.

### **Service Users**

Individuals who receive support or services from the organisation.

### **Managers / Admins**

Users with elevated permissions who configure departments, programmes, and system-wide settings.

---

## **Key Domains & Entities**

### **1. Staff**

Represents employees or support workers.

**Attributes include:**

* Name, contact info
* Department
* Role
* Employment status
* Training compliance summary

**Core operations:**

* CRUD
* Assign to department
* View tasks and training status

---

### **2. Service Users**

Individuals supported by the organisation.

**Attributes include:**

* Name
* Date of birth
* Support worker
* Programme affiliation
* Notes / documents

**Core operations:**

* CRUD
* Assign support worker
* Link to programmes
* Attach documents

---

### **3. Departments**

Organisational units grouping staff and operations.

**Core operations:**

* CRUD
* Assign staff
* Department-level reporting

---

### **4. Programmes / Services**

Service areas such as Day Services, Residential Support, or Community Outreach.

**Core operations:**

* CRUD
* Assign service users
* Overview of programme activity

---

### **5. Training & Compliance**

Tracks mandatory training requirements for staff, including recurrence rules.

**Key entities:**

* `TrainingCourse`
* `TrainingRecord`

**Core operations:**

* Record completions
* Auto-calculate expiry
* Compliance dashboards
* Optional background service for periodic evaluation

This domain introduces meaningful business logic.

---

### **6. Documents**

Supports uploading and managing files linked to staff or service users.

**Features:**

* Metadata storage
* Tagging
* Search
* Secure access control
* Integration with Azure storage (optional)

---

### **7. Tasks / Requests**

Lightweight workflow to support operational coordination.

**Core operations:**

* Create & assign tasks
* Track status (Open, In Progress, Closed)
* Add comments & history
* “My Tasks” dashboard

---

### **8. Analytics & Dashboards**

System provides high-level operational insights.

**Example widgets:**

* Staff count
* Service user count
* Training compliance status
* Overdue training
* Open tasks
* Programme statistics

Charts, tables, and filters enable quick understanding of organisational performance.

---

## **Cross-Cutting Concerns**

### **Authentication & Authorization**

* Role-based access (Admin, Manager, Staff)
* Integration with ASP.NET Core Identity or Azure AD

### **Logging & Monitoring**

* Structured logs (Serilog)
* Application Insights integration
* Request-tracing and error diagnostics

### **Audit Trail**

* Optional table for recording user actions
* Supports accountability and compliance

### **Cloud Deployment**

* Designed for hosting on Azure App Services
* Supports Azure SQL and managed identities
* Pipelines for CI/CD (GitHub Actions or Azure DevOps)

---

## **Non-Goals (for clarity)**

* No financial transactions
* No medical or clinical recordkeeping
* No public-facing functionality
* Not a replacement for full Case Management Systems

The purpose is demonstration, not production-readiness.

---

## **Intended Future Extensions**

These areas provide optional complexity for later iterations:

* Full-text search (Lucene.NET or Azure Cognitive Search)
* PWA/Offline support (Blazor WebAssembly)
* Mobile-friendly workflows
* Real-time notifications (SignalR)
* AI-assisted document tagging or summarisation

---

## **Summary**

The Service Provider Management System is a modular, extensible reference project representing the type of internal operational system frequently used in health and social care organisations. It showcases the full stack of modern .NET skills — front end, back end, cloud, logging, testing, security, and relational data — while remaining small enough to build iteratively.

---

If you'd like:
📌 I can generate *architecture diagrams*, *entity diagrams*, or a *README.md* for the repo.
