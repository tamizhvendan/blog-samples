module SuaveJson 

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave.Http.Successful
open Suave.Http
open Suave.Types

let JSON v =     
    let jsonSerializerSettings = new JsonSerializerSettings()
    jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
    
    JsonConvert.SerializeObject(v, jsonSerializerSettings)
    |> OK 
    >>= Writers.setMimeType "application/json; charset=utf-8"

let mapJsonPayload<'a> (req : HttpRequest) = 
    
    let fromJson json =
        try 
            let obj = JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a    
            Some obj
        with
        | _ -> None

    let getString rawForm = 
        System.Text.Encoding.UTF8.GetString(rawForm)

    req.rawForm |> getString |> fromJson