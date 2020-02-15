param($vs)

$source = $MyInvocation.MyCommand.Definition | Split-Path -Parent

if (-not $vs) {
    $vs = [Environment]::GetEnvironmentVariable("VisualStudio_15.0")
}
if (-not $vs) {
    throw "-vs option must be specified"
}
if (-not (Test-Path $vs)) {
    throw "specified path -vs $vs does not exist"
}
if (-not (Test-Path "$vs\Common7\IDE\devenv.exe")) {
    throw "specified path -vs $vs does not contain a Visual Studio installation"
}

if (-not (Test-Path "$source\Microsoft.VisualStudioTools.VSTestHost.15.0.dll") -or
    -not (Test-Path "$source\Microsoft.VisualStudioTools.VSTestHost.15.0.pkgdef")) {
    throw "expected VSTestHost files in $source"
}

"Installing VSTestHost from $source"
copy "$source\Microsoft.VisualStudioTools.VSTestHost.15.0.dll" "$vs\Common7\IDE\CommonExtensions\Platform" -Force
copy "$source\Microsoft.VisualStudioTools.VSTestHost.15.0.pkgdef" "$vs\Common7\IDE\CommonExtensions\Platform" -Force

"Updating *.exe.config files"
gci @(
    "$vs\Common7\IDE\MSTest.exe.config",
    "$vs\Common7\IDE\QTAgent*.exe.config",
    "$vs\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.*.exe.config"
) | ?{ Test-Path $_ } | %{
    $conf = [xml](gc $_);
    if (-not $conf.configuration.runtime.assemblyBinding.probing.privatePath.Contains("CommonExtensions\Microsoft\Editor")) {
        $conf.configuration.runtime.assemblyBinding.probing.privatePath += ";CommonExtensions\Platform;CommonExtensions\Microsoft\Editor";

        $n = $conf.configuration.runtime.assemblyBinding.AppendChild($conf.ImportNode(([xml]'<dependentAssembly>
          <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral"/>
          <bindingRedirect oldVersion="4.5.0.0-8.0.0.0" newVersion="8.0.0.0"/>
          <codeBase version="8.0.0.0" href="PrivateAssemblies\Newtonsoft.Json.dll"/>
        </dependentAssembly>').dependentAssembly, $true));

        $conf.Save("$_");
        " - $($_.Name)"
    }
}

"Executing devenv /setup"
Start-Process -Wait -NoNewWindow "$vs\Common7\IDE\devenv.exe" "/setup"

"Generating testsettings files"
$ts = [xml](gc "$source\vs2017.testsettings.template")
foreach ($n in $ts.TestSettings.Execution.TestTypeSpecific.UnitTestRunConfig.AssemblyResolution.RuntimeResolution.Directory) {
    $n.path = $n.path -replace '\$VSPath', "$vs";
    $n.path = $n.path -replace '\$VSVersion', "15.0";
}
foreach ($n in $ts.TestSettings.Execution.TestTypeSpecific.UnitTestRunConfig.AssemblyResolution.DiscoveryResolution.Directory) {
    $n.path = $n.path -replace '\$VSPath', "$vs";
    $n.path = $n.path -replace '\$VSVersion', "15.0";
}
foreach ($n in $ts.TestSettings.Properties.Property) {
    $n.value = $n.value -replace '\$VSPath', "$vs";
    $n.value = $n.value -replace '\$VSVersion', "15.0";
}
$ts.Save("$vs\vstesthost.testsettings")
" - $vs\vstesthost.testsettings"

foreach ($n in $ts.TestSettings.Properties.Property) {
    if ($n.name -eq "VSHive") {
        $n.value = "Exp";
    }
}
$ts.Save("$vs\vstesthost.exp.testsettings")
" - $vs\vstesthost.exp.testsettings"
""
