$path = Get-ChildItem -Filter *.nupkg -Recurse | Sort-Object -Descending | Select-Object -First 1 | Resolve-Path -Relative
Write-Host "Pushing NuGet package $path on to NuGet.org"
NuGet.exe push $path -source https://api.nuget.org/v3/index.json
