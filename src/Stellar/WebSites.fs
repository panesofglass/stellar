/// Provides types for managing WAWS.
module Stellar.WebSites

open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Net
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open ProviderImplementation.ProvidedTypes
open Stellar.Json

let getWebSpaces(id, certificate) =
    let uri = sprintf "https://management.core.windows.net/%s/services/WebSpaces" id
    Http.getJsonResponse uri certificate

let getWebSites(id, certificate, webSpaceName) =
    let uri = sprintf "https://management.core.windows.net/%s/services/WebSpaces/%s/sites/" id webSpaceName
    Http.getJsonResponse uri certificate

let private createWebSiteType (site: JObject) =
    let name = site.["Name"] |> string
    let webSiteProperty = ProvidedTypeDefinition(name, Some typeof<obj>)
    site.Properties()
    |> Seq.choose JProperty.ToProvidedProperty
    |> Seq.toList
    |> webSiteProperty.AddMembers
    webSiteProperty

let private provideWebSites(id, certificate, name) =
    let webSitesProperty = ProvidedTypeDefinition("Web Sites", Some typeof<obj>)
    webSitesProperty.AddMembersDelayed(fun _ ->
        getWebSites(id, certificate, name)
        |> List.map createWebSiteType
    )
    webSitesProperty

let private createWebSpaceType (id, certificate, webSpace: JObject) =
    let name = webSpace.["Name"] |> string
    let webSpaceProperty = ProvidedTypeDefinition(name, Some typeof<obj>)
    webSpace.Properties()
    |> Seq.choose JProperty.ToProvidedProperty
    |> Seq.toList
    |> webSpaceProperty.AddMembers
    webSpaceProperty.AddMember(provideWebSites(id, certificate, name))
    webSpaceProperty

let internal provideWebSpaces(id, certificate) =
    let webSpacesProperty = ProvidedTypeDefinition("Web Spaces", Some typeof<obj>)
    webSpacesProperty.AddMembersDelayed(fun _ ->
        getWebSpaces(id, certificate)
        |> List.map (fun webSpace -> createWebSpaceType(id, certificate, webSpace))
    )
    webSpacesProperty
