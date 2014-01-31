type A = Stellar.AzureManagementProvider<"""D:\azure.publishsettings""">

[<EntryPoint>]
let main argv = 
    printfn "%s" <| A.panesofglass.Id
    printfn "%s" <| A.panesofglass.``Web Spaces``.eastasiawebspace.Name
    printfn "%s" <| A.panesofglass.``Web Spaces``.eastuswebspace.``Geographic region``
    printfn "%s" <| defaultArg A.panesofglass.``Web Spaces``.northcentraluswebspace.``Current worker size`` "None"
    0 // return an integer exit code
