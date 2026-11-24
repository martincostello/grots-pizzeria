# Build context: the repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:11.0@sha256:2794448a07118a6eaf00f29ab5d2bf44928517ce5ff86505a63a69230374f599 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props .editorconfig GrotsPizzeria.ruleset stylecop.json ./
COPY src/GrotsPizzeria.Shared/GrotsPizzeria.Shared.csproj src/GrotsPizzeria.Shared/
COPY src/GrotsPizzeria.Client/GrotsPizzeria.Client.csproj src/GrotsPizzeria.Client/
COPY src/GrotsPizzeria.Server/GrotsPizzeria.Server.csproj src/GrotsPizzeria.Server/
RUN dotnet restore src/GrotsPizzeria.Server/GrotsPizzeria.Server.csproj

COPY src/ src/
RUN dotnet publish src/GrotsPizzeria.Server/GrotsPizzeria.Server.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:11.0@sha256:ca396ece3f2a2e62c7a03f1d7d710b8bc7e8c9dea5597d091178d9f5bfca13f0
WORKDIR /app

RUN mkdir /data && chown app:app /data
VOLUME /data

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080 \
    ConnectionStrings__Users="Data Source=/data/grots.db" \
    DataProtection__KeysPath=/data/keys \
    OTEL_SERVICE_NAME=grots-web

USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "GrotsPizzeria.Server.dll"]
