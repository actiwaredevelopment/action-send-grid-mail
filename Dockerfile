# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0 as base

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src
COPY . .
RUN dotnet restore "src/action.send-grid/action.send-grid.csproj"
COPY . .

RUN dotnet build "src/action.send-grid/action.send-grid.csproj" -c Release -o /app -nologo -clp:nosummary -v:m

FROM build AS publish
RUN dotnet publish "src/action.send-grid/action.send-grid.csproj" -c Release -o /github/workspace/app

FROM base AS final

COPY --from=publish /github/workspace/app /app

WORKDIR /app

ENTRYPOINT ["dotnet", "/app/actiware-send-grid.dll"]