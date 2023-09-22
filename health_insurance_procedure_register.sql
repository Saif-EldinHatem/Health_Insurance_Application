CREATE OR REPLACE PROCEDURE OnRegistration
(
    Username VARCHAR2,
    Password VARCHAR2,
	FirstName VARCHAR2,
	LastName VARCHAR2,
	JobTitle VARCHAR2,
	PhoneNum NUMBER,
	Gender VARCHAR2,
	DOB DATE,
	Address VARCHAR2,
    Result OUT NUMBER
)

AS
MaxSSN NUMBER;

BEGIN
	SELECT MAX(SSN) + 1 INTO MaxSSN FROM Users;
    IF (MaxSSN IS NULL) THEN
        MaxSSN := 1;
    END IF;
    
    INSERT INTO Users VALUES (Username, Password, FirstName, LastName, MaxSSN, JobTitle, PhoneNum, Gender, DOB, Address);
	Result := MaxSSN;
END;