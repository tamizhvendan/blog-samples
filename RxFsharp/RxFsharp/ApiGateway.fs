module ApiGateway

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave.Http
open Suave.Http.Successful
open Suave.Types
open Profile
open Suave.Http.RequestErrors

let JSON v =  
    let jsonSerializerSettings = new JsonSerializerSettings()
    jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
    
    JsonConvert.SerializeObject(v, jsonSerializerSettings)
    |> OK 
    >>= Writers.setMimeType "application/json; charset=utf-8"

let getProfile userName (httpContext : HttpContext) =
     async {
        let! profile = getProfile userName
        match profile with
        | Some p -> return! JSON p httpContext
        | None -> return! NOT_FOUND (sprintf "Username %s not found" userName) httpContext
     }