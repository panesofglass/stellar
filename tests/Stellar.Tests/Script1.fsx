#I "../../src/Stellar/bin/Debug"
#r "Newtonsoft.Json.dll"
#r "System.Net.dll"
#r "Stellar.dll"

open System
open System.Security.Cryptography.X509Certificates

type A = Stellar.AzureManagementProvider<"""D:\azure.publishsettings""">

let id = A.panesofglass.Id
let certificate = X509Certificate2(Convert.FromBase64String(A.panesofglass.ManagementCertificate))

let webSpaces = Stellar.WebSites.getWebSpaces(id, certificate)
let webSites =
    webSpaces
    |> Seq.collect (fun webSpace -> Stellar.WebSites.getWebSites(id, certificate, webSpace.["Name"] |> string))
    |> Seq.toArray
printfn "%i" webSites.Length
for webSite in webSites do printfn "%A" (webSite.["Name"] |> string)

let cloudServices = Stellar.CloudServices.getCloudServices(id, certificate)
