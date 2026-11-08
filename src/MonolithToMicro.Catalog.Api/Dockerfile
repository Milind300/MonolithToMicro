FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/MonolithToMicro.Catalog.Api/MonolithToMicro.Catalog.Api.csproj src/MonolithToMicro.Catalog.Api/

RUN dotnet restore "src/MonolithToMicro.Catalog.Api/MonolithToMicro.Catalog.Api.csproj"

COPY src/MonolithToMicro.Catalog.Api/ src/MonolithToMicro.Catalog.Api/

RUN dotnet publish "src/MonolithToMicro.Catalog.Api/MonolithToMicro.Catalog.Api.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MonolithToMicro.Catalog.Api.dll"]