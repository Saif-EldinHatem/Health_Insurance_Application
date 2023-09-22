DROP TABLE Plan_Providers;
DROP TABLE Plan_Subscriptions;
DROP TABLE Dependents;
DROP TABLE Insurance_Plans;
DROP TABLE Providers;
DROP TABLE Users;

CREATE TABLE Users
(
    UserName varchar2 (30) UNIQUE,
    Password varchar2(30) NOT NULL,
    First_Name varchar2(20) NOT NULL,
    Last_Name varchar2(20) NOT NULL,
    SSN number(15) PRIMARY KEY,
    Job_Title varchar2(20),
    Phone_Num number(12) NOT NULL,
    Gender varchar2(1) NOT NULL,
    DOB date NOT NULL,
    Address varchar2(100)
);

CREATE TABLE Dependents
(
    User_SSN number(15) NOT NULL,
    First_Name varchar2(20) NOT NULL,
    Last_Name varchar2(20) NOT NULL,
    Gender varchar2(1) NOT NULL,
    DOB date NOT NULL,
    Relationship varchar2(30),

    FOREIGN KEY (User_SSN) REFERENCES Users(SSN)
);

CREATE TABLE Insurance_Plans
(
    P_ID number(5) PRIMARY KEY,
    Name varchar2(20) NOT NULL,
    Create_Date date NOT NULL
);

CREATE TABLE Providers
(
    PR_ID number(10) PRIMARY KEY,
    Type varchar2(20) NOT NULL,
    Name varchar2(50) NOT NULL,
    Address varchar2(100) NOT NULL
);

CREATE TABLE Plan_Providers
(
    P_ID number(5) NOT NULL,
    PR_ID number(10) NOT NULL,
	
	FOREIGN KEY (P_ID) REFERENCES Insurance_Plans(P_ID),
	FOREIGN KEY (PR_ID) REFERENCES Providers(PR_ID),
	CONSTRAINT PK_PlanProviders PRIMARY KEY(P_ID, PR_ID)
);

CREATE TABLE Plan_Subscriptions
(
	SSN number(15) NOT NULL,
    P_ID number(5) NOT NULL,
	Start_Date date NOT NULL,
	End_Date date NOT NULL,
	
	FOREIGN KEY (SSN) REFERENCES Users(SSN),
	FOREIGN KEY (P_ID) REFERENCES Insurance_Plans(P_ID),
	CONSTRAINT PK_PlanSubscriptions PRIMARY KEY(SSN)
);

-- Data

INSERT INTO Insurance_Plans VALUES (1, 'Beginner', '01-APR-2023');
INSERT INTO Insurance_Plans VALUES (2, 'Intermediate', '01-APR-2023');
INSERT INTO Insurance_Plans VALUES (3, 'Advanced', '01-APR-2023');

INSERT INTO Providers VALUES (1, 'Hospital', 'Air Force', 'Al Tagamo3 5');
INSERT INTO Providers VALUES (2, 'Hospital', 'Ain Shams Al Ta5asossi', 'Abassiya');
INSERT INTO Providers VALUES (3, 'Hospital', '7asabo', 'Nasr City');
INSERT INTO Providers VALUES (4, 'Hospital', 'Saudi German', 'El Nozha');
INSERT INTO Providers VALUES (5, 'Pharmacy', 'Al Ezaby', 'Sheraton');
INSERT INTO Providers VALUES (6, 'Pharmacy', 'Seif', 'Nasr City');
INSERT INTO Providers VALUES (7, 'Pharmacy', '19011', 'Heliopolis');
INSERT INTO Providers VALUES (8, 'Pharmacy', 'Tarshobi', 'Nasr City');

INSERT INTO Plan_Providers VALUES (1, 1);
INSERT INTO Plan_Providers VALUES (1, 4);
INSERT INTO Plan_Providers VALUES (2, 1);
INSERT INTO Plan_Providers VALUES (2, 2);
INSERT INTO Plan_Providers VALUES (2, 4);
INSERT INTO Plan_Providers VALUES (2, 5);
INSERT INTO Plan_Providers VALUES (3, 1);
INSERT INTO Plan_Providers VALUES (3, 2);
INSERT INTO Plan_Providers VALUES (3, 3);
INSERT INTO Plan_Providers VALUES (3, 4);
INSERT INTO Plan_Providers VALUES (3, 5);
INSERT INTO Plan_Providers VALUES (3, 6);
