--
-- Drop tables if they exist to make script re-runnable
--
DROP TABLE IF EXISTS Facility;
DROP TABLE IF EXISTS Result;
DROP TABLE IF EXISTS SmileyUser;

--
-- Create tables
--

CREATE TABLE SmileyUser(
	smiley_user_id      VARCHAR(10)   PRIMARY KEY,
	smiley_user_pw      VARBINARY(50) NOT NULL,
	full_name	    VARCHAR(50)   NOT NULL,
	email		    VARCHAR(50)   NOT NULL,
	smiley_user_role    VARCHAR(20)   NOT NULL,
	last_login          DATETIME      NULL
);

INSERT INTO SmileyUser (smiley_user_id, smiley_user_pw , full_name, email, smiley_user_role) VALUES 
('jiamei',   HASHBYTES('SHA1', 'password1'), 'Jiamei Zeng', '19005409@myrp.edu.sg','user'),
('angeline',    HASHBYTES('SHA1', 'password2'), 'Angeline Mok', '19005146@myrp.edu.sg','owner'),
('jinhan',   HASHBYTES('SHA1', 'password3'), 'Jinhan Ke', '19034471@myrp.edu.sg', 'owner'),
('janice', HASHBYTES('SHA1', 'adminpw1'), 'Janice Chang', 'jiameizeng@gmail.com','admin');

CREATE TABLE Facility(
	sensor_id       INT PRIMARY KEY IDENTITY,
	facility_type	VARCHAR(50)  NOT NULL,
	location_name   VARCHAR(50)  NOT NULL,
	address		VARCHAR(200) NOT NULL,
	start_time      TIME  	     NOT NULL,
	end_time        TIME         NOT NULL,
	status		TINYINT      NOT NULL,
	smiley_user_id  VARCHAR(10)  NOT NULL,
	CONSTRAINT fk1 FOREIGN KEY(smiley_user_id) REFERENCES SmileyUser(smiley_user_id)
);

SELECT FORMAT(getdate(), 'hh:mm')

SET IDENTITY_INSERT Facility ON;
INSERT INTO FACILITY(sensor_id, facility_type, location_name, address, start_time, end_time, status, smiley_user_id) VALUES
(1,'door', 'toilet', 'RP_W55#1', '8:00:00','18:00:00', 1, 'angeline'),
(2,'feedback', 'toilet', 'RP_W55#1', '8:00:00', '18:00:00', 1, 'angeline'),

(3,'door', 'classroom', 'RP_W66D', '8:00:00', '17:00:00', 1,'jinhan'),
(4,'feedback', 'classroom', 'RP_W66D', '8:00:00', '17:00:00' , 1, 'jinhan'),


(5,'door', 'KFC', '1 Woodlands Square, #01-40, Singapore 738099', '8:00:00', '22:30:00', 1,'jiamei'),
(6,'feedback', 'KFC', '1 Woodlands Square, #01-40, Singapore 738099', '8:00:00', '22:30:00', 1,'jiamei');

SET IDENTITY_INSERT Facility OFF;

CREATE TABLE Prediction(
	prediction_id   INT PRIMARY KEY IDENTITY,
	result		    VARCHAR(50) NOT NULL,		
	time_stamp      TIMESTAMP   NOT NULL,
	sensor_id       INT 	    NOT NULL,
	CONSTRAINT fk2 FOREIGN KEY(sensor_id) REFERENCES Facility(sensor_id)
);

SET IDENTITY_INSERT Prediction ON;
INSERT INTO Prediction (prediction_id, result, time_stamp, sensor_id) VALUES
(0001,'open','2020-01-15',1),
(0002,'close','2020-01-15',1),

(0003,'good','2020-01-16',2),
(0004,'neutral','2020-01-16',2),
(0005,'bad','2020-01-16',2),

(0006,'open','2020-01-15',3),
(0007,'open','2020-01-15',3),

(0008,'good','2020-01-16',4),
(0009,'good','2020-01-16',4),
(0010,'bad','2020-01-16',4),

(0011,'close','2020-01-15',5),
(0012,'close','2020-01-17',5),
(0013,'open','2020-01-17',5),
(0014,'open','2020-01-15',5),

(0015,'good','2020-01-18',6),
(0016,'good','2020-01-18',6),
(0017,'nuetral','2020-01-18',6),
(0018,'bad','2020-01-18',6),
(0019,'bad','2020-01-19',6),
(0020,'good','2020-01-19',6);

SET IDENTITY_INSERT Prediction OFF;






