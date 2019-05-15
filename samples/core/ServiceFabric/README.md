For settings encryption (as well as https) on local cluster you could use cert/localhost.pfx (password: P@ssw0rd)
Location: LocalMachine\My
Thumbprint: bf 58 dc b3 5b a7 f1 7a 29 bb da 6b 17 f4 9d a9 3c 0a 3c e5

Here are steps to create your own certificate:
1. New-SelfSignedCertificate -DnsName 'yourName' -CertStoreLocation cert:\LocalMachine\My -Provider "Microsoft Strong Cryptographic Provider"

In this case new certificate will be produced with sha1RSA signature algorythm.
Be aware! By default (without -Provider setting) certificate with sha256 will be generated - which is unacceptable for SF cluster.

2. For cert export you can use:
$CertPassword = ConvertTo-SecureString -String "your password here" -Force –AsPlainText
Export-PfxCertificate -Cert cert:\LocalMachine\My\<generated cert thumbprint> -FilePath <path to new pfx file> -Password $CertPassword

3. For setting encryption
Invoke-ServiceFabricEncryptText -Text <value> -CertThumbprint <thumbprint> -CertStore -StoreLocation LocalMachine -StoreName My

Ex:
Invoke-ServiceFabricEncryptText -Text "default value" -CertThumbprint "bf 58 dc b3 5b a7 f1 7a 29 bb da 6b 17 f4 9d a9 3c 0a 3c e5" -CertStore -StoreLocation LocalMachine -StoreName My

Please use Administrator Visual Studio in case service will be able to find certificate.
For production usage please refer to RunAs policy.