FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/SubscriptionService/SubscriptionService.csproj", "src/SubscriptionService/"]
COPY ["shared/Contracts/Contracts.csproj", "shared/Contracts/"]
COPY ["shared/CommonUtils/CommonUtils.csproj", "shared/CommonUtils/"]

RUN dotnet restore "src/SubscriptionService/SubscriptionService.csproj"

COPY . .

WORKDIR /src/src/SubscriptionService

RUN dotnet build SubscriptionService.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish SubscriptionService.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SubscriptionService.dll"]
