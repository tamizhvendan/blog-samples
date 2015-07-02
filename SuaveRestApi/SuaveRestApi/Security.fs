namespace SuaveRestApi

module Security =    
    
    open Encodings
    open System.IdentityModel.Tokens
    open System.Security.Claims      
    open SuaveJwt
       
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


    let authorizeAdmin (claims : Claim seq) =
        match claims |> Seq.tryFind (fun c -> c.Type = ClaimTypes.Role && c.Value = "Admin") with
        | Some _ -> Authorized |> async.Return
        | None -> UnAuthorized "User is not an admin" |> async.Return


    let isValidCredentials username password =
        username = password |> async.Return

    