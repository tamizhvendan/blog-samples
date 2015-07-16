module AudienceStorage

open System.Collections.Generic
open JwtToken

let private audienceStorage = new Dictionary<string, Audience>()       


let getAudience clientId =        
    if audienceStorage.ContainsKey(clientId) then 
        Some audienceStorage.[clientId] |> async.Return
    else
        None |> async.Return


let saveAudience (audience : Audience) =      
    audienceStorage.Add(audience.ClientId, audience)
    audience |> async.Return