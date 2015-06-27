namespace SuaveRestApi

module App =

    open SuaveRestApi.Db
    open SuaveRestApi.MusicStoreDb
    open SuaveRestApi.Rest
    open Suave.Web
    open Suave.Http
    open System.IdentityModel.Tokens
    open System.IdentityModel
    open Suave.Http.Applicatives
    open Suave.Types
    open System.Security.Claims

    (*
        http://bitoftech.net/2014/10/27/json-web-token-asp-net-web-api-2-jwt-owin-authorization-server/
        https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/master/tests/System.IdentityModel.Tokens.Jwt.Tests/CreateAndValidateTokens.cs
        http://code.tutsplus.com/tutorials/token-based-authentication-with-angularjs-nodejs--cms-22543
    *)

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

        let handler = new JwtSecurityTokenHandler();
        let payload = new JwtPayload()
        let header = new JwtHeader()
        

       

        
        let security =  
            choose [
                path "/client/add" >>= POST >>= request (RestFul.getResourceFromReq<Security.ClientRequest> >> Security.createClient >> JSON)
                path "/client/token" >>= POST >>= request (RestFul.getResourceFromReq<Security.TokenRequest> >> Security.createToken "Suave" >> JSON)
            ]
             
        

        startWebServer defaultConfig (choose [personWebPart;albumWebPart;security])
        0 // return an integer exit code

