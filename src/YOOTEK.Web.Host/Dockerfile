#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
RUN apt update && apt install libgdiplus -y

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["IMAX.Web.Host/IMAX.Web.Host.csproj", "IMAX.Web.Host/"]
COPY ["IMAX.Core/IMAX.Core.csproj", "IMAX.Core/"]
COPY ["IMAX.Web.Core/IMAX.Web.Core.csproj", "IMAX.Web.Core/"]
COPY ["IMAX.EntityFrameworkCore/IMAX.EntityFrameworkCore.csproj", "IMAX.EntityFrameworkCore/"]
COPY ["IMAX.Application/IMAX.Application.csproj", "IMAX.Application/"]
RUN dotnet restore "IMAX.Web.Host/IMAX.Web.Host.csproj"
COPY . .
WORKDIR "/src/IMAX.Web.Host"
RUN dotnet build "IMAX.Web.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IMAX.Web.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IMAX.Web.Host.dll"]