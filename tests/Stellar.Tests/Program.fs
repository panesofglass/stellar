open System
open System.Security.Cryptography.X509Certificates
open Stellar.Runtime

type A = Stellar.AzureManagementProvider<"""D:\azure.publishsettings""">

[<EntryPoint>]
let main argv = 
    let cert = X509Certificate2(Convert.FromBase64String A.panesofglass.ManagementCertificate)
    let connection = SubscriptionConnection(A.panesofglass.Id, cert)
    let cloudServices = HostedServices(connection)
    for cs in cloudServices do
        let cloudService = (cloudServices :> IHostedServices).GetHostedService(cs.ServiceName)
        for deployment in cloudService.Deployments do
            printfn "%A" deployment.Name

    0 // return an integer exit code
