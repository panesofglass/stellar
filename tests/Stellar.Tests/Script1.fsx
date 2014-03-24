#I "../../src/Stellar/bin/Debug"
#r "Newtonsoft.Json.dll"
#r "System.Net.dll"
#r "System.Xml.Linq.dll"
#r "Stellar.dll"

open System
open System.Security.Cryptography.X509Certificates
open System.Xml.Linq

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
let first = cloudServices |> List.head
let ServiceName = XName.Get("ServiceName", "http://schemas.microsoft.com/windowsazure")
open System.Linq
first.DescendantNodes().FirstOrDefault()
first.Descendants(ServiceName).FirstOrDefault()
first.Element(ServiceName).Value
cloudServices
|> List.collect (fun cloudService ->
    [ for el in cloudService.Elements() do
        let value = el.Value 
        yield name, value ] )
