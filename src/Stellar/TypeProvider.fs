namespace Stellar.Management

open System
open System.Reflection
open System.Security.Cryptography.X509Certificates
open Microsoft.FSharp.Core.CompilerServices
open Newtonsoft.Json.Linq
open ProviderImplementation.ProvidedTypes
open Stellar.Json
open Stellar.Runtime

[<TypeProvider>]
type AzureManagementProvider(config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()

    let ns = "Stellar"
    let asm = Assembly.GetExecutingAssembly()

    let buildTypes (typeName, publishSettingsFile) =
        // Create the top-level type.
        let typeProvider = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)

        [ for subscription in Subscriptions(publishSettingsFile) do
            let id, name, encodedCert = subscription.Id, subscription.Name, subscription.ManagementCertificate

            // Create the subscription type
            let subscriptionType = ProvidedTypeDefinition(name, Some typeof<Subscription>, HideObjectMethods = true)
            [ ProvidedProperty("Id", typeof<string>, IsStatic = true, GetterCode = (fun args -> <@@ id @@>))
              ProvidedProperty("ManagementCertificate", typeof<string>, IsStatic = true, GetterCode = (fun args -> <@@ encodedCert @@>)) ]
            |> subscriptionType.AddMembers

            let cloudServicesType =
                let t = ProvidedTypeDefinition("Cloud Services", Some typeof<HostedServices>, HideObjectMethods = true)
                t.AddXmlDoc("<summary>Lists cloud services available in the selected subscription</summary>")
                t.AddMembersDelayed(fun _ ->
                    [ for hostedService in subscription.GetCloudServices() do
                        let serviceName = hostedService.ServiceName
                        let prop =
                            ProvidedProperty
                              ( serviceName, typeof<HostedService>, IsStatic = false,
                                GetterCode = (fun arg -> <@@ ((%%arg.[0] : HostedServices) :> IHostedServices).GetHostedService(serviceName) @@>))
                        match hostedService.HostedServiceProperties.Description with
                        | Some description -> prop.AddXmlDoc(description)
                        | _ -> ()
                        yield prop ]
                )
                t
            subscriptionType.AddMember cloudServicesType

//            let webSpacesType =
//                let t = ProvidedTypeDefinition("Web Spaces", Some typeof<obj>)
//                t.AddMembersDelayed(fun _ ->
//
//                    let createWebSiteType (site: JObject) =
//                        let name = site.["Name"] |> string
//                        let webSiteProperty = ProvidedTypeDefinition(name, Some typeof<obj>)
//                        site.Properties()
//                        |> Seq.choose JProperty.ToProvidedProperty
//                        |> Seq.toList
//                        |> webSiteProperty.AddMembers
//                        webSiteProperty
//
//                    let provideWebSites(id, certificate, name) =
//                        let webSitesProperty = ProvidedTypeDefinition("Web Sites", Some typeof<obj>)
//                        webSitesProperty.AddMembersDelayed(fun _ ->
//                            getWebSites(id, certificate, name)
//                            |> Async.RunSynchronously
//                            |> List.map createWebSiteType
//                        )
//                        webSitesProperty
//
//                    let createWebSpaceType (id, certificate, webSpace: JObject) =
//                        let name = webSpace.["Name"] |> string
//                        let webSpaceProperty = ProvidedTypeDefinition(name, Some typeof<obj>)
//                        webSpace.Properties()
//                        |> Seq.choose JProperty.ToProvidedProperty
//                        |> Seq.toList
//                        |> webSpaceProperty.AddMembers
//                        webSpaceProperty.AddMember(provideWebSites(id, certificate, name))
//                        webSpaceProperty
//
//                    subscription.GetWebSpaces()
//                    |> List.map (fun webSpace -> createWebSpaceType(id, certificate, webSpace))
//                )
//                t
//            subscriptionType.AddMember webSpacesType

            yield subscriptionType ]
        |> typeProvider.AddMembers

        typeProvider

    let parameters =
        [ ProvidedStaticParameter("publishSettingsFile", typeof<string>, String.Empty) ]

    let azureManagementType =
        let t = ProvidedTypeDefinition(asm, ns, "AzureManagementProvider", Some typeof<obj>)
        t.DefineStaticParameters(parameters, fun typeName args ->
            let publishSettingsFile = args.[0] :?> string
            buildTypes(typeName, publishSettingsFile))
        t

    do this.AddNamespace(ns, [ azureManagementType ])

[<assembly:TypeProviderAssembly>]
do ()
