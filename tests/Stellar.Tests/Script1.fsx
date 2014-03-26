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
