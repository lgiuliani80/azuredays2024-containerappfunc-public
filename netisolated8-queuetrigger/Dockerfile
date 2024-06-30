FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG PROJECT_NAME=netisolated8-queuetrigger
WORKDIR /src
COPY ["$PROJECT_NAME/$PROJECT_NAME.csproj", "$PROJECT_NAME/"]
RUN dotnet restore "./$PROJECT_NAME/$PROJECT_NAME.csproj"
COPY . .
WORKDIR "/src/$PROJECT_NAME"
RUN dotnet build "./$PROJECT_NAME.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG PROJECT_NAME=netisolated8-queuetrigger
RUN dotnet publish "./$PROJECT_NAME.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true