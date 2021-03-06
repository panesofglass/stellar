﻿/// Provides types for managing subscriptions.
module Stellar.Subscriptions

open System
open System.IO
open System.Linq
open System.Reflection
open System.Security.Cryptography.X509Certificates
open System.Xml.Linq
open System.Xml.XPath
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes

type Subscription = Subscription of id: string * name: string * base64EncodedCertificate: string

// Download your Azure publishsettings file using the Windows PowerShell Cmdlets: Get-AzurePublishSettingsFile
// Use your subscriptionId and base64EncodedCert below.

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
    let certificate = X509Certificate2(Convert.FromBase64String(encodedCert))

    // Create the subscription type
    let subscriptionProperty = ProvidedTypeDefinition(name, Some typeof<obj>)
    [ ProvidedProperty("Id", typeof<string>, GetterCode = (fun args -> <@@ id @@>), IsStatic = true)
      ProvidedProperty("ManagementCertificate", typeof<string>, GetterCode = (fun args -> <@@ encodedCert @@>), IsStatic = true) ]
    |> subscriptionProperty.AddMembers
      
    [ CloudServices.provideCloudServices(id, certificate)
      WebSites.provideWebSpaces(id, certificate) ]
    |> subscriptionProperty.AddMembers

    subscriptionProperty

let internal provideSubscriptions publishSettingsFile =
    load publishSettingsFile
    |> List.map createSubscriptionType
