@echo off
echo ========================================
echo   LUXE GLOW STUDIO - API SERVER
echo ========================================
echo Starting server on http://localhost:5297 and http://localhost:5298
echo Swagger UI: http://localhost:5297
echo.
dotnet run --urls="http://localhost:5297;http://localhost:5298"
pause