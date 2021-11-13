Push-Location $PSScriptRoot

[xml]$configXml = Get-Content ./src/DataTools.nuspec
$version = $configXml.package.metadata.version
Write-Host($version)

Remove-Item -Path build -Recurse -Force
New-Item -ItemType Directory -Path build

# dotnet build src/Common/Common.csproj -c Release -p:Version="$version"
dotnet build src/Analyzers/Analyzers.csproj -c Release -p:Version="$version"
# nuget pack src/DataTools.nuspec

# Move-Item -Path src/Common/bin/Release/*.nupkg -Destination build/
Move-Item -Path src/Analyzers/bin/Release/*.nupkg -Destination build/
# Move-Item *.nupkg build/