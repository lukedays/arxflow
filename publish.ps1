# Script de publish do ArxFlow
# Executa o build do frontend React e publica o backend ASP.NET Core

$ErrorActionPreference = "Stop"

Write-Host "Publicando ArxFlow..." -ForegroundColor Cyan

# Diretorio do servidor
$serverDir = Join-Path $PSScriptRoot "Server"
$publishDir = Join-Path $serverDir "publish"

# Limpar diretorio de publish anterior
if (Test-Path $publishDir) {
    Write-Host "Removendo publish anterior..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $publishDir
}

# Executar publish
Write-Host "Executando dotnet publish..." -ForegroundColor Cyan
dotnet publish $serverDir -c Release -o $publishDir

if ($LASTEXITCODE -eq 0) {
    Write-Host "Publish concluido com sucesso!" -ForegroundColor Green
    Write-Host "Iniciando servidor..." -ForegroundColor Cyan
    Write-Host ""

    Push-Location $publishDir
    dotnet Server.dll
    Pop-Location
} else {
    Write-Host "Erro no publish!" -ForegroundColor Red
    exit 1
}
