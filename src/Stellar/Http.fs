namespace Stellar

open System
open System.Net

module internal Http =
    type Result<'T> =
        | Success of result: 'T
        | Failure of statusCode: HttpStatusCode * message: string

    // Methods
    let [<Literal>] GET    = "GET"
    let [<Literal>] PUT    = "PUT"
    let [<Literal>] POST   = "POST"
    let [<Literal>] DELETE = "DELETE"

    // Header values
    let [<Literal>] ``application/json`` = "application/json"
    let [<Literal>] ``x-ms-version``     = "x-ms-version"

    // Helper functions
    let inline makeRequest httpMethod (uri: string) certificate =
        let request = HttpWebRequest.Create(uri) :?> HttpWebRequest
        request.Method <- httpMethod
        request.Accept <- ``application/json``
        request.ContentType<- ``application/json``
        request.Headers.Add("x-ms-version", "2013-11-01")
        request.ClientCertificates.Add(certificate) |> ignore
        request

    let createGetRequest (uri: string) certificate =
        makeRequest GET uri certificate
