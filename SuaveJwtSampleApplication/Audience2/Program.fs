open Suave.Http.Successful
open Suave.Http.Applicatives
open Suave.Http
open Suave.Web
open Suave.Types
open Secure
open Encodings
open System.Security.Claims

[<EntryPoint>]
let main argv = 
    let jwtConfig = {
        Issuer = "http://localhost:8083/suave"
        ClientId = "ada9263885c440869fb484fe354de13d"
        SecurityKey = KeyStore.securityKey (Base64String.fromString "0RWyzyttDmJtiaYkG9rph5cqxCTI8YAOsR7stq-P_5o")       
    }

    let authorizeSuperUser (claims : Claim seq) =
        match claims |> Seq.tryFind (fun c -> c.Type = ClaimTypes.Role && c.Value = "SuperUser") with
        | Some _ -> Authorized |> async.Return
        | None -> UnAuthorized "User is not a Super User" |> async.Return

    let authorize = jwtAuthorize jwtConfig
    let sample1 = path "/audience2/sample1" >>= OK "Sample 1"
    let sample2 = path "/audience2/sample2" >>= authorize authorizeSuperUser (OK "Sample 2")      
    let config = { defaultConfig with bindings = [HttpBinding.mk' HTTP "127.0.0.1" 8085] }    
    let app = choose [sample1;sample2]

    startWebServer config app
    0 // return an integer exit code
