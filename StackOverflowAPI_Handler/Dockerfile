# Use the ASP.NET Core runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["StackOverflowAPI_Handler.csproj", "./"]
RUN dotnet restore "./StackOverflowAPI_Handler.csproj"

# Copy the rest of your application's source code and build it
COPY . .
RUN dotnet build "StackOverflowAPI_Handler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StackOverflowAPI_Handler.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Copy the published application and the database file to the runtime image and set the entry point
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY tags.db ./   
ENTRYPOINT ["dotnet", "StackOverflowAPI_Handler.dll"]
