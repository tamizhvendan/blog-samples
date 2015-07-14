module Secure

open Suave.Types
open System.Security.Claims
open Suave.Http.RequestErrors
open System.IdentityModel.Tokens
open JwtToken

type AuthorizationResult =
    | Authorized
    | UnAuthorized of string


type AuthenticationConfig = {
    Issuer : string    
    SecurityKey : SecurityKey
    ClientId : string    
}

let jwtAuthenticate authConfig webpart (ctx: HttpContext) = 

    let updateContextWithClaims claims =
        { ctx with userState = ctx.userState.Remove("Claims").Add("Claims", claims) }    

    match ctx.request.header "token" with
    | Choice1Of2 accessToken ->                     
         
        let tokenValidationRequest =  {
            Issuer = authConfig.Issuer
            SecurityKey = authConfig.SecurityKey
            ClientId = authConfig.ClientId
            AccessToken = accessToken
        }
        let validationResult = validate tokenValidationRequest 
        match validationResult with
        | Choice1Of2 claims -> webpart (updateContextWithClaims claims)
        | Choice2Of2 err -> FORBIDDEN err ctx                         
        
    | _ -> FORBIDDEN "Invalid Request. Provide both clientid and token" ctx   



let jwtAuthorize authConfig authorizeUser webpart  = 

    let getClaims (ctx: HttpContext) =
        let userState = ctx.userState
        if userState.ContainsKey("Claims") then
            match userState.Item "Claims" with
            | :? (Claim seq) as claims -> Some claims             
            | _ -> None
        else
            None

    let authorize httpContext =
        match getClaims httpContext with
        | Some claims ->
                async {
                let! authorizationResult = authorizeUser claims          
                match authorizationResult with
                | Authorized -> return! webpart httpContext
                | UnAuthorized err -> return! FORBIDDEN err httpContext
            }
        | None -> FORBIDDEN "Claims not found" httpContext
        
    
         
    jwtAuthenticate authConfig authorize
