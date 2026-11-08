
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/MonolithToMicro.Order.Api/MonolithToMicro.Order.Api.csproj \
     src/MonolithToMicro.Order.Api/

RUN dotnet restore \
    src/MonolithToMicro.Order.Api/MonolithToMicro.Order.Api.csproj

COPY src/MonolithToMicro.Order.Api/ \
     src/MonolithToMicro.Order.Api/

RUN dotnet publish \
    src/MonolithToMicro.Order.Api/MonolithToMicro.Order.Api.csproj \
    -c Release \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "MonolithToMicro.Order.Api.dll"]
