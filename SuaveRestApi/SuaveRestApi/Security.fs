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

    type Audience = {
        AudienceId : string
        Base64Secret : string
        Name : string
    }

    type AudienceRequest = {
        Name : string
    }

    type TokenRequest = {
        UserName : string
        Password : string
        AudienceId : string
    } 
    

    type Token = {
        AccessToken : string
        TokenType : string
        ExpiresIn : float
    }
    with static member Empty = {AccessToken = ""; TokenType = ""; ExpiresIn = 0.} 

    let private audienceStorage = new Dictionary<string, Audience>()

    let initialize (client: Audience) =
        audienceStorage.Add(client.AudienceId, client)

    let createAudience audienceRequest = 
        
        let audienceId = Guid.NewGuid().ToString("N")
        let data = Array.zeroCreate 32
        RNGCryptoServiceProvider.Create().GetBytes(data)
        let base64Secret = data |> TextEncodings.Base64Url.Encode
        let newAudience = {AudienceId = audienceId; Base64Secret = base64Secret; Name =  audienceRequest.Name}
        audienceStorage.Add(audienceId, newAudience)
        newAudience

    let getAudience audienceId =
        
        if audienceStorage.ContainsKey(audienceId) then 
            Some audienceStorage.[audienceId] 
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

    let getSecretKey sharedKey =
        let symmetricKey = sharedKey |> TextEncodings.Base64Url.Decode
        new InMemorySymmetricSecurityKey(symmetricKey)

    let issueToken issuer tokenRequest =
        
        match getAudience tokenRequest.AudienceId with
        | Some audience ->
            if (tokenRequest.UserName = tokenRequest.Password) then                
                let secretKey = getSecretKey audience.Base64Secret
                let signingCredentials = 
                    new SigningCredentials(secretKey,SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest) 
                let issuedOn = new Nullable<DateTime>(DateTime.UtcNow)
                let expiresBy = new Nullable<DateTime>(DateTime.UtcNow.AddMinutes(5.))       
                let claims = getClaims tokenRequest.UserName 
                let jwtSecurityToken = new JwtSecurityToken(issuer, audience.AudienceId, claims, issuedOn, expiresBy, signingCredentials)
                let handler = new JwtSecurityTokenHandler()  
                let accessToken = handler.WriteToken(jwtSecurityToken)                
                {AccessToken = accessToken; TokenType = "bearer"; ExpiresIn = TimeSpan.FromMinutes(5.).TotalSeconds}
            else Token.Empty        
        | None -> Token.Empty

    let readToken (accessToken : string) = new JwtSecurityToken(accessToken)
        
            

    type TokenValidationResult =
        | Invalid of String
        | Valid of Claim seq

    let isTokenAlive (token : JwtSecurityToken) currentTime =
        token.ValidFrom <= currentTime && token.ValidTo >= currentTime

    let isValidToken issuer audienceId sharedKey accessToken =     

        let validationParams = new TokenValidationParameters()
        validationParams.ValidAudience <- audienceId
        validationParams.ValidIssuer <- issuer
        validationParams.ValidateLifetime <- true
        validationParams.ValidateIssuerSigningKey <- true
        validationParams.IssuerSigningKey <-  getSecretKey sharedKey
        let handler = new JwtSecurityTokenHandler() 
        let token : SecurityToken ref = ref null
        let claimsPrincipal = handler.ValidateToken(accessToken, validationParams, token)
        
        if token <> ref null then
            Some claimsPrincipal.Claims
        else
            None

    
        
