namespace Stellar

open System
open System.IO
open System.Linq
open System.Reflection
open System.Security.Cryptography.X509Certificates
open System.Xml.Linq
open System.Xml.XPath
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.WindowsAzure
open ProviderImplementation.ProvidedTypes

/// Internal representation of a Subscription, including its credentials
type internal Subscription =
    { Id: string
      Name: string
      ManagementCertificate: string
      Credential: SubscriptionCloudCredentials }

/// Provides types for managing subscriptions.
module internal Subscriptions =
    
    // Download your Azure publishsettings file using the Windows PowerShell Cmdlets: Get-AzurePublishSettingsFile
    // Use your subscriptionId and base64EncodedCert below.

    /// Convert the subscription Id and Base 64 encoded certificate into an instance of SubscriptionCloudCredentials.
    let private getCredential id encodedCert =
        CertificateCloudCredentials(id, X509Certificate2(Convert.FromBase64String(encodedCert)))
        :> SubscriptionCloudCredentials

    /// Generate a type for the subscription.
    let private createSubscriptionType subscription =
        // Create the subscription type
        let subscriptionProperty = ProvidedTypeDefinition(subscription.Name, Some typeof<obj>)
        [ ProvidedProperty("Id", typeof<string>, GetterCode = (fun args -> <@@ subscription.Id @@>), IsStatic = true)
          ProvidedProperty("ManagementCertificate", typeof<string>, GetterCode = (fun args -> <@@ subscription.ManagementCertificate @@>), IsStatic = true) ]
        |> subscriptionProperty.AddMembers

        // TODO: Lazily create clients as child properties of each subscription.

        subscriptionProperty

    /// Read the publishsettings file to obtain available subscriptions.
    let load publishSettingsFile =
        use file = File.OpenRead(publishSettingsFile)
        let document = XDocument.Load(file)
        let nodes = document.XPathSelectElements("/PublishData/PublishProfile/Subscription")
        [ for node in nodes do
            let id = node.Attribute(XName.Get "Id").Value
            let name = node.Attribute(XName.Get "Name").Value
            let encodedCert = node.Attribute(XName.Get "ManagementCertificate").Value
            let subscription =
                { Id = id
                  Name = name
                  ManagementCertificate = encodedCert
                  Credential = getCredential id encodedCert }
            yield createSubscriptionType subscription ]
