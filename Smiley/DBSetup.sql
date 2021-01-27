--
-- Drop tables if they exist to make script re-runnable
--
DROP TABLE IF EXISTS SmileyUser; --
DROP TABLE IF EXISTS Building; --
DROP TABLE IF EXISTS Sensor; --
DROP TABLE IF EXISTS Exact_Location; --
DROP TABLE IF EXISTS Door; --
DROP TABLE IF EXISTS Feedback;--
DROP TABLE IF EXISTS FaceId; --
DROP TABLE IF EXISTS SmileyCustomer; --
DROP TABLE IF EXISTS Emotion; 



--
-- Create smiley's user table
--
CREATE TABLE SmileyUser(
	smiley_user_id      VARCHAR(10)   PRIMARY KEY,
	smiley_user_pw      VARBINARY(50) NOT NULL,
	full_name			VARCHAR(50)   NOT NULL,
	smiley_user_picfile VARCHAR(200)  NULL,
	email				VARCHAR(50)   NOT NULL,
	smiley_user_role    VARCHAR(20)   NOT NULL,
	last_login          DATETIME      NULL
);


--
--Insert smiley users' records
--
INSERT INTO SmileyUser (smiley_user_id, smiley_user_pw , full_name, email, smiley_user_role) VALUES 
('jiamei',   HASHBYTES('SHA1', 'password1'), 'Jiamei Zeng', '19005409@myrp.edu.sg','user'), --user can view all of its sensor records
('angeline',    HASHBYTES('SHA1', 'password2'), 'Angeline Mok', '19005146@myrp.edu.sg','owner'), --owner can CRUD all of its sensor records
('jinhan',   HASHBYTES('SHA1', 'password3'), 'Jinhan Ke', '19034471@myrp.edu.sg', 'owner'), 
('janice', HASHBYTES('SHA1', 'adminpw1'), 'Janice Chang', 'jiameizeng@gmail.com','admin'); --admin can CRUD all available sensor records 


--
--Create sensor's building table
--
CREATE TABLE Building(
	building_id			INT PRIMARY KEY IDENTITY,
	building_name		VARCHAR(50)  NOT NULL,
	building_type		VARCHAR(50)  NOT NULL,
	building_address	VARCHAR(200)  NOT NULL,
);


--
--Insert sensor building's records
--
SET IDENTITY_INSERT Building ON;
INSERT INTO Building (building_id, building_name , building_type, building_address) VALUES 
(1, 'Causeway Point', 'Shopping Mall', '1 Woodlands Square, Singapore 738099'),
(2, 'ION Orchard', 'Shopping Mall', '2 Orchard Turn, Singapore 238801'),
(3, 'Republic Polytechnic', 'School', '9 Woodlands Ave 9, Singapore 738964'),
(4, 'Marina Bay Sands Singapore', 'Hotel','10 Bayfront Ave, Singapore 018956'),
(5, 'Esplanade', 'Performing arts centre', '1 Esplanade Dr, Singapore 038981'),
(6, 'Guoco Tower', 'Skyscrper', ' 1 Wallich St, Singapore 078881'),
(7, 'Capital Tower', 'Business center', '168 Robinson Rd, Singapore 068912'),
(8, 'One Pearl Bank', 'Condominium complex', '1 Pearl Bank, Singapore 169106'),
(9, 'Old Hill Street Police Station', 'Historic Building', '140 Hill St, Singapore 179369'),
(10, 'Funan Mall', 'Shopping Mall', '107 North Bridge Rd, Singapore 179105');
SET IDENTITY_INSERT Building OFF;

--
--Create sensor table
--
CREATE TABLE Sensor(
	sensor_id       INT PRIMARY KEY IDENTITY,
	start_time      TIME  	     NULL,
	end_time        TIME         NULL,
	sensor_status	TINYINT      NOT NULL,
	smiley_user_id  VARCHAR(10)  NOT NULL,
	building_id     INT 	     NOT NULL,
	CONSTRAINT fk11 FOREIGN KEY(smiley_user_id) REFERENCES SmileyUser(smiley_user_id),
	CONSTRAINT fk12 FOREIGN KEY(building_id) REFERENCES Building(building_id)
);

--
--Insert sensor record
--
SET IDENTITY_INSERT Sensor ON;
INSERT INTO Sensor(sensor_id, sensor_status, smiley_user_id, building_id) VALUES
(1, 1, 'jiamei', 1),
(2, 0, 'jiamei', 1),
(3, 1, 'jiamei', 1),
(4, 0, 'angeline', 2),
(5, 1, 'angeline', 2),
(6, 0, 'angeline', 3),
(7, 1, 'jinhan', 6),
(8, 0, 'jinhan', 6),
(9, 1, 'jinhan', 10),
(10, 0, 'jinhan', 10);
SET IDENTITY_INSERT Sensor OFF;

--
--Create table for sensor's exact location
--
CREATE TABLE Exact_Location(
	location_id		 INT PRIMARY KEY IDENTITY,
	location_name	 VARCHAR(50) NOT NULL,
	location_type	 VARCHAR(50) NOT NULL,
	location_address VARCHAR(50) NOT NULL,
	building_id		 INT 	     NOT NULL,
	sensor_id		 INT 	     NOT NULL,
	CONSTRAINT fk13 FOREIGN KEY(building_id) REFERENCES Building(building_id),
	CONSTRAINT fk14 FOREIGN KEY(sensor_id) REFERENCES Sensor(sensor_id)
);

--
--Insert records for sensor's exact location
--
SET IDENTITY_INSERT Exact_Location ON;
INSERT INTO Exact_Location(location_id, location_name, location_type, location_address, building_id, sensor_id) VALUES
(1, 'KFC','Restaurant','#01-40', 1, 1 ),
(2, 'Aburi-EN', 'Restaurant', '#02-09B', 1, 2 ),
(3, 'Hong Kong Sheng Kee Dessert', 'Restaurant', '#B1-24', 1, 3),
(4, 'Daiso', 'Dollar Store', '#B4-47', 2, 4),
(5, 'Olivia Burton', 'Watch Store', '#B1-10', 2, 5),
(6, 'The Republic Cultural Centre', 'Cafe', 'TRCC', 3, 6),
(7, 'Twelve Cupcakes', 'Dessert Shop', '#B2-22', 6, 7),
(8, 'Randy Indulgence Cereal & Acai Bar', 'Dessert Shop', '#B2-20', 6, 8),
(9, 'The Oyster Bank', 'Restaurant', '#B1-30', 10, 9),
(10, 'Sushi Express', 'Restaurant', ' #B2-14',10, 10);
SET IDENTITY_INSERT Exact_Location OFF;

--
--Create table for sensors use for door
--
CREATE TABLE Door(
	door_record_id   INT PRIMARY KEY IDENTITY,
	door_gesture	 VARCHAR(50) NOT NULL,
	time_stamp       DATE   NOT NULL,
	sensor_id		 INT 	     NOT NULL,
	CONSTRAINT fk15 FOREIGN KEY(sensor_id) REFERENCES Sensor(sensor_id)
);

--
--Insert records for door
--
SET IDENTITY_INSERT Door ON;
INSERT INTO Door(door_record_id, door_gesture, time_stamp, sensor_id) VALUES
(1, 'Open', '2020-01-21', 1),
(2, 'Open', '2020-01-21',1),
(3, 'Open', '2020-01-21',1),
(4, 'Close', '2020-01-21',1),
(5, 'Open', '2020-03-03',2),
(6, 'Open', '2020-03-03',2),
(7, 'Open', '2020-03-03',2),
(8, 'Close', '2020-03-03',3),
(9, 'Open', '2020-04-28',4),
(10, 'Open', '2020-04-28',4),
(11, 'Open', '2020-04-28',5),
(12, 'Close', '2021-01-01',5),
(13, 'Open', '2021-01-21',6),
(14, 'Open', '2021-01-21',7),
(15, 'Open', '2021-01-21',7),
(16, 'Open', '2021-01-28',8),
(17, 'Open', '2021-01-28',8),
(18, 'Open', '2021-01-28',9),
(19, 'Open', '2021-01-28',9),
(20, 'Open', '2021-01-28',10);
SET IDENTITY_INSERT Door OFF;

--
--Create feedback record table
--
CREATE TABLE Feedback(
	feedback_id		  INT PRIMARY KEY IDENTITY,
	feedback_gesture  VARCHAR(50) NOT NULL,		
	time_stamp        DATE   NOT NULL,
	sensor_id         INT 	      NOT NULL,
	CONSTRAINT fk16 FOREIGN KEY(sensor_id) REFERENCES Sensor(sensor_id)
);

--
--Insert feedback record table 
--
SET IDENTITY_INSERT Feedback ON;
INSERT INTO Feedback(feedback_id, feedback_gesture, time_stamp, sensor_id) VALUES
(1, 'Good', '2020-01-21', 1),
(2, 'Good', '2020-01-21',1),
(3, 'Good', '2020-01-21',1),
(4, 'Good', '2020-01-21',1),
(5, 'Good', '2020-03-03',2),
(6, 'Bad', '2020-03-03',2),
(7, 'Bad', '2020-03-03',2),
(8, 'Bad', '2020-03-03',3),
(9, 'Bad', '2020-04-28',4),
(10, 'Neutral', '2020-04-28',4),
(11, 'Neutral', '2020-04-28',5),
(12, 'Good', '2021-01-01',5),
(13, 'Good', '2021-01-21',6),
(14, 'Neutral', '2021-01-21',7),
(15, 'Neutral', '2021-01-21',7),
(16, 'Good', '2021-01-28',8),
(17, 'Good', '2021-01-28',8),
(18, 'Bad', '2021-01-28',9),
(19, 'Bad', '2021-01-28',9),
(20, 'Bad', '2021-01-28',10);
SET IDENTITY_INSERT Feedback OFF;


--
--Create user/customer's face id table
--
CREATE TABLE FaceId(
	face_record_id      INT PRIMARY KEY IDENTITY,
	face_picfile        VARCHAR(50)  NOT NULL,
	sensor_id	        INT          NOT NULL,
	CONSTRAINT fk17 FOREIGN KEY(sensor_id) REFERENCES Sensor(sensor_id)
);


--
--
--Insert records of user/customer face id
--FROM WEB APP
--
--

--
--Create customer's table
--
CREATE TABLE SmileyCustomer(
	customer_id         INT PRIMARY KEY IDENTITY,
	customer_name	    VARCHAR(50)  NOT NULL,
	surname				VARCHAR(50)  NOT NULL,
	email				VARCHAR(50)  NOT NULL,
	membership          VARCHAR(50)  NOT NULL,
	signup_date         DATE    NOT NULL,
	sensor_id	        INT          NOT NULL,
	CONSTRAINT fk18 FOREIGN KEY(sensor_id) REFERENCES Sensor(sensor_id)
);


--
--Insert customers' records
--
SET IDENTITY_INSERT SmileyCustomer ON;
INSERT INTO SmileyCustomer(customer_id, customer_name, surname, email, membership, signup_date, sensor_id) VALUES
(00001, 'Nana', 'Lim', 'nanalim@gmail.com', 'bronze', '2020-01-21', 1),
(00002, 'Akai', 'Tan', 'akaitan@gmail.com', 'gold','2020-01-21',1),
(00003, 'Aldous', 'Chen','aldouschen@gmail.com', 'silver', '2020-01-21',1),
(00004, 'Alice', 'Mok','alicemok@gmail.com', 'silver', '2020-01-21',1),
(00005, 'Misayo', 'Lim','misayolim@gmail.com', 'bronze', '2020-03-03',2),
(00006, 'Alucard', 'Zeng','alucardzeng@gmail.com', 'bronze',  '2020-03-03',2),
(00007, 'Miyuu', 'Tan','miyuutan@gmail.com', 'bronze', '2020-03-03',2),
(00008, 'Angela', 'Lee','angelalee@gmail.com', 'silver', '2020-03-03',3),
(00009, 'Aurora', 'Chen','aurorachen@gmail.com', 'silver', '2020-04-28',4),
(00010, 'Maruko', 'Mok','marukomok@gmail.com', 'gold', '2020-04-28',4),
(00011, 'Aze', 'Zeng','azezeng@gmail.com', 'gold', '2020-04-28',5),
(00012, 'Brody', 'Lee','brodylee@gmail.com', 'gold',  '2021-01-01',5),
(00013, 'Beatrix', 'Zeng','beatrixlee@gmail.com', 'silver', '2021-01-21',6),
(00014, 'Claude', 'Tan','claudetan@gmail.com', 'gold', '2021-01-21',7),
(00015, 'Atom', 'ke','atomke@gmail.com', 'silver', '2021-01-21',7),
(00016, 'Chi', 'Mok','chimok@gmail.com', 'bronze', '2021-01-28',8),
(00017, 'Alpha', 'Chen','alphachen@gmail.com', 'bronze', '2021-01-28',8),
(00018, 'Luna', 'Mok','lunamok@gmail.com', 'bronze', '2021-01-28',9),
(00019, 'Lunox', 'ke','lunoxke@gmail.com', 'silver', '2021-01-28',9),
(00020, 'Granger', 'Zeng','grangerzeng@gmail.com', 'silver', '2021-01-28',10);
SET IDENTITY_INSERT SmileyCustomer OFF;

--
--Create table for emotion
--
CREATE TABLE Emotion(
	emotion_record_id      INT PRIMARY KEY IDENTITY,
	emotion_type		   VARCHAR(50)   NOT NULL,
	sensor_id			   INT           NOT NULL,
	CONSTRAINT fk19 FOREIGN KEY(sensor_id) REFERENCES Sensor(sensor_id)
);


--
--Insert emotion records
--
SET IDENTITY_INSERT Emotion ON;
INSERT INTO Emotion(emotion_record_id, emotion_type, sensor_id) VALUES
(1, 'anger', 1),
(2, 'anger', 1),
(3, 'anticipation', 1),
(4, 'anticipation', 1),
(5, 'joy',2),
(6, 'joy', 2),
(7, 'joy', 2),
(8, 'trust', 3),
(9, 'trust', 4),
(10, 'trust', 4),
(11, 'fear',5),
(12, 'fear', 5),
(13, 'fear', 6),
(14, 'surprise', 7),
(15, 'surprise', 7),
(16, 'surprise', 8),
(17, 'sadness', 8),
(18, 'disgust', 9),
(19, 'digust', 9),
(20, 'sadness', 10);
SET IDENTITY_INSERT Emotion OFF;
