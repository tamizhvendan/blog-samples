open Suave.Web
open AuthorizationServer
open System
open JwtToken
open ResourceServer
open Suave.Http
open Suave.Http.Successful
open Suave.Http.Applicatives

[<EntryPoint>]
let main argv = 
    
    let authorizationServerConfig = {
        AddAudienceUrlPath = "/api/audience"
        CreateTokenUrlPath = "/oauth2/token"
        SaveAudience = AudienceStorage.save
        GetAudience = AudienceStorage.getAudience
        Issuer = "http://localhost:8083/suave"
        TokenTimeSpan = TimeSpan.FromMinutes(1.)
    }

    let identityStore = {
        getClaims = IdentityStore.getClaims
        isValidCredentials = IdentityStore.isValidCredentials
        getSecurityKey = IdentityStore.getSecurityKey
        getSigningCredentials = IdentityStore.getSigningCredentials
    }

    let authenticationConfig = {
        Issuer = authorizationServerConfig.Issuer
        getSecurityKey = IdentityStore.getSecurityKey
        getAudience = AudienceStorage.getAudience
    }

    let audienceWebPart' = audienceWebPart authorizationServerConfig identityStore
    let authenticate = jwtAuthenticate authenticationConfig  
    let authorizeAdmin = jwtAuthorize IdentityStore.authorizeAdmin  

    let sample1 = path "/sample1" >>= OK "Sample 1"
    let sample2 = path "/sample2" >>=  authenticate (OK "Sample 2")
    let sample3 = path "/sample3" >>= authenticate (authorizeAdmin (OK "Sample 3"))
    

    let app = choose [audienceWebPart';sample1;sample2;sample3]

    startWebServer defaultConfig app

    0 // return an integer exit code
