FROM mcr.microsoft.com/dotnet/core/sdk:3.0-bionic

# Build Arguments
ARG version
ARG url
ARG key

# Add Source Files
ADD src /app/src
WORKDIR /app

# Build & Test
RUN dotnet build src
RUN dotnet test src --no-build --no-restore

# Create Packages
RUN mkdir nuget
RUN dotnet pack --no-build --no-restore -c Release -p:PackageVersion=1.2.0-${version} -o /app/src/nuget/ Surveily.Hydra.Core/Surveily.Hydra.Core.csproj
RUN dotnet pack --no-build --no-restore -c Release -p:PackageVersion=1.2.0-${version} -o /app/src/nuget/ Surveily.Hydra.Events/Surveily.Hydra.Events.csproj

# Publish Packages
RUN dotnet nuget push -s ${url} -k ${key} /app/src/nuget/Surveily.Hydra.Core.1.2.0-${version}.nupkg
RUN dotnet nuget push -s ${url} -k ${key} /app/src/nuget/Surveily.Hydra.Events.1.2.0-${version}.nupkg
