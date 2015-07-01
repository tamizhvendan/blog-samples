namespace SuaveRestApi

open System
open System.Security.Cryptography
open System.Security.Claims
open Encodings
open Security
open System.IdentityModel.Tokens

module JwtToken =            

    type TokenRequest = {
        UserName : string
        Password : string
        AudienceId : string
    }
    
    type AudienceRequest = {
        Name : string
    } 
    

    type Token = {
        AccessToken : string
        TokenType : string
        ExpiresIn : float
    }

    type Audience = {
        AudienceId : string
        Base64Secret : string
        Name : string
    }

    type TokenValidationResult =
        | Invalid of String
        | Valid of Claim seq


    let createAudience saveAudience (audienceRequest : AudienceRequest)  =          
        let audienceId = Guid.NewGuid().ToString("N")
        let data = Array.zeroCreate 32
        RNGCryptoServiceProvider.Create().GetBytes(data)
        let base64Secret = data |> base64UrlEncode
        let newAudience = {AudienceId = audienceId; Base64Secret = base64Secret; Name =  audienceRequest.Name}
        saveAudience newAudience        
    
        
    let issueToken issuer tokenRequest audience =           
        if (tokenRequest.UserName = tokenRequest.Password) then                            
            let signingCredentials = (getSecretKey >> getSigningCredentials) audience.Base64Secret                
            let issuedOn = new Nullable<DateTime>(DateTime.UtcNow)
            let expiresBy = new Nullable<DateTime>(DateTime.UtcNow.AddMinutes(5.))       
            let claims = getClaims tokenRequest.UserName 
            let jwtSecurityToken = new JwtSecurityToken(issuer, audience.AudienceId, claims, issuedOn, expiresBy, signingCredentials)
            let handler = new JwtSecurityTokenHandler()  
            let accessToken = handler.WriteToken(jwtSecurityToken)                
            Some {AccessToken = accessToken; TokenType = "bearer"; ExpiresIn = TimeSpan.FromMinutes(5.).TotalSeconds}
        else None

   

    let validate issuer audienceId sharedKey accessToken =

        let tokenValidationParameters =
            let validationParams = new TokenValidationParameters()
            validationParams.ValidAudience <- audienceId
            validationParams.ValidIssuer <- issuer
            validationParams.ValidateLifetime <- true
            validationParams.ValidateIssuerSigningKey <- true
            validationParams.IssuerSigningKey <-  getSecretKey sharedKey
            validationParams

        let handler = new JwtSecurityTokenHandler() 
        let token : SecurityToken ref = ref null
        try 
            let principal = handler.ValidateToken(accessToken, tokenValidationParameters, token)
            Valid principal.Claims
        with
            | ex -> Invalid ex.Message