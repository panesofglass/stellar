namespace Stellar

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

/// Provides types for managing WAWS.
module WebSites =
    let getWebSpaces(id, certificate) =
        let uri = sprintf "https://management.core.windows.net/%s/services/WebSpaces" id
        let request = Http.createGetRequest uri certificate
        use response = request.GetResponse() :?> HttpWebResponse
        if response.StatusCode = HttpStatusCode.OK then
            use stream = response.GetResponseStream()
            use reader = new StreamReader(stream)
            use jsonReader = new JsonTextReader(reader)
            let json = JToken.ReadFrom(jsonReader)
            [ for item in json do yield item :?> JObject ]
        else []

//    let getWebSites(credential, webSpaceName) =
//        use client = new WebSiteManagementClient(credential)
//        // TODO: Make this async?
//        client.WebSpaces.ListWebSites(webSpaceName, WebSiteListParameters())
//
//    let private createWebSiteType (site: WebSite) =
//        let name = site.Name
//        let uri = site.Uri
//        let webSiteProperty = ProvidedTypeDefinition(name, Some typeof<obj>)
//
//        [ ProvidedProperty("Name", typeof<string>, GetterCode = (fun args -> <@@ name @@>), IsStatic = true)
//          ProvidedProperty("Uri", typeof<Uri>, GetterCode = (fun args -> <@@ uri @@>), IsStatic = true) ]
//        |> webSiteProperty.AddMembers
//
//        webSiteProperty
//
    let private provideWebSites(credential, name) =
        let webSitesProperty = ProvidedTypeDefinition("Web Sites", Some typeof<obj>)

//        webSitesProperty.AddMembersDelayed(fun _ ->
//            let response = getWebSites(credential, name)
//            response.WebSites
//            |> Seq.map createWebSiteType
//            |> Seq.toList )
//
        webSitesProperty

    let private createWebSpaceType (certificate, webSpace: JObject) =
        let name = webSpace.["Name"] |> string
        let webSpaceProperty = ProvidedTypeDefinition(name, Some typeof<obj>)
        [ for property in webSpace.Properties() do
            // TODO: if typ is nullable, use F# Option type.
            // TODO: cast to correct type.
            let name, typ, value = property.Name, property.Type, property.Value |> string
            yield ProvidedProperty(name, typeof<string>, GetterCode = (fun args -> <@@ value @@>), IsStatic = true) ]
        |> webSpaceProperty.AddMembers
        webSpaceProperty.AddMember(provideWebSites(certificate, name))
        webSpaceProperty

    let internal provideWebSpaces(id, certificate) =
        let webSpacesProperty = ProvidedTypeDefinition("Web Spaces", Some typeof<obj>)

        webSpacesProperty.AddMembersDelayed(fun _ ->
            getWebSpaces(id, certificate)
            |> List.map (fun webSpace -> createWebSpaceType(certificate, webSpace))
        )

        webSpacesProperty
