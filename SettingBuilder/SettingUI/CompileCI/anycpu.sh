#!/bin/bash
script_dir=$(cd "$(dirname "$0")" && pwd)
script_name=$(basename "$0" .sh)

cd "$script_dir"

rm -rf ../Output
rm -rf ../Output_WIN

#WINDOWS_X64
dotnet publish ../Loader/ChoristaUtau.SettingUI.csproj -c Release -r win-x64  -o ../Output_WIN --artifacts-path /tmp/tmp_ui_win
mkdir ../Output
cp ../Output_WIN/setting.exe ../Output/
rm -rf ../Output_WIN

#ANY_CPU
dotnet publish ../Loader/ChoristaUtau.SettingUI.csproj -c Release -o ../Output --artifacts-path /tmp/tmp_ui
rm -f ../Output/*.exp
rm -f ../Output/*.lib
rm -f ../Output/*.pdb

#Linux_RUNTIMER
unzip ./ext_runtime.zip -d ../Output
chmod a+x ../Output/runner.sh


mv ../Output/runner.sh ../Output/setting.sh
