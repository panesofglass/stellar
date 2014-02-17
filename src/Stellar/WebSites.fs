namespace Stellar

open System
open System.Diagnostics
open System.IO
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.WindowsAzure
open Microsoft.WindowsAzure.Management.WebSites
open Microsoft.WindowsAzure.Management.WebSites.Models
open ProviderImplementation.ProvidedTypes

/// Provides types for managing WAWS.
module WebSites =
    let getWebSpaces credential =
        use client = new WebSiteManagementClient(credential)
        // TODO: Make this async?
        client.WebSpaces.List()

    let getWebSites(credential, webSpaceName) =
        use client = new WebSiteManagementClient(credential)
        // TODO: Make this async?
        client.WebSpaces.ListWebSites(webSpaceName, WebSiteListParameters())

    let private createWebSiteType (site: WebSite) =
        let name = site.Name
        let uri = site.Uri
        let webSiteProperty = ProvidedTypeDefinition(name, Some typeof<obj>)

        [ ProvidedProperty("Name", typeof<string>, GetterCode = (fun args -> <@@ name @@>), IsStatic = true)
          ProvidedProperty("Uri", typeof<Uri>, GetterCode = (fun args -> <@@ uri @@>), IsStatic = true) ]
        |> webSiteProperty.AddMembers

        webSiteProperty

    let private provideWebSites(credential, name) =
        let webSitesProperty = ProvidedTypeDefinition("Web Sites", Some typeof<obj>)

        webSitesProperty.AddMembersDelayed(fun _ ->
            let response = getWebSites(credential, name)
            response.WebSites
            |> Seq.map createWebSiteType
            |> Seq.toList )

        webSitesProperty

    let private createWebSpaceType (credential, webSpace: WebSpacesListResponse.WebSpace) =
        let name = webSpace.Name
        let webSpaceProperty = ProvidedTypeDefinition(name, Some typeof<obj>)

        let geoLocation = webSpace.GeoLocation
        let geoRegion = webSpace.GeoRegion
        let currentWorkerSize = if webSpace.CurrentWorkerSize.HasValue then Some (webSpace.CurrentWorkerSize.Value.ToString()) else None
        let currentNumberOfWorkers = if webSpace.CurrentNumberOfWorkers.HasValue then Some webSpace.CurrentNumberOfWorkers.Value else None
        [ ProvidedProperty("Name", typeof<string>, GetterCode = (fun args -> <@@ name @@>), IsStatic = true) :> MemberInfo
          ProvidedProperty("Geographic location", typeof<string>, GetterCode = (fun args -> <@@ geoLocation @@>), IsStatic = true) :> MemberInfo
          ProvidedProperty("Geographic region", typeof<string>, GetterCode = (fun args -> <@@ geoRegion @@>), IsStatic = true) :> MemberInfo
          ProvidedProperty("Current worker size", typeof<string option>, GetterCode = (fun args -> <@@ currentWorkerSize @@>), IsStatic = true) :> MemberInfo
          ProvidedProperty("Current number of workers", typeof<int option>, GetterCode = (fun args -> <@@ currentNumberOfWorkers @@>), IsStatic = true) :> MemberInfo
          provideWebSites(credential, name) :> MemberInfo ]
        |> webSpaceProperty.AddMembers

        webSpaceProperty

    let internal provideWebSpaces credential =
        let webSpacesProperty = ProvidedTypeDefinition("Web Spaces", Some typeof<obj>)

        webSpacesProperty.AddMembersDelayed(fun _ ->
            let response = getWebSpaces credential
            response.WebSpaces
            |> Seq.map (fun webSpace -> createWebSpaceType(credential, webSpace))
            |> Seq.toList )

        webSpacesProperty
