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
    
    let authenticationConfig = {
        Issuer = "http://localhost:8083/suave"
        ClientId = "ad4c3c9c9367442a9f30bfb69edb2113"
        SecurityKey = KeyStore.securityKey (Base64String.fromString "bJ3RXpHDoOSmToBeZCwe8V6Gyw-wA4QhyJlMETQR6u4")       
    }

    let authorizeAdmin (claims : Claim seq) =
        match claims |> Seq.tryFind (fun c -> c.Type = ClaimTypes.Role && c.Value = "Admin") with
        | Some _ -> Authorized |> async.Return
        | None -> UnAuthorized "User is not an admin" |> async.Return

    let authenticate = jwtAuthenticate authenticationConfig
    let sample1 = path "/audience1/sample1" >>= authenticate (OK "Sample 1")  
    let sample2 = path "/audience1/sample2" >>= authenticate (jwtAuthorize authorizeAdmin (OK "Sample 2"))      
    let config = { defaultConfig with bindings = [HttpBinding.mk' HTTP "127.0.0.1" 8084] }    
    let app = choose [sample1;sample2]

    startWebServer config app
    0 // return an integer exit code
