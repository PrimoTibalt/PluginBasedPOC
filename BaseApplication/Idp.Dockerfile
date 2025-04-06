FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

EXPOSE 5001

COPY . .

RUN dotnet restore IdProvider/IdProvider.csproj
RUN dotnet publish "IdProvider/IdProvider.csproj" -c release -o /src

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS run
WORKDIR /src
COPY --from=build /src ./
COPY https-cert.pfx ./

RUN useradd -m -r -u 1000 appuser
RUN chown -R appuser:appuser /src
USER appuser

ENV ASPNETCORE_Kestrel__Certificates__Default__Path=https-cert.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=185411977613522415318718721014889747121821834242591101392283311182248254923726166159
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV ASPNETCORE_URLS="https://+:5001"
ENTRYPOINT [ "dotnet", "IdProvider.dll" ]


