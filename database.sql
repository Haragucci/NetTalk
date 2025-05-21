CREATE DATABASE IF NOT EXISTS nettalk;
USE nettalk;

-- Tabelle für Benutzer
CREATE TABLE IF NOT EXISTS users (
    id VARCHAR(50) NOT NULL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL
);

-- Tabelle für Räume
CREATE TABLE IF NOT EXISTS rooms (
    room_id VARCHAR(50) NOT NULL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    creator VARCHAR(50) NOT NULL,
    FOREIGN KEY (creator) REFERENCES users(id) ON DELETE SET NULL ON UPDATE CASCADE
);

-- Tabelle für Raum-Operatoren (mehrere Operatoren pro Raum möglich)
CREATE TABLE IF NOT EXISTS room_operators (
    room_id VARCHAR(50) NOT NULL,
    user_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (room_id, user_id),
    FOREIGN KEY (room_id) REFERENCES rooms(room_id) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE
);

-- Tabelle für gebannte Nutzer pro Raum
CREATE TABLE IF NOT EXISTS room_banned (
    room_id VARCHAR(50) NOT NULL,
    user_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (room_id, user_id),
    FOREIGN KEY (room_id) REFERENCES rooms(room_id) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE
);

-- Tabelle für eingeloggte Nutzer in Räumen
CREATE TABLE IF NOT EXISTS room_joined (
    room_id VARCHAR(50) NOT NULL,
    user_id VARCHAR(50) NOT NULL,
    PRIMARY KEY (room_id, user_id),
    FOREIGN KEY (room_id) REFERENCES rooms(room_id) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE ON UPDATE CASCADE
);
