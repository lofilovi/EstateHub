CREATE TABLE IF NOT EXISTS calendar_events (
    CalendarEventId int NOT NULL AUTO_INCREMENT,
    Title varchar(200) NOT NULL,
    Category varchar(100) NOT NULL,
    Location varchar(200) NOT NULL,
    StartsAt datetime NOT NULL,
    PRIMARY KEY (CalendarEventId)
);

CREATE TABLE IF NOT EXISTS customer_issues (
    CustomerIssueId int NOT NULL AUTO_INCREMENT,
    CustomerName varchar(200) NOT NULL,
    Issue varchar(200) NOT NULL,
    Status varchar(50) NOT NULL,
    Priority varchar(50) NOT NULL,
    PRIMARY KEY (CustomerIssueId)
);

CREATE TABLE IF NOT EXISTS work_orders (
    WorkOrderId int NOT NULL AUTO_INCREMENT,
    OrderNumber varchar(50) NOT NULL,
    Supplier varchar(100) NOT NULL,
    Product varchar(200) NOT NULL,
    Status varchar(50) NOT NULL,
    OrderDate datetime NOT NULL,
    PRIMARY KEY (WorkOrderId)
);

CREATE TABLE IF NOT EXISTS inspections (
    InspectionId int NOT NULL AUTO_INCREMENT,
    ApartmentNumber varchar(50) NOT NULL,
    InspectionDate datetime NOT NULL,
    Inspector varchar(200) NOT NULL,
    Status varchar(50) NOT NULL,
    Notes varchar(500) NOT NULL,
    PRIMARY KEY (InspectionId)
);

CREATE TABLE IF NOT EXISTS app_settings (
    AppSettingId int NOT NULL AUTO_INCREMENT,
    DarkMode tinyint(1) NOT NULL DEFAULT 0,
    EmailNotifications tinyint(1) NOT NULL DEFAULT 1,
    AutoSaveReports tinyint(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (AppSettingId)
);
