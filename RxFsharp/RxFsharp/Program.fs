open Suave
open Suave.Http
open Suave.Http.Applicatives
open Suave.Web

[<EntryPoint>]
let main argv = 

    let webpart = pathScan "/api/profile/%s" ApiGateway.getProfile

    startWebServer defaultConfig webpart
    
    0 // return an integer exit code
