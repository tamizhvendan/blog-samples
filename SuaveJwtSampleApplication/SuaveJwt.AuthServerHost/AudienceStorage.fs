module AudienceStorage

open System.Collections.Generic
open System
open System.Security.Cryptography
open Encodings
open JwtToken

let private audienceStorage = new Dictionary<string, Audience>()       


let getAudience clientId =        
    if audienceStorage.ContainsKey(clientId) then 
        Some audienceStorage.[clientId] |> async.Return
    else
        None |> async.Return


let createAudience audienceName =          
    let clientId = Guid.NewGuid().ToString("N")
    let data = Array.zeroCreate 32
    RNGCryptoServiceProvider.Create().GetBytes(data)
    let secret = data |> Base64String.create 
    let audience : Audience = {ClientId = clientId; Secret = secret; Name =  audienceName} 
    audienceStorage.Add(audience.ClientId, audience)
    audience |> async.Return