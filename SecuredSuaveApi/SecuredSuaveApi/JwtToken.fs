module JwtToken

open Encodings
open System
open System.Security.Cryptography

type Audience = {
    AudienceId : string
    Secret : Base64String
    Name : string
}

let createAudience audienceName =          
    let audienceId = Guid.NewGuid().ToString("N")
    let data = Array.zeroCreate 32
    RNGCryptoServiceProvider.Create().GetBytes(data)
    let secret = data |> Base64String.create 
    {AudienceId = audienceId; Secret = secret; Name =  audienceName}  