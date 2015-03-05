# manually collect build artifacts and zip them
# manual zipping is required to prevent the assembly info patcher from detecting the packaged source code and reverting the assembly info

# command line parameters
Param
(
    [Parameter(Mandatory=$False)][string]$DestinationFile,
    [Parameter(Mandatory=$False)][string]$NopSourceDir
)

# functions
function Zip
{
    Param
    (
      [Parameter(Mandatory=$True)][string]$DestinationFileName,
      [Parameter(Mandatory=$True)][string]$SourceDirectory,
      [Parameter(Mandatory=$False)][string]$CompressionLevel = "Optimal",
      [Parameter(Mandatory=$False)][bool]$IncludeParentDir
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem

    $CompressionLevel = [System.IO.Compression.CompressionLevel]::$CompressionLevel  

    [System.IO.Compression.ZipFile]::CreateFromDirectory($SourceDirectory, $DestinationFileName, $CompressionLevel, $IncludeParentDir)
}

# check for bundle directory and delete
if (Test-Path ./bundle)
{
    rm ./bundle -Recurse -Force -Verbose
}

if ($NopSourceDir -eq $null -or $NopSourceDir -eq "")
{
    $NopSourceDir = "./src"
}

# copy source code and compiled output to bundle folder
cp (Join-Path $NopSourceDir /AllSystemsGo.Plugins/nop-fix-order-status) ./bundle/source/ -Recurse -Verbose
cp (Join-Path $NopSourceDir /Presentation/Nop.Web/Plugins/AllSystemsGo.FixDownloadableProductOrderStatus) ./bundle -Recurse -Verbose

# remove build folders from bundled source
rm ./bundle/source -Recurse -Include obj,bin -Verbose

# prepare to zip bundle contents
$SourceFolder = Resolve-Path ./bundle -Verbose
$Compression = "Optimal"  # Optimal, Fastest, NoCompression

Write-Host "Resolving destination file path."

if ($DestinationFile -eq $null -or $DestinationFile -eq "")
{
    $DestinationFile = "$SourceFolder.zip"
}
else
{
    if (![System.IO.Path]::IsPathRooted($DestinationFile))
    {
        $DestinationFile = Join-Path $PWD $DestinationFile -Verbose
    }
}

Write-Host "Destination file path: $DestinationFile"

#check if file already exists and delete it
if (Test-Path $DestinationFile)
{
    rm $DestinationFile -Force -Verbose
}

Write-Host "Compressing $SourceFolder into $DestinationFile"

Zip -DestinationFileName $DestinationFile `
    -SourceDirectory $SourceFolder `
    -CompressionLevel $Compression `
    -IncludeParentDir $false

Write-Host "Package complete"