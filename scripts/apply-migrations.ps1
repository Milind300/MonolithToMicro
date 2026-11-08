$ErrorActionPreference = 'Stop'
foreach ($name in @('Catalog','Product','Order','User')) {
  $project = "src/MonolithToMicro.$name.Api"
  dotnet ef database update --project $project --startup-project $project
  if ($LASTEXITCODE -ne 0) { throw "Database update failed for $name" }
}
