try {
        cd LoadingIndicator.Winforms\bin\Release
	$version = (gi LoadingIndicator.Winforms.dll).VersionInfo.ProductVersion
	echo "Building package with version $version."
	& '..\..\..\packages\NuGet.CommandLine.4.6.2\tools\NuGet.exe' pack LoadingIndicator.Winforms.nuspec -Version $version -OutputDirectory .
}
catch {
	exit 1
}