open Suave.Web
open AuthServer
open JwtToken
open Secure
open System
open Suave.Http
open Encodings

[<EntryPoint>]
let main argv = 
    
    let authorizationServerConfig = {
        AddAudienceUrlPath = "/api/audience"
        CreateTokenUrlPath = "/oauth2/token"
        SaveAudience = AudienceStorage.saveAudience
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

    startWebServer defaultConfig audienceWebPart'

    0 // return an integer exit code
