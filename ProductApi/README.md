# ProductApi

ProductApi is a RESTful Web API built with **ASP.NET Core (.NET 8)** and **Entity Framework Core**. It is designed to manage a product catalog, offering comprehensive Create, Read, Update, and Delete (CRUD) operations, along with advanced features like search, filtering, and pagination.

## Features

- **CRUD Operations**: Manage products (Create, Read, Update, Delete).
- **Relational Database**: Uses **PostgreSQL** as the database provider via `Npgsql.EntityFrameworkCore.PostgreSQL`.
- **Search & Filtering**: Search products by name and filter by price range (`minPrice` and `maxPrice`).
- **Pagination**: Retrieve product lists by specifying page number and page size.
- **Data Validation**: Model validation using Data Annotations (e.g., required names, positive prices, max length constraints).
- **Swagger/OpenAPI UI**: Interactive API documentation available in development mode for easy testing.

## Tech Stack

- **Framework**: .NET 8.0 SDK (ASP.NET Core Web API)
- **ORM**: Entity Framework Core 8.0.11
- **Database**: PostgreSQL
- **Packages**:
  - `Microsoft.EntityFrameworkCore`
  - `Microsoft.EntityFrameworkCore.Design`
  - `Microsoft.EntityFrameworkCore.Tools`
  - `Npgsql.EntityFrameworkCore.PostgreSQL`
  - `Swashbuckle.AspNetCore` (for Swagger UI)

## App Overview

### Models

- **`Product`**: Represents an item in the catalog.
  - `Id` (int) - Primary key.
  - `Name` (string) - Name of the product (Required, Max Length 100).
  - `Price` (decimal) - The cost of the product (Required, Minimum 0).

### Endpoints (ProductsController)

| Method   | Endpoint                  | Description                                         | Query Parameters                                                                           |
| :------- | :------------------------ | :-------------------------------------------------- | :----------------------------------------------------------------------------------------- |
| `GET`    | `/api/products`           | Retrieves a paginated list of products.             | `search` (string), `minPrice` (decimal), `maxPrice` (decimal), `page` (int), `pageSize` (int) |
| `GET`    | `/api/products/{id}`      | Retrieves a specific product by its ID.             |                                                                                            |
| `POST`   | `/api/products`           | Creates a new product.                              | Send a JSON payload with `name` and `price`.                                               |
| `PUT`    | `/api/products/{id}`      | Updates an existing product.                        | Send a JSON payload with updated `name` and `price`.                                       |
| `DELETE` | `/api/products/{id}`      | Deletes a product by its ID.                        |                                                                                            |

## Installation & Setup Guide

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) server installed and running.
- A .NET CLI or an IDE like Visual Studio / Visual Studio Code.
- EF Core CLI tools (optional but recommended: run `dotnet tool install --global dotnet-ef`).

### 1. Database Configuration (appsettings.json vs .env)

By default, the application is configured to connect to a local PostgreSQL instance.

**Where is the `.env` file?**
In ASP.NET Core, we typically use **`appsettings.json`** instead of `.env` files to store configurations and database connection strings.

Open `appsettings.json` and adjust the connection string to match your PostgreSQL setup:

```json
"ConnectionStrings": {
  "Default": "Host=localhost;Database=productdb;Username=postgres;Password=1234"
}
```
*Make sure the `Username` and `Password` match your local PostgreSQL server credentials.*

> **Security Note (Hiding Credentials):** 
> To prevent your password from being committed to version control (which is the main reason why people use `.env` files in other frameworks), ASP.NET Core provides a built-in feature called **User Secrets** for development.
> You can configure it via your terminal without needing a `.env` file:
> 1. `dotnet user-secrets init`
> 2. `dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Database=productdb;Username=postgres;Password=YOUR_PASSWORD"`
> 
> Alternatively, if you strongly prefer using an actual `.env` file, you can install a third-party package like `DotNetEnv` (`dotnet add package DotNetEnv`).

### 2. Apply Database Migrations

You need to create the database schema by applying EF Core migrations.

Open your terminal in the `ProductApi` directory and run:

```bash
# Optional: generate an initial migration if none exists yet
dotnet ef migrations add InitialCreate

# Appy the migrations to create the database tables
dotnet ef database update
```
*(If you do not have `dotnet-ef` installed, you can use the Package Manager Console in Visual Studio: `Update-Database`)*

### 3. Run the Application

Start the web API project using the .NET CLI:

```bash
dotnet run
```

### 4. Testing the API

Once the application is running, navigate to the Swagger UI in your browser (usually running on port 5000/5001 or standard HTTPS ports from launchSettings.json). The console output will show you the exact listening URLs.

- **Swagger UI URL:** `http://localhost:<YOUR_PORT>/swagger`
- **Or HTTPS:** `https://localhost:<YOUR_HTTPS_PORT>/swagger`

You can use the Swagger interface to interactively test all the implemented `GET`, `POST`, `PUT`, and `DELETE` endpoints.
