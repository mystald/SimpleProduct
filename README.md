# SimpleProduct

A full-stack product management application built with **ASP.NET Core (.NET 10)** backend and **React (Vite + TypeScript)** frontend.

---

## Prerequisites

Make sure the following tools are installed on your machine:

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- [Node.js (v18+) & npm](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/sql-server) or LocalDB (`(localdb)\mssqllocaldb`)

---

## Getting Started

### 1. Clone the repository
```bash
git clone <repository-url>
cd SimpleProduct
```

### 2. Frontend Dependencies Installation
Navigate to the frontend folder and install npm packages:
```bash
cd simpleproduct.client
npm install
cd ..
```

### 3. Database Configuration
By default, the connection string in `SimpleProduct.Server/appsettings.json` points to SQL Server LocalDB:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SimpleProductDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```
If using a custom SQL Server instance, update this connection string accordingly.

> [!NOTE]
> Database creation and initial seeding happen automatically on the first application startup.

---

## How to Run Locally

### Option A: Run Both Backend & Frontend Together (Recommended)

You can launch the backend and frontend simultaneously via .NET's SPA proxy:

```powershell
cd SimpleProduct.Server
dotnet run --launch-profile https
```

- **Frontend (Vite UI):** [https://localhost:51641](https://localhost:51641)
- **Backend API (HTTPS):** [https://localhost:7182](https://localhost:7182)
- **Swagger Documentation:** [https://localhost:7182/swagger](https://localhost:7182/swagger)

> [!TIP]
> The frontend proxy dynamically connects to the backend port via the `ASPNETCORE_URLS` / `ASPNETCORE_HTTPS_PORT` environment variables passed by .NET.

---

### Option B: Run Backend and Frontend Separately

If you prefer running the services in separate terminals:

#### Terminal 1 — Backend API
```powershell
cd SimpleProduct.Server
dotnet run --launch-profile backend
```
- API starts at: `http://localhost:5213`
- Swagger available at: [http://localhost:5213/swagger](http://localhost:5213/swagger)

#### Terminal 2 — Frontend Client
```powershell
cd simpleproduct.client
npm run dev
```
- Frontend UI starts at: [https://localhost:51641](https://localhost:51641)

---

## Default Seeded Credentials

When starting up for the first time, the application automatically seeds an administrator account:

| Role | Email | Password |
|---|---|---|
| **Admin** | `admin@simpleproduct.com` | `Admin@123` |