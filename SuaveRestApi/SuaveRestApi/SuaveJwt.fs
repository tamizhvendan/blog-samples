namespace SuaveRestApi

module SuaveJwt =

    open Suave.Types
    open AudienceStorage
    open JwtToken
    open Security
    open Suave.Http.RequestErrors

    let jwtAuthenticate issuer (ctx : HttpContext)  =   
        match ctx.request.header "audienceid" with
        |  Choice1Of2 audienceId ->    
            match getAudience audienceId with
            | Some audience  ->    
                match ctx.request.header "token" with
                | Choice1Of2 accessToken ->                                    
                    validate issuer audienceId audience.Base64Secret accessToken                                        
                | _ -> Invalid "Access Token Not Found"
            | _ -> Invalid "Invalid Audiance Id"
        | _ -> Invalid "Audience Id Not Found"   
        
      
    let authorize authorizeUser webpart tokenValidationResult =
        match tokenValidationResult with
        | Valid claims -> 
            match authorizeUser claims with
            | Authorized -> webpart
            | UnAuthorized err -> FORBIDDEN err 
        | Invalid err -> BAD_REQUEST err    


