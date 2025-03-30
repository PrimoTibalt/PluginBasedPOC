FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

EXPOSE 8080

WORKDIR /app

COPY *.sln .
COPY BaseLibrary/BaseLibrary.csproj BaseLibrary/BaseLibrary.csproj
COPY PluginLoader/PluginLoader.csproj PluginLoader/PluginLoader.csproj
COPY WebApp/WebApp.csproj WebApp/WebApp.csproj
RUN dotnet restore

COPY . .
RUN dotnet publish -c release -o /src

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS run
WORKDIR /src

COPY --from=build /src ./

ENTRYPOINT [ "dotnet", "WebApp.dll" ]