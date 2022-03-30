#!/bin/sh
dotnet publish test/Vpiska.UnitTests/Vpiska.UnitTests.csproj -c Release
dotnet test test/Vpiska.UnitTests/bin/Release/net6.0/Vpiska.UnitTests.dll