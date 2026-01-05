FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Consumer/Consumer.csproj ./Consumer/
RUN dotnet restore ./Consumer/Consumer.csproj

COPY src/Consumer/ ./Consumer/
RUN dotnet build ./Consumer/Consumer.csproj -c Release -o /app/build
RUN dotnet publish ./Consumer/Consumer.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Consumer.dll"]

