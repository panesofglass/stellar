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
open Microsoft.WindowsAzure.Management.WebSites
open Microsoft.WindowsAzure.Management.WebSites.Models
open ProviderImplementation.ProvidedTypes

/// Provides types for managing cloud services.
module internal CloudServices =
    type Client = Microsoft.WindowsAzure.Management.WebSites.WebSiteManagementClient

    let private createWebSiteType(name, uri) =
        let webSiteProperty = ProvidedTypeDefinition(name, Some typeof<obj>)
        webSiteProperty.AddMembersDelayed(fun _ ->
            [ ProvidedProperty("Uri", typeof<string>, GetterCode = (fun args -> <@@ uri @@>), IsStatic = true)]
        )
        webSiteProperty

    let private getWebSites(client: Client, name) =
        // TODO: Don't add this as a property if no web sites are in use.
        let webSitesProperty = ProvidedTypeDefinition("Web Sites", Some typeof<obj>)
        webSitesProperty.AddMembersDelayed(fun _ ->
            // TODO: Make this async?
            client.WebSpaces.ListWebSites(name, WebSiteListParameters(PropertiesToInclude = [| "Name"; "Uri" |]))
            |> Seq.map (fun site -> createWebSiteType(site.Name, site.Uri))
            |> Seq.toList
        )
        webSitesProperty

    let private createWebSpaceType (client, name, geoLocation, geoRegion, workerSize, numWorkers) =
        let webSpaceProperty = ProvidedTypeDefinition(name, Some typeof<obj>)
        webSpaceProperty.AddMembersDelayed(fun _ ->
            [ getWebSites(client, name) :> MemberInfo
              ProvidedProperty("Geographic location", typeof<string>, GetterCode = (fun args -> <@@ geoLocation @@>), IsStatic = true) :> MemberInfo
              ProvidedProperty("Geographic region", typeof<string>, GetterCode = (fun args -> <@@ geoRegion @@>), IsStatic = true) :> MemberInfo
              ProvidedProperty("Current worker size", typeof<string>, GetterCode = (fun args -> <@@ workerSize @@>), IsStatic = true) :> MemberInfo
              ProvidedProperty("Current number of workers", typeof<Nullable<int>>, GetterCode = (fun args -> <@@ numWorkers @@>), IsStatic = true) :> MemberInfo ]
        )
        webSpaceProperty

    let getWebSpaces credential =
        use client = new Client(credential)   
        // TODO: Don't add this as a property if no web spaces are in use.
        let webSpacesProperty = ProvidedTypeDefinition("Web Spaces", Some typeof<obj>)
        webSpacesProperty.AddMembersDelayed(fun _ ->
            // TODO: Make this async?
            client.WebSpaces.List()
            |> Seq.map (fun space ->
                createWebSpaceType(client,
                                   space.Name,
                                   space.GeoLocation,
                                   space.GeoRegion,
                                   space.CurrentWorkerSize.GetValueOrDefault().ToString(),
                                   space.CurrentNumberOfWorkers))
            |> Seq.toList
        )
        webSpacesProperty
