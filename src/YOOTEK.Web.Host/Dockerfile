FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt update && apt install libgdiplus -y
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["YOOTEK.Web.Host/YOOTEK.Web.Host.csproj", "YOOTEK.Web.Host/"]
COPY ["YOOTEK.Core/YOOTEK.Core.csproj", "YOOTEK.Core/"]
COPY ["YOOTEK.Web.Core/YOOTEK.Web.Core.csproj", "YOOTEK.Web.Core/"]
COPY ["YOOTEK.EntityFrameworkCore/YOOTEK.EntityFrameworkCore.csproj", "YOOTEK.EntityFrameworkCore/"]
COPY ["YOOTEK.Application/YOOTEK.Application.csproj", "YOOTEK.Application/"]
RUN dotnet restore "YOOTEK.Web.Host/YOOTEK.Web.Host.csproj"
COPY . .
WORKDIR "/src/YOOTEK.Web.Host"
RUN dotnet build "YOOTEK.Web.Host.csproj" -c Release -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "YOOTEK.Web.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YOOTEK.Web.Host.dll"]
