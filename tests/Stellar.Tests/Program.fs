open System
open System.IO
open System.Net
open System.Security.Cryptography.X509Certificates

type A = Stellar.AzureManagementProvider<"""D:\azure.publishsettings""">

[<EntryPoint>]
let main argv = 
    printfn "%s" <| A.panesofglass.Id
    printfn "%s" <| A.panesofglass.``Web Spaces``.eastasiawebspace.GeoLocation
    printfn "%s" <| A.panesofglass.``Web Spaces``.eastuswebspace.AvailabilityState
    //printfn "%s" <| A.panesofglass.``Web Spaces``.eastuswebspace.``Web Sites``.

    0 // return an integer exit code
