open Suave
open Suave.Http
open Suave.Web
open Secure
open Encodings
open System.Security.Claims
open Suave.Successful
open Suave.Filters
open Suave.Operators

[<EntryPoint>]
let main argv = 
    
    let jwtConfig = {
        Issuer = "http://localhost:8083/suave"
        ClientId = "7ff79ba3305c4e4f9d0ececeae70c78f"
        SecurityKey = KeyStore.securityKey (Base64String.fromString "Op5EqjC70aLS2dx3gI0zADPIZGX2As6UEwjA4oyBjMo")       
    }

    let authorizeAdmin (claims : Claim seq) =
        match claims |> Seq.tryFind (fun c -> c.Type = ClaimTypes.Role && c.Value = "Admin") with
        | Some _ -> Authorized |> async.Return
        | None -> UnAuthorized "User is not an admin" |> async.Return
    
    let sample1 = path "/audience1/sample1" >=> jwtAuthenticate jwtConfig (OK "Sample 1")  
    let sample2 = path "/audience1/sample2" >=> jwtAuthorize jwtConfig authorizeAdmin (OK "Sample 2")      
    let config = { defaultConfig with bindings = [HttpBinding.mkSimple HTTP "127.0.0.1" 8084] }    
    let app = choose [sample1;sample2]

    startWebServer config app
    0 // return an integer exit code
