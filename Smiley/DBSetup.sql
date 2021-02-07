--
-- Drop tables if they exist to make script re-runnable
--
DROP TABLE IF EXISTS FaceId;
DROP TABLE IF EXISTS SmileyCustomer;
DROP TABLE IF EXISTS SmileyUser;
DROP TABLE IF EXISTS Building;
DROP TABLE IF EXISTS Exact_Location;
DROP TABLE IF EXISTS Sensor;
DROP TABLE IF EXISTS Emotion;
DROP TABLE IF EXISTS Door;
DROP TABLE IF EXISTS Feedback;


--
--Create user/customer's face id table
--
CREATE TABLE FaceId(
	face_id             INT PRIMARY KEY IDENTITY,
	face_picfile        VARCHAR(100)  NOT NULL
);


--
--
--Insert records of user/customer face id
SET IDENTITY_INSERT FaceId ON;
INSERT INTO FaceId(face_id, face_picfile) VALUES
(1,'19005146@myrp.edu.sg'), ---name face_picfile as email address to prevent duplicate
(2,'jiameizeng@gmail.com'),
(3,'19034471@myrp.edu.sg'),
(4,'nanalim@gmail.com'),
(5,'akaitan@gmail.com'),
(6,'aldouschen@gmail.com'),
(7,'alicemok@gmail.com'),
(8,'misayolim@gmail.com'),
(9,'alucardzeng@gmail.com'),
(10,'miyuutan@gmail.com'),
(11,'angelalee@gmail.com'),
(12,'aurorachen@gmail.com'),
(13,'marukomok@gmail.com'),
(14,'azezeng@gmail.com'),
(15,'brodylee@gmail.com'),
(16,'beatrixlee@gmail.com'),
(17,'claudetan@gmail.com'),
(18,'atomke@gmail.com'),
(19,'chimok@gmail.com'),
(20,'alphachen@gmail.com'),
(21,'lunamok@gmail.com'),
(22,'lunoxke@gmail.com'),
(23,'grangerzeng@gmail.com'),
(24,'19005409@myrp.edu.sg')
SET IDENTITY_INSERT FaceId OFF;



--
--Create customer's table
--
CREATE TABLE SmileyCustomer(
	customer_id         	INT PRIMARY KEY IDENTITY,
	customer_name	    	VARCHAR(50)  	NOT NULL,
	surname			VARCHAR(50)  	NOT NULL,
	email			VARCHAR(50)  	NOT NULL,
	membership          	VARCHAR(50)  	NOT NULL,
	picfile                 VARCHAR(200)    NULL,
	signup_date         	DATETIME   	NOT NULL,
	face_id			INT 		NOT NULL,
	CONSTRAINT fk1 FOREIGN KEY(face_id) REFERENCES FaceId(face_id)
);


--
--Insert customers' records
--
SET IDENTITY_INSERT SmileyCustomer ON;
INSERT INTO SmileyCustomer(customer_id, customer_name, surname, email, membership, signup_date, face_id) VALUES
(00001, 'Nana', 'Lim', 'nanalim@gmail.com', 'bronze', '2020-01-21 08:01:33', 4),
(00002, 'Akai', 'Tan', 'akaitan@gmail.com', 'gold','2020-01-21 08:03:34', 5),
(00003, 'Aldous', 'Chen','aldouschen@gmail.com', 'silver', '2020-01-21 09:01:33', 6),
(00004, 'Alice', 'Mok','alicemok@gmail.com', 'silver', '2020-01-21 09:05:33', 7),
(00005, 'Misayo', 'Lim','misayolim@gmail.com', 'bronze', '2020-03-03 10:01:33', 8),
(00006, 'Alucard', 'Zeng','alucardzeng@gmail.com', 'bronze',  '2020-03-03 11:01:33', 9),
(00007, 'Miyuu', 'Tan','miyuutan@gmail.com', 'bronze', '2020-03-03 12:01:33', 10),
(00008, 'Angela', 'Lee','angelalee@gmail.com', 'silver', '2020-03-03 13:01:33', 11),
(00009, 'Aurora', 'Chen','aurorachen@gmail.com', 'silver', '2020-04-28 14:01:33', 12),
(00010, 'Maruko', 'Mok','marukomok@gmail.com', 'gold', '2020-04-28 16:01:33', 13),
(00011, 'Aze', 'Zeng','azezeng@gmail.com', 'gold', '2020-04-28 16:01:33', 14),
(00012, 'Brody', 'Lee','brodylee@gmail.com', 'gold',  '2021-01-01 17:01:33', 15),
(00013, 'Beatrix', 'Zeng','beatrixlee@gmail.com', 'silver', '2021-01-21 18:01:33', 16),
(00014, 'Claude', 'Tan','claudetan@gmail.com', 'gold', '2021-01-21 18:01:33', 17),
(00015, 'Atom', 'ke','atomke@gmail.com', 'silver', '2021-01-21 19:01:33', 18),
(00016, 'Chi', 'Mok','chimok@gmail.com', 'bronze', '2021-01-28 19:01:33', 19),
(00017, 'Alpha', 'Chen','alphachen@gmail.com', 'bronze', '2021-01-28 20:01:33', 20),
(00018, 'Luna', 'Mok','lunamok@gmail.com', 'bronze', '2021-01-28 20:01:33', 21),
(00019, 'Lunox', 'ke','lunoxke@gmail.com', 'silver', '2021-01-28 21:01:33', 22),
(00020, 'Granger', 'Zeng','grangerzeng@gmail.com', 'silver', '2021-01-28 22:01:33', 23);
SET IDENTITY_INSERT SmileyCustomer OFF;



--
-- Create smiley's user table
--
CREATE TABLE SmileyUser(
	smiley_user_id		VARCHAR(20)   	PRIMARY KEY,
	smiley_user_pw      	VARBINARY(50) 	NOT NULL,
	full_name		VARCHAR(50)   	NOT NULL,
	email			VARCHAR(50)   	NOT NULL,
	smiley_user_role    	VARCHAR(20)   	NOT NULL,
	last_login          	DATETIME      	NULL,
	face_id			INT 		NOT NULL,
	superior_id 		VARCHAR(20)	NULL,
	CONSTRAINT fk2 FOREIGN KEY(face_id) REFERENCES FaceId(face_id)
);



--
--Insert smiley users' records
--
INSERT INTO SmileyUser (smiley_user_id, smiley_user_pw , full_name, email, smiley_user_role, superior_id, face_id) VALUES 
('janice', HASHBYTES('SHA1', 'adminpw1'), 'Janice Chang','jiameizeng@gmail.com','admin', NULL, 2), --admin can CRUD all available sensor records
('angeline',    HASHBYTES('SHA1', 'password2'), 'Angeline Mok','19005146@myrp.edu.sg','owner', NULL, 1),
('jinhan',   HASHBYTES('SHA1', 'password3'), 'Jinhan Ke','19034471@myrp.edu.sg', 'owner', NULL, 3),
--owner can CRUD all of its sensor records
('jiamei',   HASHBYTES('SHA1', 'password1'), 'Jiamei Zeng','19005409@myrp.edu.sg','user', 'jinhan',23 );--user can view all of its sensor records
 

--
--Create sensor's building table
--
CREATE TABLE Building(
	building_id			INT PRIMARY KEY IDENTITY,
	building_name			VARCHAR(50)  NOT NULL,
	building_type			VARCHAR(50)  NOT NULL,
	building_address		VARCHAR(200)  NOT NULL,
	building_postal_code		INT           NOT NULL
);


--
--Insert sensor building's records
--
SET IDENTITY_INSERT Building ON;
INSERT INTO Building (building_id, building_name , building_type, building_address, building_postal_code) VALUES 
(1, 'Causeway Point', 'Shopping Mall', '1 Woodlands Square, Singapore', 738099),
(2, 'ION Orchard', 'Shopping Mall', '2 Orchard Turn, Singapore', 238801),
(3, 'Republic Polytechnic', 'School', '9 Woodlands Ave 9, Singapore', 738964),
(4, 'Marina Bay Sands Singapore', 'Hotel','10 Bayfront Ave, Singapore', 018956),
(5, 'Esplanade', 'Performing arts centre', '1 Esplanade Dr, Singapore', 038981),
(6, 'Guoco Tower', 'Skyscrper', ' 1 Wallich St, Singapore', 078881),
(7, 'Capital Tower', 'Business center', '168 Robinson Rd, Singapore', 068912),
(8, 'One Pearl Bank', 'Condominium complex', '1 Pearl Bank, Singapore', 169106),
(9, 'Old Hill Street Police Station', 'Historic Building', '140 Hill St, Singapore', 179369),
(10, 'Funan Mall', 'Shopping Mall', '107 North Bridge Rd, Singapore', 179105);
SET IDENTITY_INSERT Building OFF;


--
--Create table for sensor's exact location
--
CREATE TABLE Exact_Location(
	location_id		INT PRIMARY KEY IDENTITY,
	location_name		VARCHAR(50) NOT NULL,
	location_type		VARCHAR(50) NOT NULL,
	location_address 	VARCHAR(50) NOT NULL,
	building_id		INT NOT NULL,
	CONSTRAINT fk3 FOREIGN KEY(building_id) REFERENCES Building(building_id)
);

--
--Insert records for sensor's exact location
--
SET IDENTITY_INSERT Exact_Location ON;
INSERT INTO Exact_Location(location_id, location_name, location_type, location_address, building_id) VALUES
(1, 'KFC','Restaurant','#01-40', 1),
(2, 'Aburi-EN', 'Restaurant', '#02-09B', 1),
(3, 'Hong Kong Sheng Kee Dessert', 'Restaurant', '#B1-24', 1),
(4, 'Daiso', 'Dollar Store', '#B4-47', 2),
(5, 'Olivia Burton', 'Watch Store', '#B1-10', 2),
(6, 'The Republic Cultural Centre', 'Cafe', 'TRCC', 3),
(7, 'Twelve Cupcakes', 'Dessert Shop', '#B2-22', 6),
(8, 'Randy Indulgence Cereal & Acai Bar', 'Dessert Shop', '#B2-20', 6),
(9, 'The Oyster Bank', 'Restaurant', '#B1-30', 10),
(10, 'Sushi Express', 'Restaurant', ' #B2-14',10);
SET IDENTITY_INSERT Exact_Location OFF;


--Create sensor table
--
CREATE TABLE Sensor(
	sensor_id       INT PRIMARY KEY IDENTITY,
	start_time      TIME         NOT NULL,
	end_time        TIME         NOT NULL,
	sensor_status	TINYINT      NOT NULL,
	smiley_user_id  VARCHAR(20)  NOT NULL,
	location_id     INT 	     NOT NULL,
	CONSTRAINT fk4 FOREIGN KEY(smiley_user_id) REFERENCES SmileyUser(smiley_user_id),
	CONSTRAINT fk5 FOREIGN KEY(location_id) REFERENCES Exact_Location(location_id)
);

--
--Insert sensor record
--
SET IDENTITY_INSERT Sensor ON;
INSERT INTO Sensor(sensor_id, start_time, end_time, sensor_status, smiley_user_id, location_id) VALUES
(1, '08:00', '20:00', 1, 'jiamei', 1),
(2, '09:00', '21:00', 0, 'jiamei', 2),
(3, '05:00', '20:00', 1, 'jiamei', 3),
(4, '10:00', '22:00', 0, 'angeline', 4),
(5, '11:00', '23:00', 1, 'angeline', 5),
(6, '9:30', '21:30', 0, 'angeline', 6),
(7, '10:30', '22:30', 1, 'jinhan', 7),
(8, '08:30', '20:30', 0, 'jinhan', 8),
(9, '05:00', '22:00', 1, 'jinhan', 9),
(10, '11:00', '22:00', 0, 'jinhan', 10);
SET IDENTITY_INSERT Sensor OFF;


--
--Create table for emotion
--
CREATE TABLE Emotion(
	emotion_record_id      	INT PRIMARY KEY IDENTITY,
	emotion_type		VARCHAR(50)   NOT NULL,
	time_stamp       	DATETIME      NOT NULL,
	sensor_id		INT           NOT NULL,
	CONSTRAINT fk6 FOREIGN KEY(sensor_id) REFERENCES Sensor(sensor_id)
);


--
--Insert emotion records
--
SET IDENTITY_INSERT Emotion ON;
INSERT INTO Emotion(emotion_record_id, emotion_type, time_stamp, sensor_id) VALUES
(1, 'anger', '2020-01-21 09:00:01', 1),
(2, 'anger', '2020-01-21 09:10:01', 1),
(3, 'anticipation', '2020-01-21 09:11:01', 1),
(4, 'anticipation', '2020-01-21 09:12:01', 1),
(5, 'joy', '2020-01-21 10:01:21', 2),
(6, 'joy', '2020-01-21 11:00:01', 2),
(7, 'joy', '2020-01-21 11:22:01', 2),
(8, 'trust', '2020-01-21 12:23:01', 3),
(9, 'trust', '2020-01-21 13:11:01', 4),
(10, 'trust', '2020-01-21 13:11:01', 4),
(11, 'fear', '2020-01-21 14:12:01', 5),
(12, 'fear', '2020-01-21 14:14:01', 5),
(13, 'fear', '2020-01-21 15:11:01', 6),
(14, 'surprise', '2020-01-21 15:12:01', 7),
(15, 'surprise', '2020-01-21 16:13:01', 7),
(16, 'surprise', '2020-01-21 16:14:01', 8),
(17, 'sadness', '2020-01-21 17:11:01', 8),
(18, 'disgust', '2020-01-21 17:11:01', 9),
(19, 'digust', '2020-01-21 16:11:01', 9),
(20, 'sadness', '2020-01-21 16:11:01', 10);
SET IDENTITY_INSERT Emotion OFF;

--
--Create table for sensors use for door
--
CREATE TABLE Door(
	door_record_id   INT PRIMARY KEY IDENTITY,
	door_gesture	 VARCHAR(50)     NOT NULL,
	time_stamp       DATETIME        NOT NULL,
	sensor_id	 INT 	         NOT NULL,
	CONSTRAINT fk7 FOREIGN KEY(sensor_id) REFERENCES Sensor(sensor_id)
);

--
--Insert records for door
--
SET IDENTITY_INSERT Door ON;
INSERT INTO Door(door_record_id, door_gesture, time_stamp, sensor_id) VALUES
(1, 'Open', '2020-01-21 11:00:00', 1),
(2, 'Open', '2020-01-21 11:11:11',1),
(3, 'Open', '2020-01-21 11:12:12',1),
(4, 'Close', '2020-01-21 12:10:01',1),
(5, 'Open', '2020-03-03 12:05:07',2),
(6, 'Open', '2020-03-03 13:00:01',2),
(7, 'Open', '2020-03-03 14:00:01',2),
(8, 'Close', '2020-03-03 14:05:07',3),
(9, 'Open', '2020-04-28 15:11:08',4),
(10, 'Open', '2020-04-28 15:14:07',4),
(11, 'Open', '2020-04-28 12:05:07',5),
(12, 'Close', '2021-01-01 12:05:07',5),
(13, 'Open', '2021-01-21 13:11:05',6),
(14, 'Open', '2021-01-21 13:10:06',7),
(15, 'Open', '2021-01-21 14:05:02',7),
(16, 'Open', '2021-01-28 14:05:01',8),
(17, 'Open', '2021-01-28 15:20:11',8),
(18, 'Open', '2021-01-28 15:20:50',9),
(19, 'Open', '2021-01-28 16:30:40',9),
(20, 'Open', '2021-01-28 16:30:33',10);
SET IDENTITY_INSERT Door OFF;

--
--Create feedback record table
--
CREATE TABLE Feedback(
	feedback_id	  INT PRIMARY KEY IDENTITY,
	feedback_gesture  VARCHAR(50) NOT NULL,		
	time_stamp        DATETIME    NOT NULL,
	sensor_id         INT 	      NOT NULL,
	CONSTRAINT fk8 FOREIGN KEY(sensor_id) REFERENCES Sensor(sensor_id)
);

--
--Insert feedback record table 
--
SET IDENTITY_INSERT Feedback ON;
INSERT INTO Feedback(feedback_id, feedback_gesture, time_stamp, sensor_id) VALUES
(1, 'Good', '2020-01-21 11:12:11', 1),
(2, 'Good', '2020-01-21 11:13:33',1),
(3, 'Good', '2020-01-21 11:14:05',1),
(4, 'Good', '2020-01-21 11:15:00',1),
(5, 'Good', '2020-03-03 12:00:09',2),
(6, 'Bad', '2020-03-03 12:00:11',2),
(7, 'Bad', '2020-03-03 13:00:11',2),
(8, 'Bad', '2020-03-03 13:00:11',3),
(9, 'Bad', '2020-04-28 14:00:11',4),
(10, 'Neutral', '2020-04-28 14:00:11',4),
(11, 'Neutral', '2020-04-28 14:00:11',5),
(12, 'Good', '2021-01-01 15:09:11',5),
(13, 'Good', '2021-01-21 15:00:11',6),
(14, 'Neutral', '2021-01-21 16:00:11',7),
(15, 'Neutral', '2021-01-21 16:00:11',7),
(16, 'Good', '2021-01-28 17:00:11',8),
(17, 'Good', '2021-01-28 17:00:11',8),
(18, 'Bad', '2021-01-28 19:00:11',9),
(19, 'Bad', '2021-01-28 11:00:11',9),
(20, 'Bad', '2021-01-28 12:00:11',10);
SET IDENTITY_INSERT Feedback OFF;



