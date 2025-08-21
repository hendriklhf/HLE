$ErrorActionPreference = "Stop"

$nuget_source = "https://api.nuget.org/v3/index.json"
#$nuget_source = "https://int.nugettest.org"

$api_key = $env:NUGET_API_KEY

if ($api_key.Length -eq 0)
{
    Write-Error "No NuGet API key found. Please set the environment variable 'NUGET_API_KEY'"
    exit
}

$libraries_directory = "src/libraries"
if ($args.Length -eq 0 -or $args[0].Length -eq 0)
{
    Write-Error "No library specified. Please provide the library name as an argument."
    exit
}

$library = $args[0]
$library_directory = [System.IO.Path]::Combine($libraries_directory, $library)

Push-Location
try
{
    Set-Location $library_directory
    Remove-Item "bin/Release/*.nupkg"

    dotnet pack -c Release -p:PublishingPackage=true
    if ($LastExitCode -ne 0)
    {
        exit
    }

    Set-Location "bin/Release"
    $nuget_package = Get-Item "*.nupkg"

    dotnet nuget push $nuget_package --api-key $api_key --source $nuget_source --skip-duplicate
    if ($LastExitCode -ne 0)
    {
        exit
    }
}
finally
{
    Pop-Location
}
