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
    let private createWebSiteType (site: WebSite) =
        Console.WriteLine("Inside createWebSiteType")
        let name = site.Name
        let uri = site.Uri
        let webSiteProperty = ProvidedTypeDefinition(name, Some typeof<obj>)
        webSiteProperty.AddMembers(
            [ ProvidedProperty("Name", typeof<string>, GetterCode = (fun args -> <@@ name @@>), IsStatic = true)
              ProvidedProperty("Uri", typeof<string>, GetterCode = (fun args -> <@@ uri @@>), IsStatic = true) ]
        )
        webSiteProperty

    let private getWebSites(credential, name) =
        Console.WriteLine("Inside getWebSites")
        // TODO: Don't add this as a property if no web sites are in use.
        let webSitesProperty = ProvidedTypeDefinition("Web Sites", Some typeof<obj>)
        webSitesProperty.AddMembersDelayed(fun _ ->
            Console.WriteLine("Creating WebSiteManagementClient")
            use client = new WebSiteManagementClient(credential)
            Console.WriteLine("Created WebSiteManagementClient")
            // TODO: Make this async?
            client.WebSpaces.ListWebSites(name, WebSiteListParameters(PropertiesToInclude = [| "Name"; "Uri" |]))
            |> Seq.map createWebSiteType
            |> Seq.toList
        )
        Console.WriteLine("Returning property")
        webSitesProperty

    let private createWebSpaceType (credential, webSpace: WebSpacesListResponse.WebSpace) =
        let name = webSpace.Name
        let webSpaceProperty = ProvidedTypeDefinition(name, Some typeof<obj>)

        let geoLocation = webSpace.GeoLocation
        let geoRegion = webSpace.GeoRegion
        let currentWorkerSize = if webSpace.CurrentWorkerSize.HasValue then Some (webSpace.CurrentWorkerSize.Value.ToString()) else None
        let currentNumberOfWorkers = if webSpace.CurrentNumberOfWorkers.HasValue then Some webSpace.CurrentNumberOfWorkers.Value else None
        webSpaceProperty.AddMembers(
            [ ProvidedProperty("Name", typeof<string>, GetterCode = (fun args -> <@@ name @@>), IsStatic = true) :> MemberInfo
              ProvidedProperty("Geographic location", typeof<string>, GetterCode = (fun args -> <@@ geoLocation @@>), IsStatic = true) :> MemberInfo
              ProvidedProperty("Geographic region", typeof<string>, GetterCode = (fun args -> <@@ geoRegion @@>), IsStatic = true) :> MemberInfo
              ProvidedProperty("Current worker size", typeof<string option>, GetterCode = (fun args -> <@@ currentWorkerSize @@>), IsStatic = true) :> MemberInfo
              ProvidedProperty("Current number of workers", typeof<int option>, GetterCode = (fun args -> <@@ currentNumberOfWorkers @@>), IsStatic = true) :> MemberInfo
              getWebSites(credential, name) :> MemberInfo ]
        )
        webSpaceProperty

    let getWebSpaces credential =
        // TODO: Don't add this as a property if no web spaces are in use.
        let webSpacesProperty = ProvidedTypeDefinition("Web Spaces", Some typeof<obj>)
        webSpacesProperty.AddMembersDelayed(fun _ ->
            use client = new WebSiteManagementClient(credential)
            // TODO: Make this async?
            client.WebSpaces.List()
            |> Seq.map (fun webSpace -> createWebSpaceType(credential, webSpace))
            |> Seq.toList
        )
        webSpacesProperty
