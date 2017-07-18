CREATE USER webappuser
    FOR LOGIN webappuser
GO

EXEC sp_addrolemember 'db_datareader', 'webappuser'
EXEC sp_addrolemember 'db_datawriter', 'webappuser'
EXEC sp_addrolemember 'db_ddladmin', 'webappuser'
GO