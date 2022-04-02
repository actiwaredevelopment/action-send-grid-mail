# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0-bullseye-slim-amd64 as base

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim-amd64 AS build

WORKDIR /src
COPY . .
RUN dotnet restore "src/action.send-grid/action.send-grid.csproj"
COPY . .

RUN dotnet build "src/action.send-grid/action.send-grid.csproj" -c Release -r linux-x64 --self-contained false -o /app --force

FROM build AS publish
RUN dotnet publish "src/action.send-grid/action.send-grid.csproj" -c Release -r linux-x64 --self-contained false -o /github/workspace/app --force

FROM base AS final

COPY --from=publish /github/workspace/app /app

WORKDIR /app

ENTRYPOINT ["dotnet", "/app/actiware-send-grid.dll"]