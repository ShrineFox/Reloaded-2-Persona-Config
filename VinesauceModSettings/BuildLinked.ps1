# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/VinesauceModSettings/*" -Force -Recurse
dotnet publish "./VinesauceModSettings.csproj" -c Release -o "$env:RELOADEDIIMODS/VinesauceModSettings" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location