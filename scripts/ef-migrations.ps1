$ErrorActionPreference = 'Stop'
# Run in solution root, after SQL Server and ECommerceDb exist.
$names = @('Catalog','Product','Order','User')
foreach ($name in $names) {
  $project = "src/MonolithToMicro.$name.Api"
  dotnet ef migrations add InitialCreate --project $project --startup-project $project --output-dir Data/Migrations
  if ($LASTEXITCODE -ne 0) { throw "Migration generation failed for $name" }
}
Write-Host 'Migrations generated. Review the files, then run scripts/apply-migrations.ps1.'
