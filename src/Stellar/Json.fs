module Stellar.Json

open System
open Newtonsoft.Json.Linq
open ProviderImplementation.ProvidedTypes

let internal convertJProperty (property: JProperty) =
    let name, value = property.Name, property.Value
    match value.Type with
    | JTokenType.Boolean ->
        let value = Convert.ToBoolean(value)
        Some <| ProvidedProperty(name, typeof<bool>, GetterCode = (fun args -> <@@ value @@>), IsStatic = true)
    | JTokenType.Integer ->
        let value = int value
        Some <| ProvidedProperty(name, typeof<int>, GetterCode = (fun args -> <@@ value @@>), IsStatic = true)
    | JTokenType.String ->
        let value = string value
        Some <| ProvidedProperty(name, typeof<string>, GetterCode = (fun args -> <@@ value @@>), IsStatic = true)
    | _ -> None
