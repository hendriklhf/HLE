$ErrorActionPreference = "Stop"

$nuget_source = "https://api.nuget.org/v3/index.json"
#$nuget_source = "https://int.nugettest.org"

$api_key = $env:NUGET_API_KEY

if ($api_key.Length -eq 0)
{
    Write-Error "No NuGet API key found. Please set the environment variable 'NUGET_API_KEY'"
    exit
}

$projects = "src/libraries/HLE" #, "src/libraries/HLE.Twitch"

$starting_directory = Get-Location
foreach ($project in $projects)
{
    try
    {
        Set-Location "$project"
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
        Set-Location $starting_directory
    }
}
