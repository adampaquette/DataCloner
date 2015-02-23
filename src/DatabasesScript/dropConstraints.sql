USE `DataclonerTestDatabase`;

SET foreign_key_checks = 0;

ALTER TABLE `productlines`
	DROP PRIMARY KEY;

ALTER TABLE `offices`
	DROP PRIMARY KEY;

ALTER TABLE `payments`
	DROP PRIMARY KEY,
	DROP FOREIGN KEY `payments_ibfk_1`;
	
ALTER TABLE `products`
	DROP PRIMARY KEY,
	DROP INDEX `productLine`,
	DROP FOREIGN KEY `products_ibfk_1`;
	
ALTER TABLE `orderdetails`
	DROP PRIMARY KEY,
	DROP INDEX `productCode`,
	DROP FOREIGN KEY `orderdetails_ibfk_1`,
	DROP FOREIGN KEY `orderdetails_ibfk_2`;
	
ALTER TABLE `orders`
	DROP PRIMARY KEY,
	DROP INDEX `customerNumber`,
	DROP FOREIGN KEY `orders_ibfk_1`;

ALTER TABLE `employees`
	DROP PRIMARY KEY,
	DROP INDEX `reportsTo`,
	DROP INDEX `officeCode`,
	DROP FOREIGN KEY `employees_ibfk_1`,
	DROP FOREIGN KEY `employees_ibfk_2`;
	
ALTER TABLE `customers`
	DROP PRIMARY KEY,
	DROP INDEX `salesRepEmployeeNumber`,
	DROP FOREIGN KEY `customers_ibfk_1`;	