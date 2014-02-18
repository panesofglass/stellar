module internal Stellar.Http

open System
open System.IO
open System.Net
open Newtonsoft.Json
open Newtonsoft.Json.Linq

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

let getJsonRequest (uri: string) certificate =
    let request = makeRequest GET uri certificate
    use response = request.GetResponse() :?> HttpWebResponse
    if response.StatusCode = HttpStatusCode.OK then
        use stream = response.GetResponseStream()
        use reader = new StreamReader(stream)
        use jsonReader = new JsonTextReader(reader)
        let json = JToken.ReadFrom(jsonReader)
        [ for item in json do yield item :?> JObject ]
    else []
