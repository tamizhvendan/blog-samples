module AuthorizationServer

open Suave.Http
open Suave.Http.Applicatives
open Suave.Types
open SuaveJson
open Suave.Http.RequestErrors
open JwtToken
open System

type AudienceCreateRequest = {
    Name : string
}

type AudienceCreateResponse = {
    ClientId : string
    Base64Secret : string
    Name : string
}

type TokenCreateRequest = {
    UserName : string
    Password : string
    ClientId : string
}

type Config = {
    AddAudienceUrlPath : string
    CreateTokenUrlPath : string
    SaveAudience : Audience -> Async<Audience>
    GetAudience : string -> Async<Audience option>
    Issuer : string
    TokenTimeSpan : TimeSpan
}

let audienceWebPart config identityStore =

    let toAudienceCreateResponse (audience : Audience) = {
        Base64Secret = audience.Secret.ToString()
        ClientId = audience.ClientId        
        Name = audience.Name
    }

    let tryCreateAudience (ctx: HttpContext) =
        match mapJsonPayload<AudienceCreateRequest> ctx.request with
        | Some audienceCreateRequest -> 
            async {
                let! audience = audienceCreateRequest.Name |> createAudience |> config.SaveAudience                     
                let audienceCreateResponse = toAudienceCreateResponse audience
                return! JSON audienceCreateResponse ctx
            }
        | None -> BAD_REQUEST "Invalid Audience Create Request" ctx

    let tryCreateToken (ctx: HttpContext) =
        match mapJsonPayload<TokenCreateRequest> ctx.request with
        | Some tokenCreateRequest -> 
            async {
                let! audience = config.GetAudience tokenCreateRequest.ClientId
                match audience with
                | Some audience ->
                    let tokenCreateRequest' = {         
                        Issuer = config.Issuer        
                        UserName = tokenCreateRequest.UserName
                        Password = tokenCreateRequest.Password        
                        TokenTimeSpan = config.TokenTimeSpan
                    }
                    
                    let! token = createToken tokenCreateRequest' identityStore audience
                    match token with
                    | Some token -> return! JSON token ctx
                    | None -> return! BAD_REQUEST "Invalid Login Credentials" ctx
                    
                | None -> return! BAD_REQUEST "Invalid Client Id" ctx
            }
        
        | None -> BAD_REQUEST "Invalid Token Create Request" ctx

    choose [
        path config.AddAudienceUrlPath >>= POST >>= tryCreateAudience
        path config.CreateTokenUrlPath >>= POST >>= tryCreateToken
    ]