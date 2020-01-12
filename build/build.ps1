[CmdletBinding()]
[OutputType([void])]
param(
    [Parameter(Mandatory = $false)]
    [int]
    $Major = 2,

    [Parameter(Mandatory = $false)]
    [int]
    $Minor = 0,

    [Parameter(Mandatory = $false)]
    [int]
    $Build = ('{0:yy}{1:000}' -f [datetime]::Today, [datetime]::Today.DayOfYear),

    [Parameter(Mandatory = $false)]
    [int]
    $Revision = (([datetime]::Now - [datetime]::Today).TotalSeconds / 1.4)
)

Clear-Host

# construct a Version object to ensure arguments are valid
$version = New-Object -TypeName System.Version -ArgumentList $Major, $Minor, $Build, $Revision

# build and package solution
#https://docs.microsoft.com/en-us/nuget/create-packages/symbol-packages-snupkg
dotnet build -p:DelaySign=false`;Configuration=Debug`;Major=$($version.Major)`;Minor=$($Version.Minor)`;Build=$($Version.Build)`;Revision=$($version.Revision)
dotnet build -p:DelaySign=false`;Configuration=Release`;Major=$($version.Major)`;Minor=$($Version.Minor)`;Build=$($Version.Build)`;Revision=$($version.Revision)`;GeneratePackageOnBuild=true

# generate build.local.ps1 script file that allows to redo a build and package locally wihtout altering the build version number
$path = Split-Path $script:MyInvocation.MyCommand.Path
@"
& $(Join-Path $path build.ps1) -Major $($version.Major) -Minor $($version.Minor) -Build $($version.Build) -Revision $($version.Revision)
"@ > $(Join-Path $path build.local.ps1)
