try {
	$version = (gi LoadingIndicator.Controls.dll).VersionInfo.ProductVersion
	echo "Building package with version $version."
	& '..\..\..\packages\NuGet.CommandLine.4.6.2\tools\NuGet.exe' pack LoadingIndicator.Controls.nuspec -Version $version -OutputDirectory .
}
catch {
	exit 1
}