type A = Stellar.AzureManagementProvider<"""D:\azure.publishsettings""">

[<EntryPoint>]
let main argv = 
    A.panesofglass.Id |> printfn "%s"
    0 // return an integer exit code
