$MyInvocation.MyCommand.Path | Split-Path | Push-Location

if (Get-Process "FXServer" -ErrorAction SilentlyContinue) {
    .\dependencies\icecon_windows_amd64.exe -c "restart fivepd" 127.0.0.1:30120 PMdjCw0yPbw*Vn05lvcC | Out-Null
}