open Suave.Http.Successful
open Suave.Http.Applicatives
open Suave.Http
open Suave.Web
open Suave.Types
open JwtToken
open Secure
open Encodings

[<EntryPoint>]
let main argv = 
    
    let authenticationConfig = {
        Issuer = "http://localhost:8083/suave"
        ClientId = "50a371beac4b4a708d74f00843a9d027"
        SecurityKey = KeyStore.securityKey (Base64String.fromString "BujAHh7DggfeNhpPj59NN-R4Vs0EA7vXzjp_cV-N8nk")       
    }
    let sample1 = path "/audience1/sample1" >>= jwtAuthenticate authenticationConfig (OK "Sample 1")        
    let config = { defaultConfig with bindings = [HttpBinding.mk' HTTP "127.0.0.1" 8084] }    
    let app = choose [sample1]

    startWebServer config app
    0 // return an integer exit code
