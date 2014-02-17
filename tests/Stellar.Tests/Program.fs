type A = Stellar.AzureManagementProvider<"""D:\azure.publishsettings""">

[<EntryPoint>]
let main argv = 
//    printfn "%s" <| A.panesofglass.Id
//    printfn "%s" <| A.panesofglass.``Web Spaces``.eastasiawebspace.``Geographic location``
//    printfn "%s" <| A.panesofglass.``Web Spaces``.eastuswebspace.``Geographic region``
//    printfn "%s" <| defaultArg A.panesofglass.``Web Spaces``.northcentraluswebspace.``Current worker size`` "None"
    printfn "%s" <| A.panesofglass.``Web Spaces``.eastuswebspace.``Web Sites``.wizardsofsmart.Name

    let cred = Stellar.Subscriptions.getCredential A.panesofglass.Id A.panesofglass.ManagementCertificate
    let webSpacesResponse = Stellar.WebSites.getWebSpaces cred
    printfn "%A" webSpacesResponse.StatusCode
    printfn "%d" webSpacesResponse.WebSpaces.Count
    for webSpace in webSpacesResponse.WebSpaces do
        let webSitesResponse = Stellar.WebSites.getWebSites (cred, webSpace.Name)
        printfn "%A" webSitesResponse.StatusCode
        printfn "%d" webSitesResponse.WebSites.Count
        for site in webSitesResponse.WebSites do
            printfn "%s" site.Name

    0 // return an integer exit code
