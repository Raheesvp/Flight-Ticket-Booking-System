<div align="center">

<img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/ASP.NET_MVC-blue?style=for-the-badge&logo=microsoft&logoColor=white"/>
<img src="https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white"/>
<img src="https://img.shields.io/badge/Bootstrap_5-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white"/>
<img src="https://img.shields.io/badge/Razorpay-02042B?style=for-the-badge&logo=razorpay&logoColor=white"/>

<br/><br/>

# ✈️ FlightBookingSystem

### A full-stack flight ticket booking web application built with ASP.NET MVC, C#, and SQL Server — inspired by real-world travel platforms like Alhind.com

<br/>

[🚀 Live Demo](#) &nbsp;·&nbsp; [📋 Project Board](#) &nbsp;·&nbsp; [🐛 Report Bug](#) &nbsp;·&nbsp; [💡 Request Feature](#)

</div>

---

## 📸 Screenshots

| Home — Flight Search | Search Results | Booking Flow |
|---|---|---|
| ![Home](https://via.placeholder.com/280x160/003580/ffffff?text=Search+Form) | ![Results](https://via.placeholder.com/280x160/0066cc/ffffff?text=Flight+Results) | ![Booking](https://via.placeholder.com/280x160/ff6600/ffffff?text=Booking+Flow) |

| Admin Dashboard | E-Ticket PDF | My Bookings |
|---|---|---|
| ![Admin](https://via.placeholder.com/280x160/1a1a2e/ffffff?text=Admin+Panel) | ![Ticket](https://via.placeholder.com/280x160/003580/ffffff?text=E-Ticket+PDF) | ![MyBookings](https://via.placeholder.com/280x160/0066cc/ffffff?text=My+Bookings) |

> 📌 Replace placeholder images with real screenshots after deployment.

---

## 🧩 Features

### 🔍 Flight Search
- One-way, return, and multi-city search
- Filter by price range, airlines, stops, departure time
- 7-day price calendar with lowest fare highlight
- Real-time seat availability display

### 🎫 Booking Flow
- Multi-passenger details form (Adult / Child / Infant)
- Interactive seat map (available / occupied / selected)
- Add-ons: extra baggage, meals, travel insurance
- Dynamic price recalculation via Ajax at every step

### 💳 Payment & Confirmation
- Razorpay payment gateway integration
- Server-side webhook verification
- PNR generation (6-char alphanumeric)
- Boarding-pass-style **PDF e-ticket** with QR code (iTextSharp)
- Automated confirmation email with PDF attachment (SendGrid)

### 👤 Authentication
- ASP.NET Identity with custom `ApplicationUser`
- Role-based authorization — **Admin**, **User**
- Login, Register, Forgot Password flows
- Account lockout after 5 failed attempts

### 🛠️ Admin Panel
- Dashboard with live KPIs (bookings, revenue, occupancy)
- Chart.js revenue trend charts
- Flight schedule & fare management
- Promo code engine (flat / percentage discount)
- Passenger manifest + **Excel export** (ClosedXML)
- Booking management with refund processing

---

## 🏗️ Architecture

```
FlightBookingSystem/
├── FlightBooking.Web/
│   ├── Controllers/          # MVC controllers
│   ├── Models/
│   │   ├── Domain/           # EF entity classes
│   │   └── ViewModels/       # Form + display models
│   ├── Data/
│   │   ├── AppDbContext.cs   # IdentityDbContext<ApplicationUser>
│   │   ├── Repositories/     # Generic Repository pattern
│   │   ├── UnitOfWork.cs     # Unit of Work
│   │   ├── DbSeeder.cs       # Airport + airline seed data
│   │   └── RoleSeeder.cs     # Role + admin user seed
│   ├── Services/             # Business logic layer
│   ├── Views/                # Razor views (.cshtml)
│   └── wwwroot/              # Static assets
```

### Design patterns used
- **Repository + Unit of Work** — clean data access layer, fully testable
- **MVC** — clear separation between UI, logic, and data
- **Service layer** — booking logic, fare calculation, PNR generation
- **Dependency injection** — all services registered via `Program.cs`

---

## 🗃️ Database Schema

```
Airports ──┐
           ├──→ Flights ──→ Bookings ──→ Passengers
Airlines ──┘         │             └──→ Payments
                     └──→ Seats

AspNetUsers ──→ Bookings
AspNetRoles ──→ AspNetUserRoles ──→ AspNetUsers
```

**8 domain tables** + 7 ASP.NET Identity tables = **15 tables total**

---

## ⚙️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC — .NET 8 |
| Language | C# |
| ORM | Entity Framework Core (Code First) |
| Database | SQL Server (LocalDB / Azure SQL) |
| Auth | ASP.NET Identity + Cookie auth |
| UI | Bootstrap 5 + Bootstrap Icons + jQuery |
| Payment | Razorpay gateway |
| PDF | iTextSharp |
| Email | SendGrid |
| Excel | ClosedXML |
| Charts | Chart.js |
| Testing | xUnit |
| CI/CD | GitHub Actions |
| Hosting | Azure App Service + Azure SQL |

---

## 🚀 Getting Started

### Prerequisites
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (or VS Code)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-in/sql-server/sql-server-downloads) (or LocalDB)
- [Git](https://git-scm.com/)

### 1. Clone the repo

```bash
git clone https://github.com/Raheesvp/FlightBookingSystem.git
cd FlightBookingSystem
```

### 2. Configure the connection string

Open `appsettings.json` and update:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=FlightBookingDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Apply EF migrations

Open **Package Manager Console** in Visual Studio:

```powershell
Update-Database
```

This creates `FlightBookingDB` and seeds airports, airlines, roles, and a default admin user automatically.

### 4. Run the project

Press **F5** in Visual Studio or:

```bash
dotnet run --project FlightBooking.Web
```

Navigate to `https://localhost:PORT`

### 5. Default admin login

```
Email:    admin@flightbooking.com
Password: Admin@1234
```

> ⚠️ Change this password before deploying to production.

---

## 🌍 Environment Variables (Production)

Set these in Azure App Service → Configuration:

```
ConnectionStrings__DefaultConnection   → your Azure SQL connection string
Razorpay__KeyId                        → your Razorpay key ID
Razorpay__KeySecret                    → your Razorpay key secret
SendGrid__ApiKey                       → your SendGrid API key
```

---

## 📅 Build Journey — 35-Day Roadmap

| Phase | Days | What was built |
|---|---|---|
| 🔧 **Phase 1** — Foundation | 1–4 | Project setup, SQL Server schema (15 tables), EF Code First, Repository + Unit of Work, Bootstrap 5 layout |
| 🔐 **Phase 2** — Auth | 5–8 | ASP.NET Identity, Login, Register, role-based authorization, profile management |
| ✈️ **Phase 3** — Search | 9–15 | Flight search engine (one-way / return / multi-city), seat map, price calendar, filters |
| 🎫 **Phase 4** — Booking | 16–23 | Booking funnel, Razorpay, PNR generation, PDF e-ticket, SendGrid email |
| 🛠️ **Phase 5** — Admin | 24–29 | Admin dashboard, fare management, promo codes, Excel reports, refund UI |
| 🚢 **Phase 6** — Deploy | 30–35 | Mobile responsive, xUnit tests, security hardening, Azure deployment, GitHub Actions CI/CD |

---

## 🧪 Running Tests

```bash
dotnet test
```

Tests cover: booking logic, PNR generation, fare calculation, search filtering, and the checkout flow.

---

## 🔒 Security

- CSRF tokens on all POST forms (`[ValidateAntiForgeryToken]`)
- XSS prevention — all user inputs HTML-encoded
- SQL injection prevention — parameterized queries via EF Core
- Open redirect prevention — `Url.IsLocalUrl()` on all redirects
- Login rate-limiting — 5 attempts → 10-minute lockout
- HTTPS enforced via `UseHsts()` in production

---

## 📁 Key Files

| File | Purpose |
|---|---|
| `Data/AppDbContext.cs` | EF context — extends `IdentityDbContext<ApplicationUser>` |
| `Data/UnitOfWork.cs` | Single save transaction across all repositories |
| `Data/RoleSeeder.cs` | Seeds Admin/User roles + default admin account |
| `Controllers/AccountController.cs` | Login, Register, Logout with Identity |
| `Controllers/BookingController.cs` | Multi-step booking funnel |
| `Controllers/AdminController.cs` | Admin panel — flights, fares, bookings, reports |
| `Program.cs` | Service registration, middleware pipeline, seeder calls |

---

## 🤝 Contributing

Pull requests are welcome. For major changes please open an issue first to discuss what you'd like to change.

1. Fork the repo
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit: `git commit -m "Add your feature"`
4. Push: `git push origin feature/your-feature`
5. Open a pull request

---

## 👨‍💻 Author

**Mohammed Rahees VP**
Full Stack .NET Developer — Malappuram, Kerala

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0A66C2?style=flat&logo=linkedin&logoColor=white)](https://linkedin.com/in/rahees-vp)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=flat&logo=github&logoColor=white)](https://github.com/Raheesvp)
[![Portfolio](https://img.shields.io/badge/Portfolio-FF6B35?style=flat&logo=netlify&logoColor=white)](#)

---

## 📄 License

This project is licensed under the MIT License — see [LICENSE](LICENSE) for details.

---

<div align="center">

⭐ If this project helped you, please give it a star!

Made with ❤️ using ASP.NET MVC + C# + SQL Server

</div>
