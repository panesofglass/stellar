namespace Stellar

open System
open System.Security.Cryptography.X509Certificates
open Microsoft.WindowsAzure

module private Management =

    // Download your Azure publishsettings file using the Windows PowerShell Cmdlets: Get-AzurePublishSettingsFile
    // Use your subscriptionId and base64EncodedCert below.

    let getCredential subscriptionId base64EncodedCert =
        CertificateCloudCredentials(subscriptionId, X509Certificate2(Convert.FromBase64String(base64EncodedCert)))
        :> SubscriptionCloudCredentials
