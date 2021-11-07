#!/bin/sh
# apt-get install libxml2-utils

VERSION=$(xmllint --xpath "/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='version']/text()" src/DataTools.nuspec)
echo $VERSION

rm -rf build
mkdir -p build

dotnet build src/Common/Common.csproj -c Release -p:Version=$VERSION
dotnet build src/Analyzers/Analyzers.csproj -c Release -p:Version=$VERSION
nuget pack src/DataTools.nuspec

mv src/Common/bin/Release/*.nupkg build/
mv src/Analyzers/bin/Release/*.nupkg build/
mv *.nupkg build/