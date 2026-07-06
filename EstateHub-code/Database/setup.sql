-- EstateHub database setup script
--
-- Creates the EstateHub database and all tables required to run the app.
-- Safe to re-run: every statement uses IF NOT EXISTS, so it won't touch
-- tables/data that already exist.
--
-- Usage:
--   mysql -u root -p < Database/setup.sql
--
-- After running this, start the app once (dotnet run). Program.cs will
-- automatically seed demo data (properties, apartments, tenants, calendar
-- events, work orders, etc.) into the empty tables on first launch.

CREATE DATABASE IF NOT EXISTS EstateHub;
USE EstateHub;

CREATE TABLE IF NOT EXISTS property (
  property_id int NOT NULL AUTO_INCREMENT,
  address varchar(255) NOT NULL,
  city varchar(100) DEFAULT NULL,
  postal_code varchar(20) DEFAULT NULL,
  PRIMARY KEY (property_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS tenants (
  TenantID int NOT NULL AUTO_INCREMENT,
  Phone varchar(20) DEFAULT NULL,
  Email varchar(100) DEFAULT NULL,
  LastName varchar(255) DEFAULT NULL,
  FirstName varchar(255) DEFAULT NULL,
  PersonalNumber varchar(20) DEFAULT NULL,
  Address varchar(200) DEFAULT NULL,
  MobilePhone varchar(30) DEFAULT NULL,
  PRIMARY KEY (TenantID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS apartments (
  ApartmentID int NOT NULL AUTO_INCREMENT,
  PropertyID int DEFAULT NULL,
  ApartmentNumber varchar(10) DEFAULT NULL,
  Size decimal(5,2) DEFAULT NULL,
  Rent decimal(15,2) DEFAULT NULL,
  TenantID int DEFAULT NULL,
  Rooms int NOT NULL DEFAULT '0',
  Floor int NOT NULL DEFAULT '0',
  Status varchar(50) NOT NULL DEFAULT 'Ledig',
  AvailableFrom datetime DEFAULT NULL,
  ElectricityIncluded tinyint(1) NOT NULL DEFAULT '0',
  WaterIncluded tinyint(1) NOT NULL DEFAULT '0',
  InternetIncluded tinyint(1) NOT NULL DEFAULT '0',
  Balcony tinyint(1) NOT NULL DEFAULT '0',
  Furnished tinyint(1) NOT NULL DEFAULT '0',
  ImageUrl varchar(500) NOT NULL DEFAULT '',
  PRIMARY KEY (ApartmentID),
  KEY PropertyID (PropertyID),
  CONSTRAINT apartments_ibfk_1 FOREIGN KEY (PropertyID) REFERENCES property (property_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS contracts (
  ContractID int NOT NULL AUTO_INCREMENT,
  TenantID int DEFAULT NULL,
  ApartmentID int DEFAULT NULL,
  StartDate date NOT NULL,
  EndDate date DEFAULT NULL,
  PRIMARY KEY (ContractID),
  KEY TenantID (TenantID),
  KEY ApartmentID (ApartmentID),
  CONSTRAINT contracts_ibfk_1 FOREIGN KEY (TenantID) REFERENCES tenants (TenantID),
  CONSTRAINT contracts_ibfk_2 FOREIGN KEY (ApartmentID) REFERENCES apartments (ApartmentID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS payments (
  PaymentID int NOT NULL AUTO_INCREMENT,
  ContractID int DEFAULT NULL,
  Amount decimal(15,2) DEFAULT NULL,
  PaymentDate datetime DEFAULT NULL,
  PRIMARY KEY (PaymentID),
  KEY ContractID (ContractID),
  CONSTRAINT payments_ibfk_1 FOREIGN KEY (ContractID) REFERENCES contracts (ContractID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS app_settings (
  AppSettingId int NOT NULL AUTO_INCREMENT,
  DarkMode tinyint(1) NOT NULL DEFAULT '0',
  EmailNotifications tinyint(1) NOT NULL DEFAULT '1',
  AutoSaveReports tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (AppSettingId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS calendar_events (
  CalendarEventId int NOT NULL AUTO_INCREMENT,
  Title varchar(200) NOT NULL,
  Category varchar(100) NOT NULL,
  Location varchar(200) NOT NULL,
  StartsAt datetime NOT NULL,
  PRIMARY KEY (CalendarEventId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS customer_issues (
  CustomerIssueId int NOT NULL AUTO_INCREMENT,
  CustomerName varchar(200) NOT NULL,
  Issue varchar(200) NOT NULL,
  Status varchar(50) NOT NULL,
  Priority varchar(50) NOT NULL,
  PRIMARY KEY (CustomerIssueId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS work_orders (
  WorkOrderId int NOT NULL AUTO_INCREMENT,
  OrderNumber varchar(50) NOT NULL,
  Supplier varchar(100) NOT NULL,
  Product varchar(200) NOT NULL,
  Status varchar(50) NOT NULL,
  OrderDate datetime NOT NULL,
  ApartmentNumber varchar(50) DEFAULT NULL,
  PRIMARY KEY (WorkOrderId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS inspections (
  InspectionId int NOT NULL AUTO_INCREMENT,
  ApartmentNumber varchar(50) NOT NULL,
  InspectionDate datetime NOT NULL,
  Inspector varchar(200) NOT NULL,
  Status varchar(50) NOT NULL,
  Notes varchar(500) NOT NULL,
  PRIMARY KEY (InspectionId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS inspection_reports (
  InspectionReportId int NOT NULL AUTO_INCREMENT,
  InspectionId int NOT NULL,
  PropertyAddress varchar(300) NOT NULL DEFAULT '',
  InspectionType varchar(50) NOT NULL DEFAULT 'Move-in',
  InspectionDate datetime NOT NULL,
  InspectorName varchar(200) NOT NULL DEFAULT '',
  PresentDuringInspection varchar(300) NOT NULL DEFAULT '',
  WeatherConditions varchar(200) NOT NULL DEFAULT '',
  OverallCondition varchar(50) NOT NULL DEFAULT 'Good',
  Summary text,
  RoomItemsJson longtext,
  UtilitiesJson longtext,
  MeterReadingsJson longtext,
  IncludedItemsJson longtext,
  IssuesJson longtext,
  PhotosJson longtext,
  TenantSignatureName varchar(200) NOT NULL DEFAULT '',
  TenantSignatureDate varchar(50) NOT NULL DEFAULT '',
  LandlordSignatureName varchar(200) NOT NULL DEFAULT '',
  LandlordSignatureDate varchar(50) NOT NULL DEFAULT '',
  PRIMARY KEY (InspectionReportId),
  UNIQUE KEY unique_inspection (InspectionId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS accounting_records (
  AccountingRecordId int NOT NULL AUTO_INCREMENT,
  Year int NOT NULL,
  Month int NOT NULL,
  Revenue decimal(12,2) NOT NULL DEFAULT '0.00',
  Expenses decimal(12,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (AccountingRecordId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS admin_profile (
  AdminProfileId int NOT NULL AUTO_INCREMENT,
  FirstName varchar(100) NOT NULL DEFAULT '',
  LastName varchar(100) NOT NULL DEFAULT '',
  Email varchar(200) NOT NULL DEFAULT '',
  Phone varchar(50) NOT NULL DEFAULT '',
  Role varchar(100) NOT NULL DEFAULT 'Administrator',
  Responsibilities varchar(500) NOT NULL DEFAULT '',
  PasswordHash varchar(200) NOT NULL DEFAULT '',
  ImageUrl varchar(500) NOT NULL DEFAULT '',
  PRIMARY KEY (AdminProfileId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS support_messages (
  SupportMessageId int NOT NULL AUTO_INCREMENT,
  FullName varchar(200) NOT NULL DEFAULT '',
  Email varchar(200) NOT NULL DEFAULT '',
  Phone varchar(50) NOT NULL DEFAULT '',
  Message text,
  SubmittedAt datetime NOT NULL,
  PRIMARY KEY (SupportMessageId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
