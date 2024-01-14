#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
RUN apt update && apt install libgdiplus -y

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Yootek.Web.Host/Yootek.Web.Host.csproj", "Yootek.Web.Host/"]
COPY ["Yootek.Core/Yootek.Core.csproj", "Yootek.Core/"]
COPY ["Yootek.Web.Core/Yootek.Web.Core.csproj", "Yootek.Web.Core/"]
COPY ["Yootek.EntityFrameworkCore/Yootek.EntityFrameworkCore.csproj", "Yootek.EntityFrameworkCore/"]
COPY ["Yootek.Application/Yootek.Application.csproj", "Yootek.Application/"]
RUN dotnet restore "Yootek.Web.Host/Yootek.Web.Host.csproj"
COPY . .
WORKDIR "/src/Yootek.Web.Host"
RUN dotnet build "Yootek.Web.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Yootek.Web.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yootek.Web.Host.dll"]