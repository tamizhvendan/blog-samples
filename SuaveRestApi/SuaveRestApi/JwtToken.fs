namespace SuaveRestApi

open System
open System.Security.Cryptography
open System.Security.Claims
open Encodings
open System.IdentityModel.Tokens

module JwtToken =        

    type TokenCreateRequest = {
        AudienceId : string       
        Issuer : string        
        UserName : string
        Password : string        
        TokenTimeSpan : TimeSpan
    } 

    type Audience = {
        AudienceId : string
        Base64Secret : Base64String
        Name : string
    }  

    type IdentityStore = {
         getClaims : string -> Async<Claim seq>
         isValidCredentials : string -> string -> Async<bool>
         getSecretKey : Base64String -> SecurityKey
         getSigningCredentials : SecurityKey -> SigningCredentials
         getAudience : string -> Audience option
    }

    type Token = {
        AccessToken : string        
        ExpiresIn : float        
    } 
    
    type TokenValidationParams = {
        AudienceId : string        
        SharedKey : Base64String
        AccessToken : string
        Issuer : string
    }   


    let createAudience audienceName =          
        let audienceId = Guid.NewGuid().ToString("N")
        let data = Array.zeroCreate 32
        RNGCryptoServiceProvider.Create().GetBytes(data)
        let base64Secret = data |> Base64String.create 
        {AudienceId = audienceId; Base64Secret = base64Secret; Name =  audienceName}      
    
        
    let tryCreateToken audience request identityStore =     
        async {
            let! isValidCredentials = 
                identityStore.isValidCredentials request.UserName request.Password
            if isValidCredentials then                            
                let signingCredentials = (identityStore.getSecretKey >> identityStore.getSigningCredentials) audience.Base64Secret
                let issuedOn = new Nullable<DateTime>(DateTime.UtcNow)
                let expiresBy = new Nullable<DateTime>(DateTime.UtcNow.Add(request.TokenTimeSpan))       
                let! claims =  identityStore.getClaims request.UserName 
                let jwtSecurityToken = new JwtSecurityToken(request.Issuer, audience.AudienceId, claims, issuedOn, expiresBy, signingCredentials)
                let handler = new JwtSecurityTokenHandler()  
                let accessToken = handler.WriteToken(jwtSecurityToken)                
                return Some {AccessToken = accessToken; ExpiresIn = request.TokenTimeSpan.TotalSeconds}
            else return None
        }

    let validate tokenValidationParams getSecretKey =

        let tokenValidationParameters =
            let validationParams = new TokenValidationParameters()
            validationParams.ValidAudience <- tokenValidationParams.AudienceId
            validationParams.ValidIssuer <- tokenValidationParams.Issuer
            validationParams.ValidateLifetime <- true
            validationParams.ValidateIssuerSigningKey <- true
            validationParams.IssuerSigningKey <-  getSecretKey tokenValidationParams.SharedKey
            validationParams

        let handler = new JwtSecurityTokenHandler() 
        let token : SecurityToken ref = ref null
        try 
            let principal = handler.ValidateToken(tokenValidationParams.AccessToken, tokenValidationParameters, token)
            principal.Claims |> Choice1Of2 |> async.Return
        with
            | ex -> ex.Message |> Choice2Of2 |> async.Return