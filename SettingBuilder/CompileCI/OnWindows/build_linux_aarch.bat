@echo off
setlocal enabledelayedexpansion

@REM INIT DIRECTORY
set WORK_DIR=%~dp0
set SOLUTION_DIR=%WORK_DIR%\..\..\..\

@REM PREPARE_TEMP_DIR
set TMP_DIR=%TEMP%\ChoristaUtau_LinuxCI_ARM
del /s /q %TMP_DIR%

@REM COMPILE
%~d0
cd %SOLUTION_DIR%
set AppData=%TMP_DIR%\AppData
dotnet publish -r linux-arm64 -p:PlatformTarget=ARM64 -o %TMP_DIR%\ExtDir .\UtauForTuneLab.sln

@REM CopyDepends For BaseEngines
call :COPY_BASE Ude.dll
call :COPY_BASE protobuf-net.Core.dll
call :COPY_BASE protobuf-net.dll
call :DEL_BASE TuneLab.Base.dll
call :DEL_BASE TuneLab.Extensions.Voices.dll
call :DEL_BASE *.pdb

@REM CopyDepends For OUPortPhonemizers
call :COPY_EXT libonnxruntime.so
call :COPY_EXT Microsoft.ML.OnnxRuntime.dll
call :COPY_EXT SharpCompress.dll
call :COPY_EXT Newtonsoft.Json.dll
call :COPY_EXT System.IO.Packaging.dll
call :COPY_EXT NumSharp.dll
call :COPY_EXT onnxruntime.dll
call :COPY_EXT onnxruntime_providers_shared.dll
call :COPY_EXT WanaKanaNet.dll
call :COPY_EXT Tomlyn.dll
call :COPY_EXT YamlDotNet.dll
call :COPY_EXT ZstdSharp.dll
call :COPY_EXT IkG2p.dll
call :COPY_EXT libonnxruntime_providers_shared.so
call :DEL_EXT ChoristaUtauApi.dll
call :DEL_EXT *.pdb

@REM SET DESCRIPTION
set JSON_FILE=%TMP_DIR%\AppData\TuneLab\Extensions\ChoristaUtau\description.json
set PlatForm=linux-arm64
powershell -Command "$jj=((Get-Content '%JSON_FILE%' -Raw) | ConvertFrom-Json);$jj.platforms=@('%PlatForm%');$jc=$jj | ConvertTo-Json -Depth 100;Set-Content -Path %JSON_FILE% -Value $jc"

@REM BUILD SETTING UI
cmd /c "%SOLUTION_DIR%\SettingBuilder\SettingUI\CompileCI\anycpu.bat"
xcopy "%SOLUTION_DIR%\SettingBuilder\SettingUI\Output" %TMP_DIR%\AppData\TuneLab\Extensions\ChoristaUtau\setting_ui /E /I /Y

@REM package
del %TMP_DIR\Output.tlx
powershell Compress-Archive -Path %TMP_DIR%\AppData\TuneLab\Extensions\ChoristaUtau\* -DestinationPath %TMP_DIR%\Output.zip

@REM COPYBACK
call :READ_VERSION
mkdir $SOLUTION_DIR\Output
copy /y %TMP_DIR%\Output.zip %SOLUTION_DIR%\Output\ChoristaUtauForTuneLab_%version%_linux_aarch64.tlx

@REM CLEAR
del /s /q %TMP_DIR%

@REM CALLBACK DIRECTORY
cd %WORK_DIR%
goto :eof


@REM FUNCTION PART
:COPY_BASE
copy /y %TMP_DIR%\ExtDir\%1 %TMP_DIR%\AppData\TuneLab\Extensions\ChoristaUtau\%1
goto :eof

:DEL_BASE
del %TMP_DIR%\AppData\TuneLab\Extensions\ChoristaUtau\%1
goto :eof

:COPY_EXT
copy /y %TMP_DIR%\ExtDir\%1 %TMP_DIR%\AppData\TuneLab\Extensions\ChoristaUtau\phonemizers\OpenUtauBuiltinAdapter\%1
goto :eof

:DEL_EXT
del %TMP_DIR%\AppData\TuneLab\Extensions\ChoristaUtau\phonemizers\OpenUtauBuiltinAdapter\%1
goto :eof

:READ_VERSION
@powershell -Command (Get-Content %JSON_FILE% -Raw ^| ConvertFrom-Json).version > t.txt
for /f "delims=" %%a in (t.txt) do (
	set version=%%a
	goto :eof
)
goto :eof
