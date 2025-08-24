# Netby
Repositorio para almacenar el ejercicio pureba de netby
#Autor
Santiago Morales

# Base de Datos
1. Utilizar Sql Server en su ultima version Express y Sql Server Manager 2021
#Script de generacion de base de datos
IF DB_ID('InventoryDb') IS NULL
  CREATE DATABASE InventoryDb;
GO
USE InventoryDb;
GO
IF SCHEMA_ID('inventory') IS NULL
  EXEC('CREATE SCHEMA inventory');
GO
-- Tabla: Productos
IF OBJECT_ID('inventory.Products') IS NULL
BEGIN
  CREATE TABLE inventory.Products (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Category NVARCHAR(100) NOT NULL,
    ImageUrl NVARCHAR(500) NULL,
    Price DECIMAL(18,2) NOT NULL DEFAULT(0),
    Stock INT NOT NULL DEFAULT(0),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL
  );
  CREATE UNIQUE INDEX IX_Products_Name ON inventory.Products(Name);
END
GO

-- Tabla: Transacciones
IF OBJECT_ID('inventory.InventoryTransactions') IS NULL
BEGIN
  CREATE TABLE inventory.InventoryTransactions (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    [Date] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [Type] INT NOT NULL, -- 1=Compra, 2=Venta
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Total AS (Quantity * UnitPrice) PERSISTED,
    Detail NVARCHAR(500) NULL,
    CONSTRAINT FK_InventoryTransactions_Products
      FOREIGN KEY(ProductId) REFERENCES inventory.Products(Id)
  );
  CREATE INDEX IX_InventoryTransactions_ProductId ON inventory.InventoryTransactions(ProductId);
END
GO

# Seccion BackEnd
Requisitos
Backend
1. .NET SDK 8.0 (o LTS equivalente).
2.SQL Server (Express o superior). Ejemplo local: SANTIAGO-PC\SQLEXPRESS.
3. Permisos para crear la base InventoryDb y el esquema inventory.

# Seccion Frontend
1.Node.js 20.x (o ≥ 18.19).
2.npm actualizado.
3.Angular CLI 18 (npm i -g @angular/cli@18).
4.Herramientas opcionales: Postman/curl para pruebas.
#Puertos por defecto
Products.Api → http://localhost:5293
Transactions.Api → http://localhost:5085
Frontend → http://localhost:4200

# Confuguracion y Ejecucion del backend (confugurar server y password para la conexion, en mi caso el password es "netby" y el usuario es sa por default para Sql server)
Product service
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SANTIAGO-PC\\SQLEXPRESS;Database=InventoryDb;User Id=sa;Password=TU_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}

Transaction service
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SANTIAGO-PC\\SQLEXPRESS;Database=InventoryDb;User Id=sa;Password=TU_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
  },
  "Services": {
    "ProductsBaseUrl": "http://localhost:5293"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}

 # Habilitar CORS
 Dentro de  los archivos Program.cs de cada servicio
 
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p =>
        p.WithOrigins("http://localhost:4200")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors();

#Capturas de pantalla de Product service


