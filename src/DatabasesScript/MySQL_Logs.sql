/* Logging queries */
SET GLOBAL log_output = 'TABLE';
SET GLOBAL general_log = 'OFF';
TRUNCATE table mysql.general_log;
select * from mysql.general_log;

select ARGUMENT from mysql.general_log;  

/* Counting objects */
SELECT SUM(TABLE_ROWS) 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dataClonerTestDatabase';  