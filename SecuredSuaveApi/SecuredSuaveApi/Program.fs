open Suave.Web
open AuthorizationServer
open System
open JwtToken
// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

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
        getSecretKey = IdentityStore.getSecretKey
        getSigningCredentials = IdentityStore.getSigningCredentials
    }

    startWebServer defaultConfig (audienceWebPart authorizationServerConfig identityStore)

    0 // return an integer exit code
