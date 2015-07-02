namespace SuaveRestApi

module App =   
    open SuaveRestApi.Rest
    open Suave.Http.RequestErrors
    open Suave.Http
    open Suave.Http.Applicatives
    open Suave.Types
    open Suave.Web
    open SuaveJwt    
    open JwtToken
    open SuaveRestApi.Db
    open SuaveRestApi.MusicStoreDb
    open Suave.Http.Successful
    open System

    type AudienceDto = {
        AudienceId : string
        Name : string  
    }

    type TokenRequestDto = {
        UserName : string
        Password : string
        AudienceId : string
    }

     type AudienceRequestDto = {
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
        let toAudienceDto (audience : Audience) : AudienceDto =
            {AudienceId = audience.AudienceId; Name = audience.Name}               

        let jwtCreateAudience audienceRequestDto = SuaveJwt.createAudiance AudienceStorage.save audienceRequestDto.Name

        let identityStore = {
            getClaims = Security.getClaims
            isValidCredentials = Security.isValidCredentials
            getSecretKey = Security.getSecretKey
            getSigningCredentials = Security.getSigningCredentials
            getAudience = AudienceStorage.getAudience
        }
                
        let jwtIssueToken tokenRequest = 

            let tokenRequest =  {                
                Issuer = issuer                
                UserName = tokenRequest.UserName
                Password = tokenRequest.Password                    
                TokenTimeSpan = TimeSpan.FromMinutes(5.)
                AudienceId = tokenRequest.AudienceId
            }

            SuaveJwt.issueToken identityStore tokenRequest

        let jwtAuthenticate = SuaveJwt.authenticate issuer identityStore

        let jwtAuthorize = SuaveJwt.authorize Security.authorizeAdmin 

        let audienceWebPart =
            
            choose [
                path "/audience/add" >>= POST >>= request (RestFul.getResourceFromReq<AudienceRequestDto> >> jwtCreateAudience  >> toAudienceDto >> JSON)
                path "/audience/token" 
                    >>= POST 
                    >>= request (RestFul.getResourceFromReq<TokenRequestDto> >> jwtIssueToken) 
            ]            
        
              

        startWebServer defaultConfig (choose [personWebPart;audienceWebPart;jwtAuthenticate (jwtAuthorize albumWebPart)])
        0

        

