# =========================================================
# Build Arguments
# =========================================================
ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:10.0
ARG RUNTIME_IMAGE=mcr.microsoft.com/dotnet/runtime:10.0

# =========================================================
# Build Stage
# =========================================================
FROM ${SDK_IMAGE} AS build

ARG BUILD_CONFIGURATION=Release
ARG ASPNETCORE_ENVIRONMENT=Staging

WORKDIR /src

RUN if [ -z "$ASPNETCORE_ENVIRONMENT" ]; then \
        echo "Error: ASPNETCORE_ENVIRONMENT is not set"; \
        exit 1; \
    fi

# =========================================================
# Project files
# =========================================================

# Host
COPY ["src/Hosts/Worker/CoreQuant.Worker/CoreQuant.Worker.csproj", "src/Hosts/Worker/CoreQuant.Worker/"]

# Common Host
COPY ["src/Common/Hosts/Common.Worker/Common.Worker.csproj", "src/Common/Hosts/Common.Worker/"]

# Application Layer
COPY ["src/Application/CoreQuant.Application/CoreQuant.Application.csproj", "src/Application/CoreQuant.Application/"]
COPY ["src/Application/CoreQuant.Application.Contracts/CoreQuant.Application.Contracts.csproj", "src/Application/CoreQuant.Application.Contracts/"]

# Domain Layer
COPY ["src/Domain/CoreQuant.Domain/CoreQuant.Domain.csproj", "src/Domain/CoreQuant.Domain/"]

# Infrastructure Layer
COPY ["src/Infrastructure/CoreQuant.Infrastructure/CoreQuant.Infrastructure.csproj", "src/Infrastructure/CoreQuant.Infrastructure/"]
COPY ["src/Infrastructure/CoreQuant.Infrastructure.Acl/CoreQuant.Infrastructure.Acl.csproj", "src/Infrastructure/CoreQuant.Infrastructure.Acl/"]
COPY ["src/Infrastructure/CoreQuant.Infrastructure.Persistence.EF/CoreQuant.Infrastructure.Persistence.EF.csproj", "src/Infrastructure/CoreQuant.Infrastructure.Persistence.EF/"]

# Common Layer
COPY ["src/Common/Common.Application/Common.Application.csproj", "src/Common/Common.Application/"]
COPY ["src/Common/Common.Domain/Common.Domain.csproj", "src/Common/Common.Domain/"]
COPY ["src/Common/Common.Framework/Common.Framework.csproj", "src/Common/Common.Framework/"]
COPY ["src/Common/Infrastructure/Common.Infrastructure/Common.Infrastructure.csproj", "src/Common/Infrastructure/Common.Infrastructure/"]
COPY ["src/Common/Infrastructure/Common.Infrastructure.Persistence/Common.Infrastructure.Persistence.csproj", "src/Common/Infrastructure/Common.Infrastructure.Persistence/"]
COPY ["src/Common/Infrastructure/Common.Infrastructure.Persistence.EF/Common.Infrastructure.Persistence.EF.csproj", "src/Common/Infrastructure/Common.Infrastructure.Persistence.EF/"]

# Framework Layer
COPY ["src/Framework/CoreQuant.Framework/CoreQuant.Framework.csproj", "src/Framework/CoreQuant.Framework/"]


# =========================================================
# NuGet configuration
# =========================================================
COPY nuget.${ASPNETCORE_ENVIRONMENT}.config ./nuget.config

# =========================================================
# Restore
# =========================================================
RUN dotnet restore \
    src/Hosts/Worker/CoreQuant.Worker/CoreQuant.Worker.csproj \
    --configfile ./nuget.config

# =========================================================
# Source
# =========================================================
COPY src/ ./src/

WORKDIR /src/src/Hosts/Worker/CoreQuant.Worker

# =========================================================
# Build
# =========================================================
RUN dotnet build CoreQuant.Worker.csproj \
    -c ${BUILD_CONFIGURATION} \
    --no-restore

# =========================================================
# Publish
# =========================================================
RUN dotnet publish CoreQuant.Worker.csproj \
    -c ${BUILD_CONFIGURATION} \
    --no-build \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false

# =========================================================
# Runtime Stage
# =========================================================
FROM ${RUNTIME_IMAGE} AS final

ARG ASPNETCORE_ENVIRONMENT=Staging

WORKDIR /app

# =========================================================
# Runtime environment
# =========================================================
ENV TZ=Asia/Tehran
ENV DOTNET_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}



# =========================================================
# Application
# =========================================================
COPY --from=build /app/publish .

# =========================================================
# Never run as root
# =========================================================
USER $APP_UID

# =========================================================
# Application entrypoint
# =========================================================
ENTRYPOINT ["dotnet", "CoreQuant.Worker.dll"]
