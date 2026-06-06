# syntax=docker/dockerfile:1
# Build context is the repository root (see docker-compose.yml / render.yaml), so this
# single image can build the React SPA and the .NET API and serve them same-origin.

# ---- Frontend build (React SPA) ----
FROM node:22-alpine AS frontend
WORKDIR /web
COPY frontend/package.json frontend/package-lock.json ./
# --ignore-scripts: don't run package lifecycle scripts during install (S6505). Vite's
# native deps (esbuild/rollup) ship as platform packages, so the build still works.
RUN npm ci --ignore-scripts
COPY frontend/ ./
RUN npm run build

# ---- Backend build (.NET API) ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Central build configuration first, so the restore layer caches until one of these
# (or a project manifest below) changes. `.editorconfig` is included so the in-container
# build honors the same analyzer rules as local/CI (e.g. generated-migration exemptions).
COPY backend/global.json backend/NuGet.config backend/Directory.Build.props backend/Directory.Packages.props backend/.editorconfig ./

# Project manifests only (paths preserved). Restoring before copying the rest of the
# source keeps the package-restore layer cached across ordinary code changes.
COPY backend/src/CommonGround.Api/CommonGround.Api.csproj                                 src/CommonGround.Api/
COPY backend/src/CommonGround.SharedKernel/CommonGround.SharedKernel.csproj               src/CommonGround.SharedKernel/
COPY backend/src/CommonGround.Modules.Questionnaires/CommonGround.Modules.Questionnaires.csproj src/CommonGround.Modules.Questionnaires/
COPY backend/src/CommonGround.Modules.Responses/CommonGround.Modules.Responses.csproj     src/CommonGround.Modules.Responses/
COPY backend/src/CommonGround.Modules.Reporting/CommonGround.Modules.Reporting.csproj     src/CommonGround.Modules.Reporting/
COPY backend/src/CommonGround.Modules.Privacy/CommonGround.Modules.Privacy.csproj         src/CommonGround.Modules.Privacy/
COPY backend/src/CommonGround.Modules.Audit/CommonGround.Modules.Audit.csproj             src/CommonGround.Modules.Audit/
COPY backend/src/CommonGround.Modules.Comparisons/CommonGround.Modules.Comparisons.csproj src/CommonGround.Modules.Comparisons/

RUN dotnet restore src/CommonGround.Api/CommonGround.Api.csproj

# Full source, then publish (project references pull in the modules + SharedKernel).
COPY backend/src/ src/
RUN dotnet publish src/CommonGround.Api/CommonGround.Api.csproj \
    -c Release -o /app --no-restore

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
# The SPA is served as static files by the API → one origin, so the cg_session cookie
# (SameSite=Strict) stays first-party.
COPY --from=frontend /web/dist ./wwwroot

# The aspnet image ships a non-root `app` user and defaults Kestrel to port 8080.
# TLS is terminated by the platform proxy (see ForwardedHeaders in Program.cs).
EXPOSE 8080
USER app

# Bind Kestrel to the port the platform injects (Render/Cloud Run set $PORT) via
# ASPNETCORE_HTTP_PORTS, falling back to 8080 locally. No http:// literal (TLS is
# terminated upstream). `exec` keeps dotnet as PID 1 so SIGTERM → graceful shutdown.
ENTRYPOINT ["sh", "-c", "export ASPNETCORE_HTTP_PORTS=${PORT:-8080}; exec dotnet CommonGround.Api.dll"]
