# 🎨 ArtPlatform Microservices Project

A microservices-based ASP.NET Web API platform built for managing users, artists, critiques, subscriptions, portfolios, and artworks. This project helped me master **RabbitMQ** message queues in a distributed system with independent services, shared logic, and resilient communication.

---

## 🛠️ Technologies Used

- **.NET 8 Web API**
- **Entity Framework Core**
- **FluentValidation**
- **JWT Authentication** (Custom implementation)
- **AutoMapper**
- **RabbitMQ** (required to run project)
- **SMTP** (Email Notifications)
- **BCrypt** (Password hashing)
- **Docker** *(optional for RabbitMQ setup)*

---

## 📁 Project Structure

```
ArtPlatform/
├── src/
│   ├── AuthService/
│   ├── UserService/
│   ├── CategoryService/
│   ├── ArtworkService/
│   ├── CritiqueService/
│   ├── PortfolioService/
│   └── SubscriptionService/
└── shared/
    ├── CommonUtils/          # JWT config, SMTP logic, shared utilities
    └── Contracts/            # Event DTOs, Enums, Messaging Contracts
```

### Service Architecture

Each service is independent and contains:
- Its own `DbContext`, `Models`, `DTOs`, `Controllers`, `Validators`
- A dedicated `RabbitMQ` folder for **Consumers** and **Publishers**
- Isolated database configuration

---

## 🚀 Getting Started

### Prerequisites

- [.NET SDK 8+](https://dotnet.microsoft.com/en-us/download)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/)
- [RabbitMQ Server](https://www.rabbitmq.com/download.html) – **must be running before starting the services**

### Setup Steps

1. **Clone the Repository**
   ```bash
   git clone https://github.com/yourusername/artplatform.git
   cd artplatform
   ```

2. **Open Solution in Visual Studio**
   - Open `ArtPlatform.sln` in Visual Studio 2022+

3. **Set Startup Projects**
   - Right-click on the Solution → **Set Startup Projects...**
   - Choose **Multiple startup projects**
   - Set all services (`ArtworkService`, `AuthService`, `UserService`, `CategoryService`, `CritiqueService`, `PortfolioService`, `SubscriptionService`) to **Start**

4. **Configure SMTP Credentials**
   - Contact me for the SMTP app password and update in the relevant service (likely `AuthService` or `UserService`)

5. **Start RabbitMQ Server**
   - Ensure RabbitMQ is running on your machine
   - Access management interface at `http://localhost:15672` (default: guest/guest)

6. **Run the Solution**
   - Press **F5** or click **Start** in Visual Studio
   - All services will start simultaneously

---

## 📡 RabbitMQ Messaging

Each service communicates via RabbitMQ event queues, enabling asynchronous event-driven interactions. 

### Example Flow:
- `UserService` publishes `UserCreatedEvent`
- `SubscriptionService` consumes it and reacts accordingly
- All messaging contracts are in the `shared/Contracts` project to keep event DTOs consistent

### Key Benefits:
- **Decoupled communication** between services
- **Resilient message delivery**
- **Scalable event-driven architecture**

---

## 🏗️ Development Notes

This project is intentionally split into **7 microservices** to explore and learn:

- ✅ **Service independence**
- ✅ **RabbitMQ messaging**
- ✅ **Resiliency and modular deployment**
- ✅ **Event-driven design patterns**

Although the app isn't massive, its structure simulates real-world distributed systems and provides hands-on experience with microservices architecture.

---

## 📚 What I Learned

This project gave me hands-on experience with:
- **RabbitMQ** message queues and event-driven communication
- **Microservices architecture** and service independence
- **DTO separation** and validation across services
- **Event-driven design** in .NET applications
- **Distributed system** challenges and solutions

---

## 🔧 Configuration

### Database Configuration
Each service maintains its own database context and migrations. Update connection strings in `appsettings.json` for each service as needed.

### JWT Configuration
JWT settings are centralized in the `CommonUtils` shared project for consistency across services.


---

## 📝 API Documentation

Once running, each service exposes its own Swagger documentation:
- AuthService: `https://localhost:port/swagger`
- UserService: `https://localhost:port/swagger`
- ArtworkService: `https://localhost:port/swagger`
- (and so on for each service)
- Postman Collection
- POSTMAN LINK: https://documenter.getpostman.com/view/44023225/2sB34fmgTs

---

*This project demonstrates microservices architecture with RabbitMQ messaging in a real-world-like distributed system built with .NET 8.*
