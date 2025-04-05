# Location Tracking Application

## Project Overview

This is a full-stack location tracking application built with React Native for the mobile frontend and .NET Core for the backend. The application allows users to create, view, update, and manage locations with role-based access control.

## Features

### User Features

- Create new locations
- View personal locations
- Update unverified locations
- View verified locations

### Admin Features

- Verify locations
- Delete locations
- View all locations
- Manage user-created locations

### Backend

- .NET Core Web API
- Entity Framework Core
- C#
- MYSQL Server

## Prerequisites

### Frontend

- Node.js (v16+)
- npm or yarn
- React Native CLI
- Expo

### Backend

- .NET 8 SDK
- MySql Server
- Visual Studio or VS Code

## Installation

### Backend Setup

1. Clone the repository

```bash
git clone https://github.com/yourusername/your-project-name.git
```

2. Navigate to backend directory

```bash
cd WSFBackendApi
```

3. Restore NuGet packages

```bash
dotnet restore
```

4. Update database connection string in `appsettings.json`

5. Apply database migrations

```bash
dotnet ef database update
```

6. Run the backend

```bash
dotnet run
```

## Environment Configuration

### Backend (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database='database_name';User=your_username;Password=your_password;"
  },
  "Jwt": {
    "Key": "your_secret_key",
    "Issuer": "your_issuer",
    "Audience": "your_audience"
  }
}
```

## Database Schema

### Users Table

- Id (Guid)
- Username
- Email
- Role
- Password (hashed)

### Locations Table

- Id (Guid)
- Name
- Description
- Latitude
- Longitude
- Address
- UserId (Foreign Key)
- IsActive
- IsVerified
- CreatedAt

## Deployment

### Backend

- Deploy to Azure App Service
- Configure connection strings
- Set up CI/CD pipeline

## Troubleshooting

- Ensure all connection strings are correct
- Check network permissions
- Verify API endpoint configurations

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

Distributed under the MIT License. See `LICENSE` for more information.

## Contact

Your Name - Mr. Blaq.
