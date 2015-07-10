open Suave.Web
open AuthServer
open JwtToken
open Secure
open System
open Suave.Http
open Suave.Http.Successful
open Suave.Http.Applicatives
open Encodings

[<EntryPoint>]
let main argv = 
    
    let authorizationServerConfig = {
        AddAudienceUrlPath = "/api/audience"
        CreateTokenUrlPath = "/oauth2/token"
        CreateAudience = AudienceStorage.createAudience
        GetAudience = AudienceStorage.getAudience
        Issuer = "http://localhost:8083/suave"
        TokenTimeSpan = TimeSpan.FromMinutes(1.)
    }

    let identityStore = {
        getClaims = IdentityStore.getClaims
        isValidCredentials = IdentityStore.isValidCredentials
        getSecurityKey = KeyStore.securityKey
        getSigningCredentials = KeyStore.hmacSha256
    }    

    let audienceWebPart' = audienceWebPart authorizationServerConfig identityStore
    //let authenticate = jwtAuthenticate authenticationConfig  
    let authorizeAdmin = jwtAuthorize IdentityStore.authorizeAdmin  

    let sample1 = path "/sample1" >>= OK "Sample 1" 
    

    let app = choose [audienceWebPart';sample1;]

    startWebServer defaultConfig app

    0 // return an integer exit code
