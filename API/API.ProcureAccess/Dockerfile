# SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
#
# SPDX-License-Identifier: MIT

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, from the build settings and project files only, so the restore layer stays
# cached until a package or a project reference changes. Restoring the API restores every
# project it references. A new project under DAL/, SERVICES/ or src/ needs its line here,
# otherwise publish stops with "assets file not found" (NETSDK1004).
COPY NuGet.config Directory.Build.props ./
COPY src/Directory.Build.props src/
COPY API/API.ProcureAccess/API.ProcureAccess.csproj API/API.ProcureAccess/
COPY DAL/DAL.ProcureAccess/DAL.ProcureAccess.csproj DAL/DAL.ProcureAccess/
COPY DAL/MODELS.ProcureAccess/MODELS.ProcureAccess.csproj DAL/MODELS.ProcureAccess/
COPY SERVICES/SERVICES.ProcureAccess/SERVICES.ProcureAccess.csproj SERVICES/SERVICES.ProcureAccess/
# module projects (ARCHITECTURE §4.3)
COPY src/ProcureAccess.Modules.Catalog/ProcureAccess.Modules.Catalog.csproj src/ProcureAccess.Modules.Catalog/
COPY src/ProcureAccess.Modules.Catalog.Contracts/ProcureAccess.Modules.Catalog.Contracts.csproj src/ProcureAccess.Modules.Catalog.Contracts/
COPY src/ProcureAccess.Modules.Configurator/ProcureAccess.Modules.Configurator.csproj src/ProcureAccess.Modules.Configurator/
COPY src/ProcureAccess.Modules.Configurator.Contracts/ProcureAccess.Modules.Configurator.Contracts.csproj src/ProcureAccess.Modules.Configurator.Contracts/
COPY src/ProcureAccess.Modules.Directory/ProcureAccess.Modules.Directory.csproj src/ProcureAccess.Modules.Directory/
COPY src/ProcureAccess.Modules.Directory.Contracts/ProcureAccess.Modules.Directory.Contracts.csproj src/ProcureAccess.Modules.Directory.Contracts/
COPY src/ProcureAccess.Modules.Export/ProcureAccess.Modules.Export.csproj src/ProcureAccess.Modules.Export/
COPY src/ProcureAccess.Modules.Export.Contracts/ProcureAccess.Modules.Export.Contracts.csproj src/ProcureAccess.Modules.Export.Contracts/
COPY src/ProcureAccess.Modules.IdentityAccess/ProcureAccess.Modules.IdentityAccess.csproj src/ProcureAccess.Modules.IdentityAccess/
COPY src/ProcureAccess.Modules.IdentityAccess.Contracts/ProcureAccess.Modules.IdentityAccess.Contracts.csproj src/ProcureAccess.Modules.IdentityAccess.Contracts/
COPY src/ProcureAccess.Modules.Moderation/ProcureAccess.Modules.Moderation.csproj src/ProcureAccess.Modules.Moderation/
COPY src/ProcureAccess.Modules.Moderation.Contracts/ProcureAccess.Modules.Moderation.Contracts.csproj src/ProcureAccess.Modules.Moderation.Contracts/
COPY src/ProcureAccess.Modules.Platform/ProcureAccess.Modules.Platform.csproj src/ProcureAccess.Modules.Platform/
COPY src/ProcureAccess.Modules.Platform.Contracts/ProcureAccess.Modules.Platform.Contracts.csproj src/ProcureAccess.Modules.Platform.Contracts/
COPY src/ProcureAccess.Modules.Workspace/ProcureAccess.Modules.Workspace.csproj src/ProcureAccess.Modules.Workspace/
COPY src/ProcureAccess.Modules.Workspace.Contracts/ProcureAccess.Modules.Workspace.Contracts.csproj src/ProcureAccess.Modules.Workspace.Contracts/
RUN dotnet restore API/API.ProcureAccess/API.ProcureAccess.csproj

# then the sources
COPY DAL/ DAL/
COPY SERVICES/ SERVICES/
COPY API/ API/
COPY src/ src/
RUN dotnet publish API/API.ProcureAccess/API.ProcureAccess.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# Licence notices of the shipped NuGet packages (#187, S4-27); scripts/check-api-notices.mjs keeps
# the file in step with the packages. The runtime's own are in /usr/share/dotnet of the base image.
COPY API/API.ProcureAccess/THIRD-PARTY-NOTICES.txt ./
# Catalog release folders for `catalog import <version>` (S3-16 decision 2): the reviewed folders
# ship with the image; Catalog:ReleasesPath (catalog/releases) is relative to /app.
COPY catalog/releases/ catalog/releases/
# The base image listens on port 8080 (ASPNETCORE_HTTP_PORTS) and ships the non-root user "app".
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "API.ProcureAccess.dll"]
