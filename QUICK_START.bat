@echo off
echo ===============================================
echo JMRTD C# Quick Start Verification
echo ===============================================
echo.

echo [1/5] Building projects...
dotnet build CSharpProject
if %errorlevel% neq 0 (
    echo ERROR: CSharpProject build failed!
    pause
    exit /b 1
)

dotnet build TestConsole
if %errorlevel% neq 0 (
    echo ERROR: TestConsole build failed!
    pause
    exit /b 1
)

echo ✓ Projects built successfully
echo.

echo [2/5] Running comprehensive tests...
cd TestConsole
dotnet run -- --test-all
if %errorlevel% neq 0 (
    echo ERROR: Tests failed!
    pause
    exit /b 1
)

echo ✓ All tests passed
echo.

echo [3/5] Checking for PC/SC readers...
dotnet run -- --list-readers
echo.

echo [4/5] Testing MRZ key derivation...
dotnet run -- --mrz-key L898902C36 740812 120415
echo.

echo [5/5] Ready for passport scanning!
echo.
echo ===============================================
echo SETUP COMPLETE - READY FOR PRODUCTION USE
echo ===============================================
echo.
echo Next steps:
echo 1. Connect your passport reader
echo 2. Place passport on reader
echo 3. Run: dotnet run -- --scan-passport-real
echo.
echo For help: dotnet run -- --help
echo.
pause
