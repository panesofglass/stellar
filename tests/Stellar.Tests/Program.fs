type A = Stellar.AzureManagementProvider<"""M:\azure.publishsettings""">

[<EntryPoint>]
let main argv = 
    A.panesofglass.Id |> printfn "%s"
    A.panesofglass.``Web Spaces``.eastuswebspace.AvailabilityState |> printfn "AvailabilityState %i"
    A.panesofglass.``Web Spaces``.eastuswebspace.Plan |> printfn "Plan %s"
    //A.panesofglass.``Web Spaces``.eastuswebspace.``Web Sites``. |> printfn "%s"

    0 // return an integer exit code
