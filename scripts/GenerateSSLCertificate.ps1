
$certStore = "cert:\CurrentUser\My"
$cert = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation $certStore
Write-Host $cert
$PFXPass = ConvertTo-SecureString -String “cert-password” -Force -AsPlainText
$certLocation = $certStore + "\" + $cert.Thumbprint
Write-Host $certLocation
$exportDir = $PSScriptRoot + "\..\Vonk.IdentityServer.Test\ssl_cert.pfx"
Export-PfxCertificate -Cert $certLocation -Password $PFXPass -FilePath "c:\git\Vonk.IdentityServer.Test\Vonk.IdentityServer.Test\ssl_cert.pfx"
