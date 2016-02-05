<## 

	This CmdLet creates a Package.appxmanifest file that can be used in a 
    Visual Studio Universal Windows Platform (UAP) project
    for deployment to the Windows Store.

	Usage:

		New-PackageAppxManifest -Template <path-to-package-appx-manifest-template-file> `
								-PackageInfo <path-to-package-xml-file> `
								[-OutDir <path-to-target-folder>]

	Parameters:
		
		-Template:		Path to a .\Package.appxmanifest XML template file.
		
						The .\Package.appxmanifest XML template file acts as a template
						that contains most of the values in the target .\Package.appxmanifest.
						The following values will be replaced by those coming from the
						package information XML file:

						/Package/Identity/@Name
						/Package/Identity/@Publisher
						/Package/Identity/@Version

						/Package/PhoneIdentity/@PhoneProductId
						/Package/PhoneIdentity/@PhonePublisherId

						/Package/Properties/DisplayName
						/Package/Properties/PublisherDisplayName

						/Package/Applications/Application[@Id = "App"]/VisualElements/@DisplayName
						/Package/Applications/Application[@Id = "App"]/VisualElements/@Description

		-PackageInfo:	Path to a file containing package publishing information.

						The package information file is an XML document containing
						information about the package and the publisher that make
						it possible to publish the application to the Windows Store.

						The package information file is missing from the source code
						for this app because it contains confidential information.
						To make a blank package information file with default values,
						use the Make-PackageInfo.ps1 CmdLet.

		-OutDir:		Path to a folder in which the .\Package.appxmanifest should be created (optional).

						If not specified, the target package manifest file will be
						written to the standard console output.

 ##>
[CmdLetBinding()]
param(
	[Parameter(Mandatory = $true)]
	[String] $template,

	[Parameter(Mandatory = $true)]
	[String] $packageInfo,

	[String] $outDir = $null,

	[Switch] $whatIf = $false
)

PROCESS
{
	if (-not (Test-Path $template))
		{ throw "Missing required Package.appxmanifest XML template file." }

	if (-not (Test-Path $packageInfo))
		{ throw "Missing required package information XML file." }


	[xml] $manifest = Get-Content $template
	[xml] $package = Get-Content $packageInfo

	$manifest.Package.Identity.Name = $package.Package.Identity.Name
	$manifest.Package.Identity.Publisher = $package.Package.Identity.Publisher
	$manifest.Package.Identity.Version = $package.Package.Identity.Version

	$manifest.Package.PhoneIdentity.PhoneProductId = $package.Package.PhoneIdentity.PhoneProductId
	$manifest.Package.PhoneIdentity.PhonePublisherId = $package.Package.PhoneIdentity.PhonePublisherId

	$manifest.Package.Properties.DisplayName = $package.Package.Properties.DisplayName
	$manifest.Package.Properties.PublisherDisplayName = $package.Package.Properties.PublisherDisplayName

	$application = $manifest.Package.Applications.Application |? { $_.Id -eq "App" }

	$application.VisualElements.DisplayName = $package.Package.Application.DisplayName
	$application.VisualElements.Description = $package.Package.Application.Description

	if (-not $outDir)
		{ Write-Output $manifest.OuterXml }

	else
	{
		[Environment]::CurrentDirectory = $PWD
		$path = Join-Path -Path $outDir -ChildPath "Package.appxmanifest"
		if (-not $whatIf)
		{
			$manifest.Save($path)
		}
		else
		{
			Write-Host "Saving $($path)..."
		}
	}
}