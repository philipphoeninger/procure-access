FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DAL/MODELS.ProcureAccess/MODELS.ProcureAccess.csproj DAL/MODELS.ProcureAccess/
COPY DAL/DAL.ProcureAccess/DAL.ProcureAccess.csproj DAL/DAL.ProcureAccess/
COPY SERVICES/SERVICES.ProcureAccess/SERVICES.ProcureAccess.csproj SERVICES/SERVICES.ProcureAccess/

RUN dotnet restore SERVICES/SERVICES.ProcureAccess/SERVICES.ProcureAccess.csproj

COPY . .

WORKDIR /src/SERVICES/SERVICES.ProcureAccess
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SERVICES.ProcureAccess.dll"]

