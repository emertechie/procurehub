FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ProcureHub.BlazorApp/ProcureHub.BlazorApp.csproj", "ProcureHub.BlazorApp/"]
RUN dotnet restore "ProcureHub.BlazorApp/ProcureHub.BlazorApp.csproj"
COPY . .
WORKDIR "/src/ProcureHub.BlazorApp"
RUN dotnet build "./ProcureHub.BlazorApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ProcureHub.BlazorApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProcureHub.BlazorApp.dll"]
