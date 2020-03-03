FROM mcr.microsoft.com/dotnet/core/sdk:3.0-bionic

# Build Arguments
ARG version
ARG key

# Set Project Directory
WORKDIR /app

# Restore Packages
COPY src/*.sln ./src/
COPY src/*/*.csproj ./src/
RUN for file in $(ls ./src/*.csproj); do mkdir -p ./${file%.*}/ && mv $file ./${file%.*}/; done
RUN dotnet msbuild /t:restore /p:Configuration=Release src

# Add Source Files
ADD src ./src
ADD LICENSE.md .

# Build & Test
RUN dotnet build src -c Release --no-restore
#RUN dotnet test src -c Release --no-build --no-restore

# Create Packages
RUN mkdir nuget
RUN dotnet pack --no-build --no-restore -c Release -p:PackageVersion=${version} -o /app/src/nuget/ src/Surveily.Hydra.Core/Surveily.Hydra.Core.csproj
RUN dotnet pack --no-build --no-restore -c Release -p:PackageVersion=${version} -o /app/src/nuget/ src/Surveily.Hydra.Events/Surveily.Hydra.Events.csproj

RUN ls /app/src/nuget

# Publish Packages
RUN dotnet nuget push -s nuget.org -k ${key} /app/src/nuget/Surveily.Hydra.Core.${version}.nupkg
RUN dotnet nuget push -s nuget.org -k ${key} /app/src/nuget/Surveily.Hydra.Events.${version}.nupkg
