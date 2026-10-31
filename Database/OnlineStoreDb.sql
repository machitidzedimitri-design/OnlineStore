/* OnlineStoreDb - alternative to EF Core migrations (use ONE of the two, not both). */
IF DB_ID(N'OnlineStoreDb') IS NULL CREATE DATABASE OnlineStoreDb;
GO
USE OnlineStoreDb;
GO

CREATE TABLE dbo.Categories (
    Id          INT IDENTITY(1,1) CONSTRAINT PK_Categories PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Description NVARCHAR(300) NOT NULL
);

CREATE TABLE dbo.Products (
    Id            INT IDENTITY(1,1) CONSTRAINT PK_Products PRIMARY KEY,
    Name          NVARCHAR(100)  NOT NULL,
    Description   NVARCHAR(500)  NOT NULL,
    Price         DECIMAL(18,2)  NOT NULL,
    StockQuantity INT            NOT NULL,
    CategoryId    INT            NOT NULL,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id),
    CONSTRAINT CK_Products_Price CHECK (Price > 0),
    CONSTRAINT CK_Products_Stock CHECK (StockQuantity >= 0)
);

CREATE TABLE dbo.Customers (
    Id        INT IDENTITY(1,1) CONSTRAINT PK_Customers PRIMARY KEY,
    FirstName NVARCHAR(50)  NOT NULL,
    LastName  NVARCHAR(50)  NOT NULL,
    Email     NVARCHAR(150) NOT NULL,
    Phone     NVARCHAR(30)  NULL
);
CREATE UNIQUE INDEX IX_Customers_Email ON dbo.Customers(Email);

CREATE TABLE dbo.Orders (
    Id          INT IDENTITY(1,1) CONSTRAINT PK_Orders PRIMARY KEY,
    CustomerId  INT           NOT NULL,
    OrderDate   DATETIME2     NOT NULL,
    Status      NVARCHAR(30)  NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id)
);

CREATE TABLE dbo.OrderItems (
    Id        INT IDENTITY(1,1) CONSTRAINT PK_OrderItems PRIMARY KEY,
    OrderId   INT           NOT NULL,
    ProductId INT           NOT NULL,
    Quantity  INT           NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders   FOREIGN KEY (OrderId)   REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0)
);
GO

INSERT INTO dbo.Categories (Name, Description) VALUES
 (N'Computers',   N'Laptops, desktops and monitors'),
 (N'Phones',      N'Smartphones and mobile devices'),
 (N'Accessories', N'Headphones, cables, chargers and more');
GO

/* View: order lines with customer, product and line totals */
CREATE VIEW dbo.vw_OrderDetails AS
SELECT o.Id AS OrderId, o.OrderDate, o.Status, o.TotalAmount,
       c.FirstName + N' ' + c.LastName AS CustomerName, c.Email,
       p.Name AS ProductName, oi.Quantity, oi.UnitPrice,
       oi.Quantity * oi.UnitPrice AS LineTotal
FROM dbo.Orders o
JOIN dbo.Customers  c  ON c.Id = o.CustomerId
JOIN dbo.OrderItems oi ON oi.OrderId = o.Id
JOIN dbo.Products   p  ON p.Id = oi.ProductId;
GO

/* Stored procedure: all orders of one customer with item counts */
CREATE PROCEDURE dbo.sp_GetCustomerOrders @CustomerId INT AS
BEGIN
    SET NOCOUNT ON;
    SELECT o.Id, o.OrderDate, o.Status, o.TotalAmount, COUNT(oi.Id) AS ItemCount
    FROM dbo.Orders o
    LEFT JOIN dbo.OrderItems oi ON oi.OrderId = o.Id
    WHERE o.CustomerId = @CustomerId
    GROUP BY o.Id, o.OrderDate, o.Status, o.TotalAmount
    ORDER BY o.OrderDate DESC;
END
GO
