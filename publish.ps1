$ErrorActionPreference = "Stop"

$nuget_source = "https://api.nuget.org/v3/index.json" # "https://int.nugettest.org" #

$api_key = $env:NUGET_API_KEY
if ($api_key.Length == 0)
{
    Write-Error "No NuGet API key found. Please set the environment variable 'NUGET_API_KEY'"
    exit
}

$projects = "src/HLE" #, "src/HLE.Twitch"

$starting_directory = Get-Location
foreach ($project in $projects)
{
    Set-Location "$project"
    Remove-Item "bin/Release/*.nupkg"
    dotnet publish -c Release -p:PublishingPackage=true
    Set-Location "bin/Release"
    $nuget_package = Get-Item "*.nupkg"
    dotnet nuget push $nuget_package --api-key $api_key --source $nuget_source --skip-duplicate

    Set-Location $starting_directory
}
