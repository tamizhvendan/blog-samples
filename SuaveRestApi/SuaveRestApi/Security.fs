namespace SuaveRestApi

module Security =    
    
    open Encodings
    open System.IdentityModel.Tokens
    open System.Security.Claims   

    
    type AuthorizationResult =
    | Authorized
    | UnAuthorized of string
       
    let getSecretKey sharedKey =
        let symmetricKey = sharedKey |> base64UrlDecode
        new InMemorySymmetricSecurityKey(symmetricKey)

    let getSigningCredentials secretKey =
        new SigningCredentials(secretKey,SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest)

    let getClaims userName =
        seq {
            yield (ClaimTypes.Name, userName)
            if (userName = "Admin") then
                yield (ClaimTypes.Role, "Admin")            
        } |> Seq.map (fun x -> new Claim(fst x, snd x)) 


    let authorizeAdmin (claims : Claim seq) =
        match claims |> Seq.tryFind (fun c -> c.Type = ClaimTypes.Role && c.Value = "Admin") with
        | Some _ -> Authorized
        | None -> UnAuthorized "User is not an admin"

    