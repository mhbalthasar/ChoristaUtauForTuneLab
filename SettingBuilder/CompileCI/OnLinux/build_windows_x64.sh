#!/bin/bash

WORK_DIR=$(cd $(dirname $0); pwd)
SOLUTION_DIR=$(cd "$WORK_DIR"; cd ../../../; pwd)

#PREPARE_TEMP_DIR
TMP_DIR=/tmp/ChoristaUtau_WinCI
rm -rf $TMP_DIR

#COMPILE
cd $SOLUTION_DIR
AppData=$TMP_DIR/AppData dotnet publish -r win-x64 -o $TMP_DIR/ExtDir ./UtauForTuneLab.sln

#CopyDepends For BaseEngines
function COPY_BASE() {
    cp $TMP_DIR/ExtDir/$1 $TMP_DIR/AppData/TuneLab/Extensions/ChoristaUtau/$1
}
function DEL_BASE() {
    rm $TMP_DIR/AppData/TuneLab/Extensions/ChoristaUtau/$1
}
COPY_BASE Ude.dll
COPY_BASE protobuf-net.Core.dll
COPY_BASE protobuf-net.dll
COPY_BASE MathNet.Numerics.dll
COPY_BASE YamlDotNet.dll
DEL_BASE TuneLab.Base.dll
DEL_BASE TuneLab.Extensions.Voices.dll
DEL_BASE *.pdb
#CopyDepends For OUPortPhonemizers
function COPY_EXT() {
    cp $TMP_DIR/ExtDir/$1 $TMP_DIR/AppData/TuneLab/Extensions/ChoristaUtau/phonemizers/OpenUtauBuiltinAdapter/$1
}
function DEL_EXT() {
    rm $TMP_DIR/AppData/TuneLab/Extensions/ChoristaUtau/phonemizers/OpenUtauBuiltinAdapter/$1
}
COPY_EXT libonnxruntime.so
COPY_EXT Microsoft.ML.OnnxRuntime.dll
COPY_EXT SharpCompress.dll
COPY_EXT Newtonsoft.Json.dll
COPY_EXT System.IO.Packaging.dll
COPY_EXT NumSharp.dll
COPY_EXT onnxruntime.dll
COPY_EXT onnxruntime_providers_shared.dll
COPY_EXT WanaKanaNet.dll
COPY_EXT Tomlyn.dll
COPY_EXT YamlDotNet.dll
COPY_EXT ZstdSharp.dll
COPY_EXT IkG2p.dll
COPY_EXT libonnxruntime_providers_shared.so
DEL_EXT ChoristaUtauApi.dll
DEL_EXT *.pdb

#SET DESCRIPTION
jq '.platforms = ["win-x64"]' $TMP_DIR/ExtDir/description.json > $TMP_DIR/AppData/TuneLab/Extensions/ChoristaUtau/description.json

#BUILD SETTING UI
chmod a+x "$SOLUTION_DIR/SettingBuilder/SettingUI/CompileCI/anycpu.sh"
"$SOLUTION_DIR/SettingBuilder/SettingUI/CompileCI/anycpu.sh"
cp -r "$SOLUTION_DIR/SettingBuilder/SettingUI/Output" $TMP_DIR/AppData/TuneLab/Extensions/ChoristaUtau/setting_ui

#Package
cd $TMP_DIR/AppData/TuneLab/Extensions/ChoristaUtau/
zip -r $TMP_DIR/Output.tlx .

#GETVERSION
version=$(jq -r ".version" $TMP_DIR/ExtDir/description.json)

#COPYBACK
mkdir $SOLUTION_DIR/Output
cp $TMP_DIR/Output.tlx $SOLUTION_DIR/Output/ChoristaUtauForTuneLab_$version_win64.tlx

#CLEAR
rm -rf $TMP_DIR

