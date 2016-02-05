<## 

	This CmdLet creates a Package.StoreAssociation.xml file that can be used in a 
    Visual Studio Universal Windows Platform (UAP) project
    for deployment to the Windows Store.

	Usage:

		New-PackageStoreAssociation -Template <path-to-package-store-association-template-file> `
									-PackageInfo <path-to-package-xml-file> `
									[-OutDir <path-to-target-folder>]

	Parameters:
		
		-Template:		Path to a .\Package.StoreAssociation.xml template file.
		
						The .\Package.StoreAssociation.xml template file acts as a template
						that contains most of the values in the target .\Package.StoreAssociation.xml.
						The following values will be replaced by those coming from the
						package information XML file:

						/StoreAssociation/Publisher
						/StoreAssociation/PublisherDisplayName

						/StoreAssociation/ProductReservedInfo/MainPackageIdentityName
						/StoreAssociation/ProductReservedInfo/ReservedNames[position() = 1]/ReservedName
						/StoreAssociation/PackageInfoList/@LandingUrl

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


	[xml] $storeAssociation = Get-Content $template
	[xml] $package = Get-Content $packageInfo

	$storeAssociation.StoreAssociation.Publisher = $package.Package.Identity.Publisher
	$storeAssociation.StoreAssociation.PublisherDisplayName = $package.Package.Properties.PublisherDisplayName

	$storeAssociation.StoreAssociation.ProductReservedInfo.MainPackageIdentityName = $package.Package.Identity.Name

	$reservedName = $storeAssociation.StoreAssociation.ProductReservedInfo.ReservedNames | Select-Object -First 1
	$reservedName.InnerXml = "<ReservedName xmlns=`"http://schemas.microsoft.com/appx/2010/storeassociation`">$($package.Package.ReservedName)</ReservedName>"

	$storeAssociation.StoreAssociation.PackageInfoList.LandingUrl = "https://dev.windows.com/dashboard/Application?appId=$($package.Package.AppId)"


	if (-not $outDir)
		{ Write-Output $storeAssociation.OuterXml }

	else
	{
		[Environment]::CurrentDirectory = $PWD
		$path = Join-Path -Path $outDir -ChildPath "Package.StoreAssociation.xml"
		if (-not $whatIf)
		{
			$storeAssociation.Save($path)
		}
		else
		{
			Write-Host "Saving $($path)..."
		}
	}
}