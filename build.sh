#!/bin/sh
# apt-get install libxml2-utils

VERSION=$(xmllint --xpath "/build/version/text()" src/BuildConfig.xml)
echo $VERSION

rm -rf build
mkdir -p build

dotnet build src/Common/Common.csproj -c RELEASE -p:Version=$VERSION
dotnet build src/Analyzers/Analyzers.csproj -c RELEASE -p:Version=$VERSION
nuget pack src/DataTools.nuspec -Version $VERSION

mv src/Common/bin/Release/*.nupkg build/
mv src/Analyzers/bin/Release/*.nupkg build/
mv *.nupkg build/