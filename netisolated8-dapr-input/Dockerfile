FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG PROJECT_NAME=netisolated8-dapr-input
WORKDIR /src
COPY ["$PROJECT_NAME/$PROJECT_NAME.csproj", "$PROJECT_NAME/"]
RUN dotnet add "./$PROJECT_NAME/$PROJECT_NAME.csproj" package Microsoft.Azure.WebJobs.Extensions.Dapr --version 1.0.0
RUN dotnet remove "./$PROJECT_NAME/$PROJECT_NAME.csproj" package Microsoft.Azure.WebJobs.Extensions.Dapr
RUN ln -s $HOME/.nuget/packages/microsoft.azure.webjobs.extensions.dapr/1.0.0 $HOME/.nuget/packages/microsoft.azure.webjobs.extensions.dapr/99.99.99
RUN dotnet restore "./$PROJECT_NAME/$PROJECT_NAME.csproj"
RUN ln -s Microsoft.Azure.Functions.Worker.Extensions.Dapr.dll $HOME/.nuget/packages/microsoft.azure.functions.worker.extensions.dapr/1.0.0/lib/netstandard2.0/Microsoft.Azure.Functions.Worker.Extensions.Dapr.1.dll
COPY . .
WORKDIR "/src/$PROJECT_NAME"
RUN dotnet build "./$PROJECT_NAME.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG PROJECT_NAME=netisolated8-dapr-input
RUN dotnet publish "./$PROJECT_NAME.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true