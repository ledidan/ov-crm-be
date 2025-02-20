# Project Name

## Description
A CRM system built for SMEs, where each SME has an admin owner who can create sub-users. The system manages subscription periods, automatically signing out all users when the subscription expires until the admin owner renews it. It also includes real-time notifications for actions such as sharing contacts with other users.

## Technologies Used
- .NET Core
- C#
- MySQL
- Entity Framework Core
- SignalR (for real-time notifications)
- Other relevant libraries and tools

## Prerequisites
Before running the project, ensure you have the following installed:
- [.NET SDK](https://dotnet.microsoft.com/download)
- [MySQL Server](https://dev.mysql.com/downloads/)
- [Visual Studio](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)

## Installation
1. Clone the repository:
   ```sh
   git clone https://github.com/yourusername/yourproject.git
   cd yourproject
   ```
2. Install dependencies:
   ```sh
   dotnet restore
   ```
3. Configure the database connection in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=yourserver;Database=yourdb;User Id=youruser;Password=yourpassword;"
     }
   }
   ```
4. Run database migrations (if applicable):
   ```sh
   dotnet ef database update
   ```
5. Build and run the application:
   ```sh
   dotnet run
   ```

## Usage
The CRM provides the following key functionalities:
- Business admin owners can create and manage sub-users.
- Subscription management, ensuring access is revoked upon expiry.
- Real-time notifications for events like contact sharing.

## Deployment
Instructions on how to deploy the application, such as:
- Using Docker
- Hosting on Azure/AWS
- Publishing as a standalone executable

## Contributing
1. Fork the repository.
2. Create a new branch (`git checkout -b feature-name`).
3. Commit your changes (`git commit -m 'Add new feature'`).
4. Push to the branch (`git push origin feature-name`).
5. Open a Pull Request.

## License
Specify the license (e.g., MIT, Apache-2.0) and include a link to the `LICENSE` file.

## Contact
For issues or inquiries, contact:
- **Your Name**
- Email: danle.ov.software@gmail.com
- GitHub: [ledidan](https://github.com/ledidan)

