namespace SuaveRestApi

module SuaveJwt =

    open Suave.Types    
    open JwtToken    
    open Suave.Http.RequestErrors
    open System.Security.Claims  
    open SuaveRestApi.Rest
        

    type AuthorizationResult =
    | Authorized
    | UnAuthorized of string
      
    let authorize authorizeUser webpart (ctx : HttpContext) = 
        let bad_request err = BAD_REQUEST err ctx
        match ctx.userState.ContainsKey("Claims") with
        | true -> 
            match ctx.userState.Item "Claims" with
            | :? (Claim seq) as claims ->
                async {
                    let! authorizationResult = authorizeUser claims          
                    match authorizationResult with
                    | Authorized -> return! webpart ctx
                    | UnAuthorized err -> return! FORBIDDEN err ctx
                }
            | _ -> bad_request "Invalid Claims Type"            
        | false -> bad_request "Claims not found"
                
    let createAudiance saveAudience audienceName =
        createAudience audienceName |> saveAudience


    let authenticate issuer identityStore webpart ctx  =      
        match ctx.request.header "audienceid" with
        |  Choice1Of2 audienceId ->    
            match identityStore.getAudience audienceId with
            | Some audience  ->    
                match ctx.request.header "token" with
                | Choice1Of2 accessToken -> 
                    let tokenValidationParams = {
                        AudienceId = audienceId
                        SharedKey = audience.Base64Secret
                        AccessToken = accessToken
                        Issuer = issuer
                    }   
                    async {
                        let! validationResult = validate tokenValidationParams identityStore.getSecretKey                                  
                        match validationResult with
                        | Choice1Of2 claims -> return! webpart { ctx with userState = ctx.userState.Add("Claims", claims) } 
                        | Choice2Of2 err -> return! BAD_REQUEST err ctx 
                    }
                | _ -> BAD_REQUEST "Access Token Not Found" ctx 
            | _ -> BAD_REQUEST "Invalid Audiance Id" ctx 
        | _ -> BAD_REQUEST "Audience Id Not Found" ctx  


    let issueToken identityStore (tokenCreateRequest : TokenCreateRequest)  ctx = 
        match identityStore.getAudience tokenCreateRequest.AudienceId with
        | Some audience -> async {            
            let! token = JwtToken.tryCreateToken audience tokenCreateRequest identityStore
            match token with
            | Some token -> return! JSON token ctx 
            | None -> return! BAD_REQUEST "User Credentials not found" ctx
         }
        | None -> BAD_REQUEST "Invalid Audience Id" ctx
        