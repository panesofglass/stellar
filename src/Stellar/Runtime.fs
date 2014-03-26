/// Provides types for managing Cloud Services.
module Stellar.Runtime

open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Linq
open System.Net
open System.Reflection
open System.Security.Cryptography.X509Certificates
open System.Xml.Linq
open System.Xml.XPath
open Microsoft.FSharp.Core.CompilerServices
open Newtonsoft.Json.Linq
open ProviderImplementation.ProvidedTypes
open Stellar.Json

[<Literal>]
let private AzureNamespace = "http://schemas.microsoft.com/windowsazure"
let private ServiceName = XName.Get("ServiceName", AzureNamespace)
let private Name = XName.Get("Name", AzureNamespace)

let inline internal (!!) name = XName.Get(name, AzureNamespace)

type HostedServiceDeployment internal (element: XElement) =
    let name = element.Element(!!"Name").Value
    let slot = element.Element(!!"DeploymentSlot").Value
    let privateId = element.Element(!!"PrivateId").Value
    let status = element.Element(!!"Status").Value
    let label = element.Element(!!"Label").Value
    let url = Uri(element.Element(!!"Url").Value)
    let configuration = Text.Encoding.UTF8.GetString(Convert.FromBase64String(element.Element(!!"Configuration").Value))

    member x.Name = name
    member x.DeploymentSlot = slot
    member x.PrivateId = privateId
    member x.Status = status
    member x.Label = label
    member x.Url = url
    member x.Configuration = configuration

type HostedServiceProperties internal (element: XElement) =
    let affinityGroup = element.Element(!!"AffinityGroup").Value
    let location = element.Element(!!"Location").Value
    let label = element.Element(!!"Label").Value
    let status = element.Element(!!"Status").Value
    let dateCreated = DateTime.Parse(element.Element(!!"DateCreated").Value)
    let dateLastModified = DateTime.Parse(element.Element(!!"DateLastModified").Value)
    let description = let d = element.Element(!!"Description").Value in
                      if String.IsNullOrEmpty(d) then None else Some d
    let extendedProperties =
        let Name = !!"Name"
        let Value = !!"Value"
        dict [ for el in element.Element(!!"ExtendedProperties").Elements() -> el.Element(Name).Value, el.Element(Value).Value ]

    member x.Description = description
    member x.AffinityGroup = affinityGroup
    member x.Location = element.Element(!!"Location").Value
    member x.Label = element.Element(!!"Label").Value
    member x.Status = element.Element(!!"Status").Value
    member x.DateCreated = DateTime.Parse(element.Element(!!"DateCreated").Value)
    member x.DateLastModified = DateTime.Parse(element.Element(!!"DateLastModified").Value)
    member x.ExtendedProperties = extendedProperties

type HostedService internal (element: XElement) =
    let url = Uri(element.Element(!!"Url").Value)
    let serviceName = element.Element(!!"ServiceName").Value
    let hostedServiceProperties = HostedServiceProperties(element.Element(!!"HostedServiceProperties"))
    let deployments =
        [ for el in element.Elements(!!"Deployments") do
          for d in el.Elements() do
              yield HostedServiceDeployment(d) ]

    member x.Url = url
    member x.ServiceName = serviceName
    member x.HostedServiceProperties = hostedServiceProperties
    member x.Deployments = deployments

type SubscriptionConnection(id, certificate) =
    let cloudServicesUrl = sprintf "https://management.core.windows.net/%s/services/hostedservices" id
    let webSpacesUrl = sprintf "https://management.core.windows.net/%s/services/WebSpaces" id

    let listHostedServices() = async {
        let! response = Http.getXmlResponse cloudServicesUrl certificate
        match response with
        | Some xml ->
            return [ for el in xml.Elements() -> HostedService el ]
        | None -> return []
    }

    let getHostedService serviceName = async {
        let uri = cloudServicesUrl + sprintf "/%s?embed-detail=true" serviceName
        let! xml = Http.getXmlResponse uri certificate
        return xml |> Option.map (fun el -> HostedService el)
    }

    let listWebSpaces() =
        Http.getJsonResponse webSpacesUrl certificate

    let listWebSites webSpaceName =
        let uri = webSpacesUrl + sprintf "/%s/sites/" webSpaceName
        Http.getJsonResponse uri certificate

    member internal x.HostedServices = listHostedServices() |> Async.RunSynchronously
    member internal x.WebSpaces =  listWebSpaces() |> Async.RunSynchronously

    member internal x.GetHostedService(serviceName) =
        getHostedService serviceName
        |> Async.RunSynchronously
        |> Option.get

    member internal x.GetWebSites(webSpaceName) = listWebSites webSpaceName |> Async.RunSynchronously

type IHostedServices =
    abstract GetHostedService : serviceName:string -> HostedService
    abstract AsyncGetHostedService : serviceName:string -> Async<HostedService>

type HostedServices(connection: SubscriptionConnection) =
    let hostedServices = seq { for hostedService in connection.HostedServices -> connection.GetHostedService(hostedService.ServiceName) }
    interface IHostedServices with
        member x.GetHostedService(serviceName) = connection.GetHostedService(serviceName)
        member x.AsyncGetHostedService(serviceName) = async { return connection.GetHostedService(serviceName) }
    interface seq<HostedService> with member x.GetEnumerator() = hostedServices.GetEnumerator()
    interface IEnumerable with member x.GetEnumerator() = (hostedServices :> IEnumerable).GetEnumerator()

type IWebSpaces =
    abstract GetWebSites : webSpaceName:string -> JObject list
    abstract AsyncGetWebSites : webSpaceName:string -> Async<JObject list>

type WebSpaces(connection: SubscriptionConnection) =
    let webSpaces = seq { for webSpace in connection.WebSpaces -> webSpace }
    interface IWebSpaces with
        member x.GetWebSites(webSpaceName) = connection.GetWebSites(webSpaceName)
        member x.AsyncGetWebSites(webSpaceName) = async { return connection.GetWebSites(webSpaceName) }
    interface seq<JObject> with member x.GetEnumerator() = webSpaces.GetEnumerator()
    interface IEnumerable with member x.GetEnumerator() = (webSpaces :> IEnumerable).GetEnumerator()

// Download your Azure publishsettings file using the Windows PowerShell Cmdlets: Get-AzurePublishSettingsFile
// Use your subscriptionId and base64EncodedCert below.

type Subscription(id: string, name: string, encodedCert: string) =
    let certificate = X509Certificate2(Convert.FromBase64String(encodedCert))
    let connection = SubscriptionConnection(id, certificate)
    member x.Id = id
    member x.Name = name
    member x.ManagementCertificate = encodedCert
    member x.GetCloudServices() = HostedServices(connection)
    member x.GetWebSpaces() = WebSpaces(connection)

/// Read the publishsettings file to obtain available subscriptions.
let internal load publishSettingsFile =
    use file = File.OpenRead(publishSettingsFile)
    let document = XDocument.Load(file)
    let nodes = document.XPathSelectElements("/PublishData/PublishProfile/Subscription")
    [ for node in nodes do
        let id = node.Attribute(XName.Get "Id").Value
        let name = node.Attribute(XName.Get "Name").Value
        let encodedCert = node.Attribute(XName.Get "ManagementCertificate").Value
        yield Subscription(id, name, encodedCert) ]

type ISubscriptions =
    abstract GetSubscription : id:string -> Subscription 

type Subscriptions(publishSettingsFile) =
    let subscriptions = load publishSettingsFile
    let enumerable = seq { for s in subscriptions -> s }
    let indexed = dict [ for s in subscriptions -> s.Id, s ]
    interface ISubscriptions with member x.GetSubscription(id) = indexed.[id]
    interface seq<Subscription> with member x.GetEnumerator() = enumerable.GetEnumerator()
    interface IEnumerable with member x.GetEnumerator() = (enumerable :> IEnumerable).GetEnumerator()
