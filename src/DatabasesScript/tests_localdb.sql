--http://blogs.msdn.com/b/sqlexpress/archive/2011/12/09/using-localdb-with-full-iis-part-2-instance-ownership.aspx

CREATE DATABASE NORTHWIND
ON (FILENAME = 'C:\Programmation\C#\datacloner\src\DatabasesScript\NORTHWIND.mdf')
FOR ATTACH_REBUILD_LOG ;


SELECT [CategoryID],[CategoryName],"Description","Picture"
FROM northwind.dbo.Categories 
WHERE CategoryID = 1

SELECT "CategoryID","CategoryName","Description","Picture" FROM "c:\programmation\c#\datacloner\src\databasesscript\northwnd.mdf".dbo.Categories

SELECT * FROM SYS.DATABASES WHERE NAME = 'northwnd'

EXEC sp_detach_db "C:\Programmation\C#\datacloner\src\DatabasesScript\NORTHWND.mdf", 'true';
DROP DATABASE "C:\Programmation\C#\datacloner\src\DatabasesScript\NORTHWND.mdf"

SELECT  
	COL.TABLE_SCHEMA,  
	COL.TABLE_NAME,  
	COL.COLUMN_NAME,  
	COL.DATA_TYPE,  
	COLUMNPROPERTY(object_id(COL.TABLE_NAME), COL.COLUMN_NAME, 'Precision') AS 'Precision',   
	ISNULL(COLUMNPROPERTY(object_id(COL.TABLE_NAME), COL.COLUMN_NAME, 'Scale'), 0) AS 'Scale',   
	CAST(0 AS BIT) AS IsUnsigned,  
	CAST(ISNULL((  
		SELECT TOP 1 1  
		FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU  
		INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC ON TC.CONSTRAINT_CATALOG = KCU.CONSTRAINT_CATALOG AND  
															  TC.CONSTRAINT_SCHEMA = KCU.CONSTRAINT_SCHEMA AND  
															  TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME  
		WHERE  
			KCU.TABLE_CATALOG = COL.TABLE_CATALOG AND  
			KCU.TABLE_SCHEMA = COL.TABLE_SCHEMA AND  
			KCU.TABLE_NAME = COL.TABLE_NAME AND  
			KCU.COLUMN_NAME = COL.COLUMN_NAME AND  
			TC.CONSTRAINT_TYPE = 'PRIMARY KEY'  
	), 0) AS BIT) AS 'IsPrimaryKey',  
	CAST(COLUMNPROPERTY(object_id(COL.TABLE_NAME), COL.COLUMN_NAME, 'IsIdentity') AS BIT) AS 'IsAutoIncrement'  
FROM INFORMATION_SCHEMA.COLUMNS COL  
INNER JOIN INFORMATION_SCHEMA.TABLES TBL ON TBL.TABLE_CATALOG = COL.TABLE_CATALOG AND  
											TBL.TABLE_SCHEMA = COL.TABLE_SCHEMA AND  
											TBL.TABLE_NAME = COL.TABLE_NAME AND  
											TBL.TABLE_TYPE = 'BASE TABLE'  
WHERE COL.TABLE_CATALOG = 'northwnd'  
ORDER BY  
	COL.TABLE_NAME,  
	COL.ORDINAL_POSITION;



INSERT INTO NORTHWND.Categories("CategoryName","Description","Picture")VALUES('as','',null);
