module AuthorizationServer

open Suave.Http
open Suave.Http.Applicatives
open Suave.Types
open SuaveJson
open Suave.Http.RequestErrors
open JwtToken

type AudienceCreateRequest = {
    Name : string
}

type AudienceCreateResponse = {
    AudienceId : string
    Base64Secret : string
    Name : string
}

type Config = {
    AddAudienceUrlPath : string
    SaveAudience : Audience -> Audience
}

let audienceWebPart config =

    let toAudienceCreateResponse audience = {
        Base64Secret = audience.Secret.ToString()
        AudienceId = audience.AudienceId        
        Name = audience.Name
    }

    let tryCreateAudience request =
        match mapJsonPayload<AudienceCreateRequest> request with
        | Some audienceCreateRequest -> 
            audienceCreateRequest.Name 
            |> createAudience 
            |> config.SaveAudience 
            |> toAudienceCreateResponse 
            |> JSON
        | None -> BAD_REQUEST "Invalid AudienceCreateRequest"

    choose [
        path config.AddAudienceUrlPath >>= POST >>= request tryCreateAudience
    ]