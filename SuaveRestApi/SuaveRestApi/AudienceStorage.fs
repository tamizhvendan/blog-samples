namespace SuaveRestApi

module AudienceStorage =      
    open System.Collections.Generic
    open JwtToken

    let private audienceStorage = new Dictionary<string, Audience>()
        
    let save audience =
        audienceStorage.Add(audience.AudienceId, audience)
        audience

    let getAudience audienceId =        
        if audienceStorage.ContainsKey(audienceId) then 
            Some audienceStorage.[audienceId] 
        else
            None