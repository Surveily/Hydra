FROM mcr.microsoft.com/dotnet/core/sdk:3.0-bionic

# Build Arguments
ARG connection

# Test Connection String
ENV HYDRATEST=${connection}

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
RUN dotnet test src -c Release --no-build --no-restore