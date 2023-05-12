open Suave
open Suave.Filters

[<EntryPoint>]
let main argv = 
    let config = {
        defaultConfig with
             bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" 8083 ]
    }
    let webpart = pathScan "/api/profile/%s" ApiGateway.getProfile

    startWebServer config webpart
    
    0 // return an integer exit code
