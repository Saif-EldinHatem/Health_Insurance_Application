CREATE OR REPLACE PROCEDURE OnLogin
(
    LogUser VARCHAR2,
    LogPassword VARCHAR2,
    Result OUT NUMBER
)
AS
BEGIN
    SELECT SSN INTO Result FROM Users WHERE lower(Username) = lower(LogUser) AND Password = LogPassword;
END;