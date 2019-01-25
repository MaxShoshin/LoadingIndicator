try {
        cd LoadingIndicator.WinForms\bin\Release
	$version = (gi LoadingIndicator.WinForms.dll).VersionInfo.ProductVersion
	echo "Building package with version $version."
	& '..\..\..\packages\NuGet.CommandLine.4.9.2\tools\NuGet.exe' pack LoadingIndicator.WinForms.nuspec -Version $version -OutputDirectory .
}
catch {
	exit 1
}