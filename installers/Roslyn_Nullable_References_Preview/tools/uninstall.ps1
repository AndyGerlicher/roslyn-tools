Set-StrictMode -version 2.0
$ErrorActionPreference="Stop"

# Deploy our core VSIX libraries to Visual Studio via the Roslyn VSIX tool.  This is an alternative to
# deploying at build time.
function Uninstall-VsixViaTool() {
    $vsixExe = Join-Path $PSScriptRoot "vsixexpinstaller\VsixExpInstaller.exe"
    $vsixExe = "`"$vsixExe`""
    $both = Get-VisualStudioDirAndId
    $vsDir = $both[0].Trim("\")
    $vsId = $both[1]
    $hive = ""
    Write-Host "Using VS Instance $vsId at `"$vsDir`""
    $baseArgs = "/rootSuffix:$hive /u /vsInstallDir:`"$vsDir`""
    $all = @(
        "vsix\ExpressionEvaluatorPackage.vsix"
        "vsix\Roslyn.VisualStudio.InteractiveComponents.vsix",
        "vsix\Roslyn.VisualStudio.Setup.Next.vsix",
        "vsix\Roslyn.VisualStudio.Setup.vsix",
        "vsix\Roslyn.Compilers.Extension.vsix")

    Write-Host "Un-Installing all Roslyn VSIXes"
    foreach ($e in $all) {
        $name = $e
        $filePath = "`"$((Resolve-Path $e).Path)`""
        $fullArg = "$baseArgs $filePath"
        Write-Host "`tUn-Installing $name"
        Exec-Console $vsixExe $fullArg
    }
}

try {
    . (Join-Path $PSScriptRoot "utils.ps1")

    if (Test-Process "devenv") {
        Write-Host "Please shut down all instances of Visual Studio before running"
        exit 1
    }

    Uninstall-VsixViaTool
    exit 0
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}

