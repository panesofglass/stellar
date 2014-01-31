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

/// Provides types for managing subscriptions.
module Subscriptions =
    type Subscription = Subscription of id: string * name: string * base64EncodedCertificate: string
    
    // Download your Azure publishsettings file using the Windows PowerShell Cmdlets: Get-AzurePublishSettingsFile
    // Use your subscriptionId and base64EncodedCert below.

    /// Convert the subscription Id and Base 64 encoded certificate into an instance of SubscriptionCloudCredentials.
    let getCredential id encodedCert =
        CertificateCloudCredentials(id, X509Certificate2(Convert.FromBase64String(encodedCert)))
        :> SubscriptionCloudCredentials

    /// Read the publishsettings file to obtain available subscriptions.
    let load publishSettingsFile =
        use file = File.OpenRead(publishSettingsFile)
        let document = XDocument.Load(file)
        let nodes = document.XPathSelectElements("/PublishData/PublishProfile/Subscription")
        [ for node in nodes do
            let id = node.Attribute(XName.Get "Id").Value
            let name = node.Attribute(XName.Get "Name").Value
            let encodedCert = node.Attribute(XName.Get "ManagementCertificate").Value
            yield Subscription(id, name, encodedCert) ]

    /// Generate a type for the subscription.
    let private createSubscriptionType (Subscription(id, name, encodedCert)) =
        let credential = getCredential id encodedCert

        // Create the subscription type
        let subscriptionProperty = ProvidedTypeDefinition(name, Some typeof<obj>)
        [ ProvidedProperty("Id", typeof<string>, GetterCode = (fun args -> <@@ id @@>), IsStatic = true) :> MemberInfo
          ProvidedProperty("ManagementCertificate", typeof<string>, GetterCode = (fun args -> <@@ encodedCert @@>), IsStatic = true) :> MemberInfo
          CloudServices.provideWebSpaces credential :> MemberInfo ]
        |> subscriptionProperty.AddMembers

        subscriptionProperty

    let internal provideSubscriptions publishSettingsFile =
        load publishSettingsFile
        |> List.map createSubscriptionType
