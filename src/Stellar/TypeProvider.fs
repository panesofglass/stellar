namespace Stellar.Management

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
        let typeProvider =
            ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)
        let ctor = ProvidedConstructor(parameters = [], InvokeCode = (fun args -> <@@ null @@>))
        typeProvider.AddMember(ctor)

        // Create child members for the various management clients.
        let publishSettingsFile = args.[0] :?> string
        typeProvider.AddMembers(Stellar.Subscriptions.load publishSettingsFile)

        typeProvider

    let parameters =
        [ ProvidedStaticParameter("publishSettingsFile", typeof<string>, String.Empty) ]

    let azureManagementType =
        ProvidedTypeDefinition(asm, ns, "AzureManagementProvider", Some typeof<obj>)
    do azureManagementType.DefineStaticParameters(parameters, buildTypes)
    do this.AddNamespace(ns, [ azureManagementType ])

[<assembly:TypeProviderAssembly>]
do ()
