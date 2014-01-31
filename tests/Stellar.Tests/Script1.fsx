#I "../../src/Stellar/bin/Debug"
#r "Newtonsoft.Json.dll"
#r "Microsoft.Threading.Tasks.dll"
#r "Microsoft.Threading.Tasks.Extensions.dll"
#r "System.Net.dll"
#r "System.Net.Http.dll"
#r "System.Net.Http.WebRequest.dll"
#r "System.Net.Http.Primitives.dll"
#r "System.Net.Http.Extensions.dll"
#r "Microsoft.WindowsAzure.Common.dll"
#r "Microsoft.WindowsAzure.Common.NetFramework.dll"
#r "Microsoft.WindowsAzure.Management.WebSites.dll"
#r "Stellar.dll"

type A = Stellar.AzureManagementProvider<"""D:\azure.publishsettings""">

let credential = Stellar.Subscriptions.getCredential A.panesofglass.Id A.panesofglass.ManagementCertificate
let webSpaces = Stellar.CloudServices.getWebSpaces credential
let webSites =
    webSpaces
    |> Seq.collect (fun webSpace -> Stellar.CloudServices.getWebSites(credential, webSpace.Name))
for webSite in webSites do printfn "%s" webSite.Name

