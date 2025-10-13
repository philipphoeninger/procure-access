FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj files
COPY DAL/MODELS.ProcureAccess/MODELS.ProcureAccess.csproj DAL/MODELS.ProcureAccess/
COPY DAL/DAL.ProcureAccess/DAL.ProcureAccess.csproj DAL/DAL.ProcureAccess/
COPY SERVICES/SERVICES.ProcureAccess/SERVICES.ProcureAccess.csproj SERVICES/SERVICES.ProcureAccess/
COPY API/API.ProcureAccess/API.ProcureAccess.csproj API/API.ProcureAccess/

# restore
RUN dotnet restore API/API.ProcureAccess/API.ProcureAccess.csproj

# copy the rest
COPY . .

WORKDIR /src/API/API.ProcureAccess
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "API.ProcureAccess.dll"]

