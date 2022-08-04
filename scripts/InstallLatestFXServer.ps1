$BuildUrl = (Invoke-RestMethod -Uri "https://changelogs-live.fivem.net/api/changelog/versions/windows/server").latest_download # locate latest build

# get the paths we need to store the build
$RootPath = Split-Path $PSScriptRoot -Parent
$BuildPath = Join-Path -Path $RootPath -ChildPath "dev\FXServer"
$BuildZipPath = Join-Path -Path $BuildPath -ChildPath "build.zip"

Invoke-WebRequest -Uri $BuildUrl -OutFile $BuildZipPath # download zip
Expand-Archive -Path $BuildZipPath -DestinationPath $BuildPath # unarchive zip into fxserver folder
Remove-Item $BuildZipPath # delete zip