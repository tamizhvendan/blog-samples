module AudienceStorage

open System.Collections.Generic
open JwtToken

let private audienceStorage = new Dictionary<string, Audience>()
        
let save (audience : Audience) =
    audienceStorage.Add(audience.AudienceId, audience)
    audience