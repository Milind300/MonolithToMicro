dotnet restore MonolithToMicro.slnx
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet build MonolithToMicro.slnx --no-restore
exit $LASTEXITCODE
