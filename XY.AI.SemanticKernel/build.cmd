@echo off
setlocal

set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Release

set ROOT=%~dp0..
set LIB=%~dp0XY.AI.SemanticKernel.csproj
set TEST=%ROOT%\XY.AI.SemanticKernelTests\XY.AI.SemanticKernelTests.csproj
set OUT=%ROOT%\artifacts\packages

echo [1/4] Restore library...
dotnet restore "%LIB%" || exit /b 1

echo [2/4] Build library...
dotnet build "%LIB%" -c %CONFIG% --no-restore || exit /b 1

echo [3/4] Run tests...
dotnet test "%TEST%" -c %CONFIG% --no-restore || exit /b 1

echo [4/4] Pack library...
if not exist "%OUT%" mkdir "%OUT%"
dotnet pack "%LIB%" -c %CONFIG% --no-build -o "%OUT%" || exit /b 1

echo Done. Package output: %OUT%
exit /b 0
