FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy files
COPY DAL/ DAL/
COPY SERVICES/ SERVICES/
COPY API/ API/

# restore
RUN dotnet restore DAL/DAL.ProcureAccess/DAL.ProcureAccess.csproj
RUN dotnet restore DAL/MODELS.ProcureAccess/MODELS.ProcureAccess.csproj
RUN dotnet restore SERVICES/SERVICES.ProcureAccess/SERVICES.ProcureAccess.csproj
RUN dotnet restore API/API.ProcureAccess/API.ProcureAccess.csproj

WORKDIR /src/API/API.ProcureAccess
RUN dotnet publish -c Debug -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "API.ProcureAccess.dll"]

