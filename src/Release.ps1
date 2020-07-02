<#
.Description
This script builds the binaries and packs them into archives so that they are ready for release.
#>

$ErrorActionPreference = "Stop"

function Main {
    if (!(Get-Command "dotnet" -ErrorAction SilentlyContinue))
    {
        throw "dotnet could not be found on the PATH."
    }


    Push-Location

    $srcDir = $PSScriptRoot
    $outDir = Join-Path $srcDir "out"

    Set-Location $srcDir
    dotnet publish -c Release -o $(Join-Path $outDir "schema-validation")

    Set-Location $outDir
    Compress-Archive -Path "schema-validation" -Destination "schema-validation.zip" -Force

    Pop-Location
}
Main
