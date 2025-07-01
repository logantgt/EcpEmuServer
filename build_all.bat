@echo on

dir

mkdir build
cd src

echo Build Win
dotnet publish -c Release --os win --self-contained true
ren .\EcpEmuServer\bin\Release\net9.0\win-x64\publish EcpEmuServer-win_x64
powershell Compress-Archive .\EcpEmuServer\bin\Release\net9.0\win-x64\EcpEmuServer-win_x64 ../build/EcpEmuServer-win_x64.zip -Force

echo Build Linux
dotnet publish -c Release --os linux --self-contained true
del .\EcpEmuServer\bin\Release\net9.0\linux-x64\publish\EcpEmuServer.pdb
del .\EcpEmuServer\bin\Release\net9.0\linux-x64\publish\EcpEmuServer.deps.json
del .\EcpEmuServer\bin\Release\net9.0\linux-x64\publish\EcpEmuServer.staticwebassets.endpoints.json
del .\EcpEmuServer\bin\Release\net9.0\linux-x64\publish\AutoHotKey.Interop.dll
ren .\EcpEmuServer\bin\Release\net9.0\linux-x64\publish EcpEmuServer-linux_x64
powershell Compress-Archive .\EcpEmuServer\bin\Release\net9.0\linux-x64\EcpEmuServer-linux_x64 ../build/EcpEmuServer-linux_x64.zip -Force

echo Build Mac64
dotnet publish -c Release --os osx --self-contained true
del .\EcpEmuServer\bin\Release\net9.0\osx-x64\publish\EcpEmuServer.pdb
del .\EcpEmuServer\bin\Release\net9.0\osx-x64\publish\EcpEmuServer.deps.json
del .\EcpEmuServer\bin\Release\net9.0\osx-x64\publish\EcpEmuServer.staticwebassets.endpoints.json
del .\EcpEmuServer\bin\Release\net9.0\osx-x64\publish\AutoHotKey.Interop.dll
ren .\EcpEmuServer\bin\Release\net9.0\osx-x64\publish EcpEmuServer-osx_x64
powershell Compress-Archive .\EcpEmuServer\bin\Release\net9.0\osx-x64\EcpEmuServer-osx_x64 ../build/EcpEmuServer-osx_x64.zip -Force

echo Build MacARM
dotnet publish -c Release --os osx --arch arm64 --self-contained true
del .\EcpEmuServer\bin\Release\net9.0\osx-arm64\publish\EcpEmuServer.pdb
del .\EcpEmuServer\bin\Release\net9.0\osx-arm64\publish\EcpEmuServer.deps.json
del .\EcpEmuServer\bin\Release\net9.0\osx-arm64\publish\EcpEmuServer.staticwebassets.endpoints.json
del .\EcpEmuServer\bin\Release\net9.0\osx-arm64\publish\AutoHotKey.Interop.dll
ren .\EcpEmuServer\bin\Release\net9.0\osx-arm64\publish EcpEmuServer-osx_arm64
powershell Compress-Archive .\EcpEmuServer\bin\Release\net9.0\osx-arm64\EcpEmuServer-osx_arm64 ../build/EcpEmuServer-osx_arm64.zip -Force

echo Done
PAUSE