module AudienceStorage

open System.Collections.Generic
open JwtToken

let private audienceStorage = new Dictionary<string, Audience>()
        
let save (audience : Audience) =
    audienceStorage.Add(audience.ClientId, audience)
    audience

let getAudience clientId =        
    if audienceStorage.ContainsKey(clientId) then 
        Some audienceStorage.[clientId] 
    else
        None