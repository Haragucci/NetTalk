# This is an ongoing side project, so improvements to code structure, security, and refactoring will be coming soon.

# NetTalk

NetTalk is a console-based network chat application written in **C#**, allowing multiple users to communicate in various chat rooms backed by a MySQL database (using XAMPP).

---

## Features

- User registration, login, and logout
- Creation and management of chat rooms with custom names
- Support for multiple concurrent chat rooms
- User roles within rooms: regular users and operators
- Room-specific commands:
  - `/leave` — Leave the current room
  - `/active` — Show active users in the room
  - `/op` — Grant operator status (operator-only)
  - `/deop` — Revoke operator status
  - `/ban` — Ban a user from the room
  - `/unban` — Unban a user
  etc.
- Persistent storage of users, rooms, and permissions in MySQL
- Easy setup with XAMPP MySQL server
- Robust error handling and logging

---

## System Requirements

- .NET Framework or .NET Core compatible with C# 7.3
- MySQL database (XAMPP recommended)
- Windows / Linux / macOS (command line / terminal)

---

## Installation

1. **Prepare the Database**

   - Install XAMPP and start the MySQL server.
   - Create a database named `nettalk` and execute the provided SQL script (`database.sql`) to create necessary tables for users and rooms.

2. **Configuration**

   - In the `NetTalk.Shared` project folder, locate `appsettings.json` in the /bin/debug Folder.
   - Adjust the database connection string as needed (username, password, host).

3. **Build & Run**

   - Open the solution in Visual Studio or your preferred IDE.
   - Build the project with C# 7.3 compatibility.
   - Run the console application.

---

## Usage

- On startup, register a new user or log in.
- Join existing rooms or create new ones.
- Use chat commands within rooms to interact, e.g. `/leave`, `/active`, `/op`, etc.
- Operators have special privileges like managing bans.

---

## Chat Commands Overview

| Command         | Description                        |
| ----------------| -----------------------------------|
| `/leave`        | Leave the current chat room        |
| `/active`       | List all active users in the room  |
| `/rename [name]`| Rename the Room                    |
| `/kick [name]`  | Kick a User from u Rm              |
| `/op [name]`    | Grant operator rights to a user    |
| `/deop [name]`  | Revoke operator rights             |
| `/ban [name]`   | Ban a user from the room           |
| `/unban [name]` | Unban a previously banned user     |

---

## Project Structure

- **NetTalk.Shared**: Shared logic and data access (rooms, users, database)
- **NetTalk.Client**: Console application with user interface and networking
- **NetTalk.Server**: Server component managing connections

---

## Security

- Database connection uses a configurable connection string.
- Passwords are stored as hashes (assuming `password_hash` contains hashed passwords).
- SQL parameters are used to prevent SQL injection.

---

## Future Improvements

- TLS/SSL encryption for network communication
- Enhanced user interface (e.g., GUI)
- Additional chat commands and moderation tools
- Scalability improvements for larger user bases


---

## Contact

Feel free to reach out with questions or feedback.

---

**Enjoy using NetTalk!**
