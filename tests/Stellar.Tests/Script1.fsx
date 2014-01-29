#r "../../src/Stellar/bin/Debug/Stellar.dll"

type A = Stellar.AzureManagementProvider<"""D:\azure.publishsettings""">

// Print the Id and ManagementCertificate for a subscription.
A.``Pay-As-You-Go``.Id |> printfn "%s"
A.``Pay-As-You-Go``.ManagementCertificate |> printfn "%s"
