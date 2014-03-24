type A = Stellar.AzureManagementProvider<"""D:\azure.publishsettings""">

[<EntryPoint>]
let main argv = 
    A.panesofglass.Id |> printfn "%s"
    A.panesofglass.``Web Spaces``.eastuswebspace.AvailabilityState |> printfn "AvailabilityState %i"
    A.panesofglass.``Web Spaces``.eastuswebspace.Plan |> printfn "Plan %s"
    A.panesofglass.``Web Spaces``.eastuswebspace.``Web Sites``.wizardsofsmart.SelfLink |> printfn "SelfLink %s"
    A.panesofglass.``Cloud Services``.
    0 // return an integer exit code
