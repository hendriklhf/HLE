$ErrorActionPreference = "Stop"

$nuget_source = "https://api.nuget.org/v3/index.json"
#$nuget_source = "https://int.nugettest.org"

$api_key = $env:NUGET_API_KEY

if ($api_key.Length -eq 0)
{
    Write-Error "No NuGet API key found. Please set the environment variable 'NUGET_API_KEY'"
    exit
}

$packages_directory = "src/packages"
if ($args.Length -eq 0 -or $args[0].Length -eq 0)
{
    Write-Error "No package specified. Please provide the package name as an argument."
    exit
}

$package = $args[0]
$package_directory = [System.IO.Path]::Combine($packages_directory, $package, "src")

Push-Location
try
{
    Set-Location $package_directory
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
