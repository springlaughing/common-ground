# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Central build configuration first, so the restore layer caches until one of these
# (or a project manifest below) actually changes. `.editorconfig` is included so the
# in-container build honors the same analyzer rules as local/CI (e.g. the generated-
# migration exemptions) — without it, warnings-as-errors fails on generated code.
COPY global.json NuGet.config Directory.Build.props Directory.Packages.props .editorconfig ./

# Project manifests only (paths preserved). Restoring before copying the rest of the
# source keeps the package-restore layer cached across ordinary code changes.
COPY src/CommonGround.Api/CommonGround.Api.csproj                                 src/CommonGround.Api/
COPY src/CommonGround.SharedKernel/CommonGround.SharedKernel.csproj               src/CommonGround.SharedKernel/
COPY src/CommonGround.Modules.Questionnaires/CommonGround.Modules.Questionnaires.csproj src/CommonGround.Modules.Questionnaires/
COPY src/CommonGround.Modules.Responses/CommonGround.Modules.Responses.csproj     src/CommonGround.Modules.Responses/
COPY src/CommonGround.Modules.Reporting/CommonGround.Modules.Reporting.csproj     src/CommonGround.Modules.Reporting/
COPY src/CommonGround.Modules.Privacy/CommonGround.Modules.Privacy.csproj         src/CommonGround.Modules.Privacy/
COPY src/CommonGround.Modules.Audit/CommonGround.Modules.Audit.csproj             src/CommonGround.Modules.Audit/
COPY src/CommonGround.Modules.Comparisons/CommonGround.Modules.Comparisons.csproj src/CommonGround.Modules.Comparisons/

RUN dotnet restore src/CommonGround.Api/CommonGround.Api.csproj

# Full source, then publish (project references pull in the modules + SharedKernel).
COPY src/ src/
RUN dotnet publish src/CommonGround.Api/CommonGround.Api.csproj \
    -c Release -o /app --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

# The aspnet image ships a non-root `app` user and defaults Kestrel to port 8080.
# TLS is expected to be terminated by an upstream proxy in production.
EXPOSE 8080
USER app

ENTRYPOINT ["dotnet", "CommonGround.Api.dll"]
