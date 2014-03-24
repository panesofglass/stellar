/// Provides types for managing Cloud Services.
module Stellar.CloudServices

open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Net
open System.Reflection
open System.Xml.Linq
open System.Xml.XPath
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open Stellar.Json

[<Literal>]
let private AzureNamespace = "http://schemas.microsoft.com/windowsazure"
let private ServiceName = XName.Get("ServiceName", AzureNamespace)
let private Name = XName.Get("Name", AzureNamespace)

let inline (!!) name = XName.Get(name, AzureNamespace)

type HostedServiceProperties =
    { Description: string option
      AffinityGroup: string
      Location: string
      Label: string
      Status: string
      DateCreated: DateTime
      DateLastModified: DateTime 
      ExtendedProperties: (string * string) list }
type HostedService =
    { Url: Uri
      ServiceName: string
      HostedServiceProperties: HostedServiceProperties }
    with
    static member Parse(element: XElement) =
        let props =
            element.Element(!!"HostedServiceProperties")
        let extendedProperties =
            let Name = !!"Name"
            let Value = !!"Value"
            [ for el in props.Element(!!"ExtendedProperties").Elements() do
                yield el.Element(Name).Value, el.Element(Value).Value ]
        let properties =
            {
                Description = let d = props.Element(!!"Description").Value in if String.IsNullOrEmpty(d) then None else Some d
                AffinityGroup = props.Element(!!"AffinityGroup").Value
                Location = props.Element(!!"Location").Value
                Label = props.Element(!!"Label").Value
                Status = props.Element(!!"Status").Value
                DateCreated = DateTime.Parse(props.Element(!!"DateCreated").Value)
                DateLastModified = DateTime.Parse(props.Element(!!"DateLastModified").Value)
                ExtendedProperties = extendedProperties
            }
        {
            Url = Uri(element.Element(!!"Url").Value)
            ServiceName = element.Element(!!"ServiceName").Value
            HostedServiceProperties = properties
        }

let getCloudServices(id, certificate) =
    let uri = sprintf "https://management.core.windows.net/%s/services/hostedservices" id
    Http.getXmlResponse uri certificate

let getCloudService(id, certificate, cloudServiceName) =
    let uri = sprintf "https://management.core.windows.net/%s/services/hostedservices/%s?embed-detail=true" id cloudServiceName
    Http.getXmlResponse uri certificate

let rec private createProperty (name, element: XElement) =
    let name = element.Name.LocalName
    if element.HasElements then
        let name =
            if name = "ExtendedProperty" then
                element.Element(Name).Value
            else name
        createType (name, element) :> MemberInfo
    else
        let propertyType, value =
            if name.StartsWith("Date") then
                typeof<DateTime>, box <| DateTime.Parse(element.Value)
            else typeof<string>, box element.Value
        ProvidedProperty(name, propertyType, GetterCode = (fun args -> <@@ value @@>), IsStatic = true) :> MemberInfo

and private createType (name, element: XElement) =
    let providedType = ProvidedTypeDefinition(name, Some typeof<obj>)
    [ for el in element.Elements() do
        yield createProperty(name, el) ]
    |> providedType.AddMembers
    providedType

let private createCloudServiceType (id, certificate, cloudService: XElement) =
    let name = cloudService.Element(ServiceName).Value
    let cloudServiceType = ProvidedTypeDefinition(name, Some typeof<obj>)
    cloudServiceType.AddMembersDelayed(fun _ ->
        let cloudService = getCloudService(id, certificate, name)
        [ for el in cloudService.Elements() do
            yield createProperty(name, el) ]
    )
    cloudServiceType

let internal provideCloudServices(id, certificate) =
    let cloudServices = ProvidedTypeDefinition("Cloud Services", Some typeof<obj>)
    cloudServices.AddMembersDelayed(fun _ ->
        getCloudServices(id, certificate)
        //|> List.map (fun cloudService -> createCloudServiceType(id, certificate, cloudService))
        |> List.map (fun cloudService ->
            let hostedService = HostedService.Parse cloudService
            ProvidedProperty(hostedService.ServiceName,
                             typeof<HostedService>,
                             GetterCode = (fun args -> <@@ hostedService @@>),
                             IsStatic = true))
    )
    cloudServices
