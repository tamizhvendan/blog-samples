open Suave.Web
open AuthorizationServer

// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

[<EntryPoint>]
let main argv = 
    
    let authorizationServerConfig = {
        AddAudienceUrlPath = "/audience"
        SaveAudience = AudienceStorage.save
    }
    startWebServer defaultConfig (audienceWebPart authorizationServerConfig)

    0 // return an integer exit code
