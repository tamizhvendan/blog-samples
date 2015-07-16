module Secure

open Suave.Types
open System.Security.Claims
open Suave.Http.RequestErrors
open System.IdentityModel.Tokens
open JwtToken

type AuthorizationResult =
    | Authorized
    | UnAuthorized of string


type JwtConfig = {
    Issuer : string    
    SecurityKey : SecurityKey
    ClientId : string    
}

let jwtAuthenticate jwtConfig webpart (ctx: HttpContext) = 

    let updateContextWithClaims claims =
        { ctx with userState = ctx.userState.Remove("Claims").Add("Claims", claims) }    

    match ctx.request.header "token" with
    | Choice1Of2 accessToken ->                     
         
        let tokenValidationRequest =  {
            Issuer = jwtConfig.Issuer
            SecurityKey = jwtConfig.SecurityKey
            ClientId = jwtConfig.ClientId
            AccessToken = accessToken
        }
        let validationResult = validate tokenValidationRequest 
        match validationResult with
        | Choice1Of2 claims -> webpart (updateContextWithClaims claims)
        | Choice2Of2 err -> FORBIDDEN err ctx                         
        
    | _ -> BAD_REQUEST "Invalid Request. Request header doesn't contain token" ctx   



let jwtAuthorize jwtConfig authorizeUser webpart  = 

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
        
    
         
    jwtAuthenticate jwtConfig authorize
