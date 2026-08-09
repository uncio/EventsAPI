# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# copy everything else and build app
WORKDIR /source
COPY RU.Uncio.UserService.Domain/. ./RU.Uncio.UserService.Domain/

WORKDIR /source
COPY RU.Uncio.UserService.Application/. ./RU.Uncio.UserService.Application/

WORKDIR /source
COPY RU.Uncio.UserService.Infrastructure/. ./RU.Uncio.UserService.Infrastructure/

# copy csproj and restore as distinct layers
COPY RU.Uncio.UserService.Presentation/*.csproj ./RU.Uncio.UserService.Presentation/
WORKDIR /source/RU.Uncio.UserService.Presentation
RUN dotnet restore

# copy everything else and build app
WORKDIR /source
COPY RU.Uncio.UserService.Presentation/. ./RU.Uncio.UserService.Presentation/

WORKDIR /source/RU.Uncio.UserService.Presentation
RUN dotnet publish -c release -o /app --no-restore