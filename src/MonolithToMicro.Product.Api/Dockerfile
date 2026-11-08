
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/MonolithToMicro.Product.Api/MonolithToMicro.Product.Api.csproj \
     src/MonolithToMicro.Product.Api/

RUN dotnet restore \
    src/MonolithToMicro.Product.Api/MonolithToMicro.Product.Api.csproj

COPY src/MonolithToMicro.Product.Api/ \
     src/MonolithToMicro.Product.Api/

RUN dotnet publish \
    src/MonolithToMicro.Product.Api/MonolithToMicro.Product.Api.csproj \
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

ENTRYPOINT ["dotnet", "MonolithToMicro.Product.Api.dll"]
