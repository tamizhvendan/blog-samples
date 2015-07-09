module IdentityStore
    
open Encodings
open System.IdentityModel.Tokens
open System.Security.Claims      
    
       
let getSecretKey sharedKey : SecurityKey =
    let symmetricKey = sharedKey |> Base64String.decode
    new InMemorySymmetricSecurityKey(symmetricKey) :> SecurityKey

let getSigningCredentials secretKey =
    new SigningCredentials(secretKey,SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest)

let getClaims userName =
    seq {
        yield (ClaimTypes.Name, userName)
        if (userName = "Admin") then
            yield (ClaimTypes.Role, "Admin")            
    } |> Seq.map (fun x -> new Claim(fst x, snd x)) |> async.Return 

let isValidCredentials username password =
    username = password |> async.Return

    