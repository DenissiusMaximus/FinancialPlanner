FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Domain/FinancialPlanner.Domain.csproj src/Domain/
COPY src/Application/FinancialPlanner.Application.csproj src/Application/
COPY src/Infrastructure/FinancialPlanner.Infrastructure.csproj src/Infrastructure/
COPY src/Api/FinancialPlanner.Api.csproj src/Api/
RUN dotnet restore src/Api/FinancialPlanner.Api.csproj

COPY src/ src/
RUN dotnet publish src/Api/FinancialPlanner.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FinancialPlanner.Api.dll"]
