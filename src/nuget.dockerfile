FROM mcr.microsoft.com/dotnet/core/sdk:3.0-bionic

# Build Arguments
ARG arg-version
ARG arg-url
ARG arg-key

# Add Source Files
ADD src /app/src
WORKDIR /app

# Build & Test
RUN dotnet build src
#RUN dotnet test src --no-build --no-restore

# Create Packages
RUN mkdir nuget
RUN dotnet pack --no-build --no-restore -c Release -p:PackageVersion=2.0-${arg-version} -o /app/src/nuget/ src/Surveily.Hydra.Core/Surveily.Hydra.Core.csproj
RUN dotnet pack --no-build --no-restore -c Release -p:PackageVersion=2.0-${arg-version} -o /app/src/nuget/ src/Surveily.Hydra.Events/Surveily.Hydra.Events.csproj

# Publish Packages
RUN dotnet nuget push -s ${arg-url} -k ${arg-key} /app/src/nuget/Surveily.Hydra.Core.2.0-${arg-version}.nupkg
RUN dotnet nuget push -s ${arg-url} -k ${arg-key} /app/src/nuget/Surveily.Hydra.Events.2.0-${arg-version}.nupkg
