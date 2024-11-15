@echo off
cd %~dp0
%~d0

rmdir /s /q ..\Output

REM WINDOWS_X64
dotnet publish ..\Loader\ChoristaUtau.SettingUI.csproj -c Release  -o ..\Output --artifacts-path %temp%\tmp_ui
del /s /q ..\Output\*.exp
del /s /q ..\Output\*.lib
del /s /q ..\Output\*.pdb

REM Linux_RUNTIMER
set "zipFilePath=.\ext_runtime.zip"
set "destinationFolder=..\Output\"
powershell -Command "Expand-Archive -Path '%zipFilePath%' -DestinationPath '%destinationFolder%' -Force"

REM Linux_SH
move ..\Output\runner.sh ..\Output\setting.sh
