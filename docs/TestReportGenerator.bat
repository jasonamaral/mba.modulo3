@echo off
setlocal

:: Caminho do projeto de testes
set TEST_PROJECT=..\src\FluencyHub.Tests\FluencyHub.Tests.csproj

:: Caminho relativo à pasta do projeto
set COVERAGE_DIR=TestResults\Coverage
set COVERLET_OUTPUT=%COVERAGE_DIR%\coverage

:: Caminho absoluto para relatório final
set REPORT_DIR=..\src\FluencyHub.Tests\TestResults\Coverage\report

:: Limpa e recria o diretório (opcional)
if exist %REPORT_DIR% (
    rd /s /q %REPORT_DIR%
)
mkdir %REPORT_DIR%

:: Build do projeto
echo.
echo Buildando o projeto de testes...
dotnet build %TEST_PROJECT% --configuration Release
IF %ERRORLEVEL% NEQ 0 (
    echo Build falhou. Abortando.
    exit /b 1
)

:: Executa testes com cobertura
echo.
echo Executando testes com cobertura...
dotnet test %TEST_PROJECT% --no-build --configuration Release ^
  /p:CollectCoverage=true ^
  /p:CoverletOutputFormat="cobertura" ^
  /p:CoverletOutput=%COVERLET_OUTPUT%

:: Verifica se o ReportGenerator está instalado
where reportgenerator >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo.
    echo ReportGenerator não encontrado.
    echo Instale com: dotnet tool install -g dotnet-reportgenerator-globaltool
    exit /b 1
)

:: Gera o relatório HTML
echo.
echo Gerando relatório HTML de cobertura...
reportgenerator ^
  "-reports:..\src\FluencyHub.Tests\%COVERLET_OUTPUT%.cobertura.xml" ^
  "-targetdir:%REPORT_DIR%" ^
  -reporttypes:Html

echo.
echo Relatório gerado em: %REPORT_DIR%\index.html

endlocal
pause
