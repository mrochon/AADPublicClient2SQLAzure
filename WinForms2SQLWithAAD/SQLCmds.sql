CREATE USER [sql users] FROM EXTERNAL PROVIDER;
alter role db_owner add member [sql users]