@echo off
set projDir=%cd%
echo Project directory is %projDir%

cd ..
set myDIR=Lethal Company\BepInEx\plugins\MyFirstMod
IF not exist "%myDIR%" (
    echo Creating directory: %myDIR%
    mkdir "%myDIR%"
)

echo Copying DLL to mod directory...
copy "%projDir%\bin\Debug\netstandard2.1\ESN.MyFirstMod.dll" "%myDIR%"

if %errorlevel% equ 0 (
    echo Copy successful!
) else (
    echo Copy failed with error code %errorlevel%
    exit /b %errorlevel%
)

echo Launching Lethal Company...
"Lethal Company\Lethal Company.exe"
echo OK