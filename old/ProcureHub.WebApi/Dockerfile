FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ProcureHub.WebApi/ProcureHub.WebApi.csproj", "ProcureHub.WebApi/"]
COPY ["ProcureHub/ProcureHub.csproj", "ProcureHub/"]
RUN dotnet restore "ProcureHub.WebApi/ProcureHub.WebApi.csproj"
COPY . .
WORKDIR "/src/ProcureHub.WebApi"
RUN dotnet build "./ProcureHub.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ProcureHub.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProcureHub.WebApi.dll"]
