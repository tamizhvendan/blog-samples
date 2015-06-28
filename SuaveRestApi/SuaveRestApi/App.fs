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
    open Suave.Http.RequestErrors
    open Suave.Http.Successful

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

        let issuer = "Suave"
        let localClient : Security.Client = 
            {ClientId = "5e29eed0db0248ae9f4214cc774fb81f"; Base64Secret = "aIUgjlHv_TE08YsESkAQbkOcKNiCWE9A6hpVl6BhV6o"; Name = "Tamizh"}        

        

        let jwtAuthenticate (ctx : HttpContext)  =            
            match ctx.request.header "token" with
            | Choice1Of2 token ->                
                Security.validateToken issuer localClient.Base64Secret token
            | _ -> Security.TokenValidationResult.Invalid "Access Token Not Found"      

           
        let admin webpart tokenValidationResult =
            match tokenValidationResult with
            | Security.TokenValidationResult.Valid claims -> 
                match claims |> Seq.tryFind (fun c -> c.Type = "role" && c.Value = "Admin") with
                | Some _ -> webpart
                | None -> FORBIDDEN "Invalid Request. User is not an admin" 
            | Security.TokenValidationResult.Invalid err -> FORBIDDEN err        
            
           

        let secure f (ctx:HttpContext) = f ctx ctx

        let authorize f1 f2 (ctx : HttpContext) = 
            match f1 ctx with
            | Security.TokenValidationResult.Valid claims -> f2 claims 
            | Security.TokenValidationResult.Invalid err -> FORBIDDEN err        
        
        let security =             
            Security.initialize localClient
            choose [
                path "/client/add" >>= POST >>= request (RestFul.getResourceFromReq<Security.ClientRequest> >> Security.createClient >> JSON)
                path "/client/token" >>= POST >>= request (RestFul.getResourceFromReq<Security.TokenRequest> >> Security.createToken issuer >> JSON)              
            ]
             

        let securedMusicStore = context (jwtAuthenticate >> admin albumWebPart)
        

        startWebServer defaultConfig (choose [personWebPart;security;securedMusicStore])
        0 // return an integer exit code

