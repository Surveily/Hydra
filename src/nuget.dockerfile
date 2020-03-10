FROM surveily-hydra-build

# Build Arguments
ARG version
ARG key

# Create Packages
RUN mkdir /app/src/nuget
RUN dotnet pack --no-build --no-restore -c Release -p:PackageVersion=${version} -o /app/src/nuget/ src/Surveily.Hydra.Core/Surveily.Hydra.Core.csproj
RUN dotnet pack --no-build --no-restore -c Release -p:PackageVersion=${version} -o /app/src/nuget/ src/Surveily.Hydra.Events/Surveily.Hydra.Events.csproj
RUN dotnet pack --no-build --no-restore -c Release -p:PackageVersion=${version} -o /app/src/nuget/ src/Surveily.Hydra.Tools/Surveily.Hydra.Tools.csproj

# Publish Packages
RUN dotnet nuget push -s nuget.org -k ${key} /app/src/nuget/Surveily.Hydra.Core.${version}.nupkg
RUN dotnet nuget push -s nuget.org -k ${key} /app/src/nuget/Surveily.Hydra.Events.${version}.nupkg
RUN dotnet nuget push -s nuget.org -k ${key} /app/src/nuget/Surveily.Hydra.Tools.${version}.nupkg
