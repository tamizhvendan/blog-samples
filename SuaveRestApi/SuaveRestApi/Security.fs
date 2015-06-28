namespace SuaveRestApi

open System
open System.Security.Cryptography
open Microsoft.Owin.Security.DataHandler.Encoder
open System.Collections.Generic

module Security =     
    open Thinktecture.IdentityModel.Tokens
    open System.Security.Claims
    open System.IdentityModel.Tokens
    open System.Text

    type Client = {
        ClientId : string
        Base64Secret : string
        Name : string
    }

    type ClientRequest = {
        Name : string
    }

    type TokenRequest = {
        UserName : string
        Password : string
        ClientId : string
    } 
    

    type Token = {
        AccessToken : string
        TokenType : string
        ExpiresIn : float
    }
    with static member Empty = {AccessToken = ""; TokenType = ""; ExpiresIn = 0.} 

    let private clientStorage = new Dictionary<string, Client>()

    let initialize (client: Client) =
        clientStorage.Add(client.ClientId, client)

    let createClient clientRequest = 
        
        let clientId = Guid.NewGuid().ToString("N")
        let data = Array.zeroCreate 32
        RNGCryptoServiceProvider.Create().GetBytes(data)
        let base64Secret = data |> TextEncodings.Base64Url.Encode
        let newAudience = {ClientId = clientId; Base64Secret = base64Secret; Name =  clientRequest.Name}
        clientStorage.Add(clientId, newAudience)
        newAudience

    let getClient clientId =
        
        if clientStorage.ContainsKey(clientId) then 
            Some clientStorage.[clientId] 
        else
            None
        

    let createIdentity claims =
            let identity = new ClaimsIdentity("JWT")
            claims |> Seq.map (fun x -> new Claim(fst x, snd x)) |> Seq.iter identity.AddClaim
            identity

    let getClaims userName =
        seq {
            yield (ClaimTypes.Name, userName)
            if (userName = "Admin") then
                yield (ClaimTypes.Role, "Admin")            
        } |> Seq.map (fun x -> new Claim(fst x, snd x))

    

    let getSigningCredentials secret =
        let hmacSigningCredentials (key: byte[]) = new HmacSigningCredentials(key)
        secret |> TextEncodings.Base64Url.Decode |> hmacSigningCredentials

    let createToken issuer tokenRequest =
        match getClient tokenRequest.ClientId with
        | Some client ->
            if (tokenRequest.UserName = tokenRequest.Password) then
                let signingKey = getSigningCredentials client.Base64Secret
                let issued = new Nullable<DateTime>(DateTime.UtcNow)
                let expires = new Nullable<DateTime>(DateTime.UtcNow.AddMinutes(5.))
                let claims = getClaims tokenRequest.UserName 
                let token = new JwtSecurityToken(issuer, tokenRequest.ClientId, claims, issued, expires, signingKey)
                let handler = new JwtSecurityTokenHandler()                
                let accessToken = handler.WriteToken(token)
                {AccessToken = accessToken; TokenType = "bearer"; ExpiresIn = TimeSpan.FromMinutes(5.).TotalSeconds}

            else Token.Empty 
            
        | None -> Token.Empty
        
    let readToken (accessToken : string) = new JwtSecurityToken(accessToken)
        
            

    type TokenValidationResult =
        | Invalid of String
        | Valid of Claim seq

    let isTokenAlive (token : JwtSecurityToken) currentTime =
        token.ValidFrom <= currentTime && token.ValidTo >= currentTime

    let isValidToken (token : JwtSecurityToken) sharedKey accessToken =
        use sha256 = new HMACSHA256()
        sha256.Key <- sharedKey |> TextEncodings.Base64Url.Decode
        let hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(token.RawData))
        let signature = hash |> Convert.ToBase64String 
        // TODO - Have to fix it
        true

    let validateToken issuer sharedKey accessToken =
        let token = readToken accessToken
        if (token.Issuer = issuer) then
            if isTokenAlive token DateTime.UtcNow then
                if isValidToken token sharedKey accessToken then
                    Valid token.Claims
                else
                    Invalid "Invalid Signature"                             
            else
                Invalid "Access Token Expired"
        else
            Invalid "Issuer is invalid"   
        
