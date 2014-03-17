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

let getCloudServices(id, certificate) =
    let uri = sprintf "https://management.core.windows.net/%s/services/hostedservices" id
    Http.getXmlResponse uri certificate

let private createCloudServiceType (id, certificate, cloudService: XElement) =
    let name = cloudService.Element(XName.Get "ServiceName").Value
    let cloudServiceType = ProvidedTypeDefinition(name, Some typeof<obj>)
    // TODO: Flatten the list.
    // Move HostedServiceProperties up to the root
    // Create properties for the ExtendedProperties key/value pairs
    // TODO: Map to correct types.
    [ for el in cloudService.Elements() do
        let value = el.Value 
        yield ProvidedProperty(name, typeof<string>, GetterCode = (fun args -> <@@ value @@>), IsStatic = true) ]
    |> cloudServiceType.AddMembers
    cloudServiceType

let internal provideCloudServices(id, certificate) =
    let cloudServicesProperty = ProvidedTypeDefinition("Cloud Services", Some typeof<obj>)
    cloudServicesProperty.AddMembersDelayed(fun _ ->
        getCloudServices(id, certificate)
        |> List.map (fun cloudService -> createCloudServiceType(id, certificate, cloudService))
    )
    cloudServicesProperty
