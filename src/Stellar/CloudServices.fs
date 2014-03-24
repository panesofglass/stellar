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

let getCloudServices(id, certificate) =
    let uri = sprintf "https://management.core.windows.net/%s/services/hostedservices" id
    Http.getXmlResponse uri certificate

let rec private createType (name, element: XElement) =
    let providedType = ProvidedTypeDefinition(name, Some typeof<obj>)
    [ for el in element.Elements() do
        let name = el.Name.LocalName
        if el.HasElements then
            let name =
                if name = "ExtendedProperty" then
                    el.Element(Name).Value
                else name
            yield createType (name, el) :> MemberInfo
        else
            let propertyType, value =
                if name.StartsWith("Date") then
                    typeof<DateTime>, box <| DateTime.Parse(el.Value)
                else typeof<string>, box el.Value
            yield ProvidedProperty(name, propertyType, GetterCode = (fun args -> <@@ value @@>), IsStatic = true) :> MemberInfo ]
    |> providedType.AddMembers
    providedType

let private createCloudServiceType (id, certificate, cloudService: XElement) =
    let name = cloudService.Element(ServiceName).Value
    createType(name, cloudService)

let internal provideCloudServices(id, certificate) =
    let cloudServices = ProvidedTypeDefinition("Cloud Services", Some typeof<obj>)
    cloudServices.AddMembersDelayed(fun _ ->
        getCloudServices(id, certificate)
        |> List.map (fun cloudService -> createCloudServiceType(id, certificate, cloudService))
    )
    cloudServices
