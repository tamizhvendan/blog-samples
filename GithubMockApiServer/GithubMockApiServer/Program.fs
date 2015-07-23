open System.IO
open Suave.Http
open Suave.Http.Successful
open Suave
open Suave.Http.Applicatives
open Suave.Web

[<EntryPoint>]
let main argv = 
    let json fileName =
        let content = File.ReadAllText fileName  
        content.Replace("\r", "").Replace("\n","")
        |> OK >>= Writers.setMimeType "application/json"      
    
    let user = pathScan "/users/%s" (fun _ -> "User.json" |> json)  
    let repos = pathScan "/user/%s/repos" (fun _ -> "Repos.json" |> json)
    let mockApi = choose [user;repos]

    startWebServer defaultConfig mockApi          
    0 
