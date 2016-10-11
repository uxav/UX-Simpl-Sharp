param([string] $ConfigurationName = "" )
if ($ConfigurationName -eq "")
{
  $ConfigurationName = "Release"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value;
    if($Invocation.PSScriptRoot)
    {
        $Invocation.PSScriptRoot;
    }
    Elseif($Invocation.MyCommand.Path)
    {
        Split-Path $Invocation.MyCommand.Path
    }
    else
    {
        $Invocation.InvocationName.Substring(0,$Invocation.InvocationName.LastIndexOf("\"));
    }
}

$scriptsdir = Get-ScriptDirectory
$resourcesPath = Join-Path (Get-Item $scriptsdir).Parent.FullName "Resources"
$dllpath =  Join-Path (Get-Item $scriptsdir).Parent.FullName "bin\$ConfigurationName\UXLib.dll"

Write-Host "Packaging cpz file..."
Write-Host "Script path         = " $scriptsdir
Write-Host "Resources path      = " $resourcesPath
Write-Host "dll path            = " $dllpath

$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllpath).FileVersion

Write-Host "dll Version         = " $version

Write-Host "Renaming file to UXLib_$version`_$ConfigurationName.dll"

Write-Host "Moving cpz to C:\Users\mike\Dropbox (UX Digital)\Crestron Resources\UXLib Builds\$ConfigurationName\UXLib_$version`_$ConfigurationName.dll"

Copy-Item $dllpath "C:\Users\mike\Dropbox (UX Digital)\Crestron Resources\UXLib Builds\$ConfigurationName\UXLib_$version`_$ConfigurationName.dll"