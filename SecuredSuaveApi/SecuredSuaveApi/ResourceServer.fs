module ResourceServer

open Suave.Types

open JwtToken
open System.Security.Claims
open Suave.Http.RequestErrors
open System.IdentityModel.Tokens
open Encodings

type AuthorizationResult =
    | Authorized
    | UnAuthorized of string


type AuthenticationConfig = {
    Issuer : string
    getSecurityKey : Base64String -> SecurityKey
    getAudience : string -> Async<Audience option>    
}

let jwtAuthenticate authConfig webpart (ctx: HttpContext) = 

    let updateContextWithClaims claims =
        { ctx with userState = ctx.userState.Remove("Claims").Add("Claims", claims) }    

    match ctx.request.header "clientid", ctx.request.header "token" with
    | Choice1Of2 clientId, Choice1Of2 accessToken -> 
        async {
            let! audience = authConfig.getAudience clientId
            match audience with
            | Some audience -> 
                let tokenValidationRequest =  {
                    Issuer = authConfig.Issuer
                    Secret = audience.Secret
                    ClientId = audience.ClientId
                    AccessToken = accessToken
                }
                let validationResult = validate tokenValidationRequest authConfig.getSecurityKey
                match validationResult with
                | Choice1Of2 claims -> return! webpart (updateContextWithClaims claims)
                | Choice2Of2 err -> return! FORBIDDEN err ctx                 
            | None -> return! FORBIDDEN "Invalid Client Id" ctx
        }
    | _, _ -> FORBIDDEN "Invalid Request. Provide both clientid and token" ctx   



let jwtAuthorize authorizeUser webpart (ctx: HttpContext) =

    let getClaims (ctx: HttpContext) =
        let userState = ctx.userState
        if userState.ContainsKey("Claims") then
            match userState.Item "Claims" with
            | :? (Claim seq) as claims -> Some claims             
            | _ -> None
        else
            None

    match getClaims ctx with
    | Some claims ->
         async {
            let! authorizationResult = authorizeUser claims          
            match authorizationResult with
            | Authorized -> return! webpart ctx
            | UnAuthorized err -> return! FORBIDDEN err ctx
        }
    | None -> FORBIDDEN "Claims not found" ctx
