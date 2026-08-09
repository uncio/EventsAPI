# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# copy everything else and build app
WORKDIR /source
COPY RU.Uncio.BookingService.Domain/. ./RU.Uncio.BookingService.Domain/

WORKDIR /source
COPY RU.Uncio.BookingService.Application/. ./RU.Uncio.BookingService.Application/

WORKDIR /source
COPY RU.Uncio.BookingService.Infrastructure/. ./RU.Uncio.BookingService.Infrastructure/

# copy csproj and restore as distinct layers
COPY RU.Uncio.BookingService.Presentation/*.csproj ./RU.Uncio.BookingService.Presentation/
WORKDIR /source/RU.Uncio.BookingService.Presentation
RUN dotnet restore

# copy everything else and build app
WORKDIR /source
COPY RU.Uncio.BookingService.Presentation/. ./RU.Uncio.BookingService.Presentation/

WORKDIR /source/RU.Uncio.BookingService.Presentation
RUN dotnet publish -c release -o /app --no-restore