#!/bin/sh
# apt-get install libxml2-utils

VERSION=$(xmllint --xpath "/package/metadata/version/text()" src/DataTools.nuspec)
echo $VERSION

rm -rf build
mkdir -p build

dotnet build src/Common/Common.csproj -c RELEASE -p:Version=$VERSION
dotnet build src/Analyzers/Analyzers.csproj -c RELEASE -p:Version=$VERSION
nuget pack src/DataTools.nuspec

mv src/Common/bin/Release/*.nupkg build/
mv src/Analyzers/bin/Release/*.nupkg build/
mv *.nupkg build/