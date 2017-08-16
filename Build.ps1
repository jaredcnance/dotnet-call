# Gets the version suffix from the repo tag
# example: v1.0.0-preview1-final => preview1-final
function Get-Version-Suffix-From-Tag
{
  $tag=$env:APPVEYOR_REPO_TAG_NAME
  $split=$tag -split "-"
  $suffix=$split[1..2]
  $final=$suffix -join "-"
  return $final
}

$revision = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1 }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$revision = "{0:D4}" -f [convert]::ToInt32($revision, 10)

dotnet restore .\src\dotnet-exec.csproj

#dotnet test ./test/UnitTests/UnitTests.csproj

dotnet build .\src\dotnet-exec.csproj -c Release

echo "APPVEYOR_REPO_TAG: $env:APPVEYOR_REPO_TAG"

If($env:APPVEYOR_REPO_TAG -eq $true) {
    $revision = Get-Version-Suffix-From-Tag
    echo "RUNNING dotnet pack .\src\dotnet-exec.csproj -c Release -o .\artifacts --version-suffix=$revision"
    dotnet pack .\src\dotnet-exec.csproj -c Release -o .\artifacts --version-suffix=$revision 
}
Else { 
    echo "RUNNING dotnet pack .\src\dotnet-exec.csproj -c Release -o .\artifacts --version-suffix=alpha1-$revision"
    dotnet pack .\src\dotnet-exec.csproj -c Release -o .\artifacts --version-suffix=alpha1-$revision 
}