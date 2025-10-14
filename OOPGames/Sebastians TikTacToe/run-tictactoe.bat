@echo off
REM Builds and runs Sebastian's TicTacToe WPF project
set PROJ_DIR=%~dp0
set PROJ=%PROJ_DIR%SebastiansTikTacToe.csproj
echo Building %PROJ%
dotnet build "%PROJ%" -c Debug || goto :error
echo Running project
dotnet run --project "%PROJ%" -c Debug || goto :error
echo Done
pause
exit /b 0
:error
echo Build or run failed
pause
exit /b 1
