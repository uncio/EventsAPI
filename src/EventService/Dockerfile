# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# copy everything else and build app
WORKDIR /source
COPY RU.Uncio.EventService.Domain/. ./RU.Uncio.EventService.Domain/

WORKDIR /source
COPY RU.Uncio.EventService.Application/. ./RU.Uncio.EventService.Application/

WORKDIR /source
COPY RU.Uncio.EventsService.Infrastructure/. ./RU.Uncio.EventsService.Infrastructure/

# copy csproj and restore as distinct layers
COPY RU.Uncio.EventService.Presentation/*.csproj ./RU.Uncio.EventService.Presentation/
WORKDIR /source/RU.Uncio.EventService.Presentation
RUN dotnet restore

# copy everything else and build app
WORKDIR /source
COPY RU.Uncio.EventService.Presentation/. ./RU.Uncio.EventService.Presentation/

WORKDIR /source/RU.Uncio.EventService.Presentation
RUN dotnet publish -c release -o /app --no-restore