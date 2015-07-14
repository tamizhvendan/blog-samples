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
        ClientId = "aed752ffb32b4dc286374698115ee8c2"
        SecurityKey = KeyStore.securityKey (Base64String.fromString "90Gzp66xNJwd9leyAaxJfIhi3ziGHDxZl05ZoGexm-w")       
    }

    let authorizeSuperUser (claims : Claim seq) =
        match claims |> Seq.tryFind (fun c -> c.Type = ClaimTypes.Role && c.Value = "SuperUser") with
        | Some _ -> Authorized |> async.Return
        | None -> UnAuthorized "User is not a Super User" |> async.Return

    let authorize = jwtAuthorize authenticationConfig
    let sample1 = path "/audience2/sample1" >>= OK "Sample 1"
    let sample2 = path "/audience2/sample2" >>= authorize authorizeSuperUser (OK "Sample 2")      
    let config = { defaultConfig with bindings = [HttpBinding.mk' HTTP "127.0.0.1" 8085] }    
    let app = choose [sample1;sample2]

    startWebServer config app
    0 // return an integer exit code
