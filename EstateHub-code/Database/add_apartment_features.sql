ALTER TABLE apartments
    ADD COLUMN Rooms int NOT NULL DEFAULT 0,
    ADD COLUMN Floor int NOT NULL DEFAULT 0,
    ADD COLUMN Status varchar(50) NOT NULL DEFAULT 'Ledig',
    ADD COLUMN AvailableFrom datetime NULL,
    ADD COLUMN ElectricityIncluded tinyint(1) NOT NULL DEFAULT 0,
    ADD COLUMN WaterIncluded tinyint(1) NOT NULL DEFAULT 0,
    ADD COLUMN InternetIncluded tinyint(1) NOT NULL DEFAULT 0,
    ADD COLUMN Balcony tinyint(1) NOT NULL DEFAULT 0,
    ADD COLUMN Furnished tinyint(1) NOT NULL DEFAULT 0;

ALTER TABLE tenants
    ADD COLUMN PersonalNumber varchar(20) NULL,
    ADD COLUMN Address varchar(200) NULL,
    ADD COLUMN MobilePhone varchar(30) NULL;
