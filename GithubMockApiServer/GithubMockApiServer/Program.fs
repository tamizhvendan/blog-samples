open Suave
open Suave.Operators
open Suave.Successful
open Suave.Filters
open System.IO

[<EntryPoint>]
let main argv = 
    let json fileName =
        let content = File.ReadAllText fileName  
        content.Replace("\r", "").Replace("\n","")
        |> OK >=> Writers.setMimeType "application/json"      
    
    let user = pathScan "/users/%s" (fun _ -> "User.json" |> json)  
    let repos = pathScan "/users/%s/repos" (fun _ -> "Repos.json" |> json)
    let mockApi = choose [repos;user]

    startWebServer defaultConfig mockApi          
    0 
