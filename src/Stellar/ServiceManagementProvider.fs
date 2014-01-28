module Stellar.ServiceManagementProvider

open System
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes

[<TypeProvider>]
type AzureManagementProvider(config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()

    let ns = "Stellar"
    let asm = Assembly.GetExecutingAssembly()

    let buildTypes (typeName: string) (args: obj[]) =
        // Create the top-level type.
        let typeProviderForSubscription =
            ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)
        let ctor = ProvidedConstructor(parameters = [], InvokeCode = (fun args -> <@@ null @@>))
        typeProviderForSubscription.AddMember(ctor)

        // Create child members for the various management clients.
        // TODO: get credentials
        // TODO: create clients; should these be lazily created? If so, how are they appropriately disposed?

        typeProviderForSubscription

    let parameters =
        [ ProvidedStaticParameter("subscriptionId", typeof<string>, String.Empty)
          ProvidedStaticParameter("base64EncodedCert", typeof<string>, String.Empty) ]

    let azureManagementType =
        ProvidedTypeDefinition(asm, ns, "AzureManagementProvider", Some typeof<obj>)
    do azureManagementType.DefineStaticParameters(parameters, buildTypes)
    do this.AddNamespace(ns, [ azureManagementType ])

[<assembly:TypeProviderAssembly>]
do ()
