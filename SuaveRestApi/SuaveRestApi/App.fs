namespace SuaveRestApi

module App =   
    open SuaveRestApi.Rest
    open Suave.Http.RequestErrors
    open Suave.Http
    open Suave.Http.Applicatives
    open Suave.Types
    open Suave.Web
    open SuaveJwt
    open Security
    open JwtToken
    open SuaveRestApi.Db
    open SuaveRestApi.MusicStoreDb

    type AudienceDto = {
        AudienceId : string
        Name : string  
    }

    [<EntryPoint>]
    let main argv =    

        let personWebPart = rest "people" {
            GetAll = Db.getPeople
            GetById = Db.getPerson
            Create = Db.createPerson
            Update = Db.updatePerson
            UpdateById = Db.updatePersonById
            Delete = Db.deletePerson
            IsExists = Db.isPersonExists
        }

        let albumWebPart = rest "albums" {
            GetAll = MusicStoreDb.getAlbums
            GetById = MusicStoreDb.getAlbumById
            Create = MusicStoreDb.createAlbum
            Update = MusicStoreDb.updateAlbum
            UpdateById = MusicStoreDb.updateAlbumById
            Delete = MusicStoreDb.deleteAlbum
            IsExists = MusicStoreDb.isAlbumExists
        }       

        let issuer = "http://localhost:8083/suaverestapi"
        let localAudience  = 
            {AudienceId = "5e29eed0db0248ae9f4214cc774fb81f"; Base64Secret = "aIUgjlHv_TE08YsESkAQbkOcKNiCWE9A6hpVl6BhV6o"; Name = "BootsrapAudience"}        
        let toAudienceDto (audience : Audience) =
            {AudienceId = audience.AudienceId; Name = audience.Name} 
                    
        let issueToken issuer (tokenRequest: TokenRequest) = 
            match AudienceStorage.getAudience tokenRequest.AudienceId with
            | Some audience -> 
                match JwtToken.issueToken issuer tokenRequest audience with
                | Some token -> token |> JSON
                | None -> BAD_REQUEST "User Credentials not found"
            | None -> BAD_REQUEST "Invalid Audience Id"
        
        let audienceWebPart =
            AudienceStorage.save localAudience |> ignore
            choose [
                path "/audience/add" >>= POST >>= request (RestFul.getResourceFromReq<AudienceRequest> >> createAudience AudienceStorage.save  >> toAudienceDto >> JSON)
                path "/audience/token" >>= POST >>= request (RestFul.getResourceFromReq<TokenRequest> >> issueToken issuer)              
            ]
             
        let securedMusicStore = context (jwtAuthenticate issuer >> authorize authorizeAdmin albumWebPart)

        startWebServer defaultConfig (choose [personWebPart;audienceWebPart;securedMusicStore])
        0

        

