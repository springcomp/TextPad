<## 

	This CmdLet creates a package information file that can be modified
	and used to generate the package manifest and store association files.

	Both .\Package.appxmanifest and .\Package.StoreAssociation.xml files
	are used in Visual Studio to build and deploy a Universal Windows Platform (UAP)
	application to the Windows Store.

	Usage:

		New-PackageInfo [-Path <path-to-target-package-xml-file>] `
						[-PackageIdentity <package-identity>] `
						[-PackageName <package-name] `
						[-PackageDisplayName <package-display-name>] `
						[-PackageVersion <package-version>] `
						[-PackageDescription <package-description>] `
						[-PublisherDisplayName <publisher-display-name>] `
						[-PublisherCertificate <publisher-certificate>] `
						[-PhonePublisherId <phone-publisher-id>] `
						[-PhoneProductId <phone-product-id>] `
						[-PackageAppId <package-app-id>] `
						[-PackageReservedName <package-reserved-name>]
						
	Parameters:

		All parameters are optional.

		If the -Path parameter is not specified, the resulting file will be
		written to the standard console output.

		All specified parameters will end up in the resulting file.
		Omitted parameters will have default values with descriptive names.

 ##>
[CmdLetBinding()]
param(
	[String]$path = $null,
	[String]$packageAppId = "app-id",
	[String]$packageIdentity = "package.identity",
	[String]$packageName = "package.name",
	[String]$packageDisplayName = "package.displayname",
	[String]$packageReservedName = "package.reservedname",
	[String]$packageVersion = "1.1.0.0",
	[String]$packageDescription = "package.description",
	[String]$publisherDisplayName = "publisher.displayname",
	[String]$publisherCertificate = "publisher.certificate",
	[String]$phonePublisherId = "phone.publisher.id",
	[String]$phoneProductId = "phone.product.id"
)

PROCESS
{
	$document = `
@"
<?xml version="1.0" encoding="UTF-8" ?>
<Package AppId="$($packageAppId)" ReservedName="$($packageReservedName)">
  <Identity Name="$($packageIdentity)" Publisher="$($packageCertificate)" Version="$($packageVersion)" />
  <PhoneIdentity PhoneProductId="$($phoneProductId)" PhonePublisherId="$($phonePublisherId)" />
  <Properties>
    <DisplayName>$($packageDisplayName)</DisplayName>
    <PublisherDisplayName>$($publisherDisplayName)</PublisherDisplayName>
  </Properties>
  <Application DisplayName="$($packageName)" Description="$($packageDescription)" />
</Package>      
"@

	if (-not $path)
		{ Write-Output ([xml] $document).OuterXml }

	else
	{
		[Environment]::CurrentDirectory = $PWD
		([xml] $document).Save($path)
	}
}