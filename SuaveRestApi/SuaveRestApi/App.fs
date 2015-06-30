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

        let issuer = "http://suaverestapi"
        let localAudience : Security.Audience = 
            {AudienceId = "5e29eed0db0248ae9f4214cc774fb81f"; Base64Secret = "aIUgjlHv_TE08YsESkAQbkOcKNiCWE9A6hpVl6BhV6o"; Name = "Tamizh"}        

        
        let toAudienceDto (audience : Security.Audience) =
            {AudienceId = audience.AudienceId; Name = audience.Name}  

        let jwtAuthenticate (ctx : HttpContext)  =   
            match ctx.request.header "audienceid" with
            |  Choice1Of2 audienceId ->    
                match Security.getAudience audienceId with
                | Some audience  ->    
                    match ctx.request.header "token" with
                    | Choice1Of2 accessToken ->                
                        match Security.isValidToken issuer audienceId audience.Base64Secret accessToken with
                        | Some claims -> Security.TokenValidationResult.Valid claims
                        | None -> Security.TokenValidationResult.Invalid "Invalid Signature"
                    | _ -> Security.TokenValidationResult.Invalid "Access Token Not Found"
                | _ -> Security.TokenValidationResult.Invalid "Invalid Audiance Id"
            | _ -> Security.TokenValidationResult.Invalid "Audience Id Not Found"      

           
        let admin webpart tokenValidationResult =
            match tokenValidationResult with
            | Security.TokenValidationResult.Valid claims -> 
                match claims |> Seq.tryFind (fun c -> c.Type = ClaimTypes.Role && c.Value = "Admin") with
                | Some _ -> webpart
                | None -> FORBIDDEN "Invalid Request. User is not an admin" 
            | Security.TokenValidationResult.Invalid err -> FORBIDDEN err        
            
           

        let secure f (ctx:HttpContext) = f ctx ctx

        let authorize f1 f2 (ctx : HttpContext) = 
            match f1 ctx with
            | Security.TokenValidationResult.Valid claims -> f2 claims 
            | Security.TokenValidationResult.Invalid err -> FORBIDDEN err            
        
        
        let security =             
            Security.initialize localAudience
            choose [
                path "/audience/add" >>= POST >>= request (RestFul.getResourceFromReq<Security.AudienceRequest> >> Security.createAudience >> toAudienceDto >> JSON)
                path "/audience/token" >>= POST >>= request (RestFul.getResourceFromReq<Security.TokenRequest> >> Security.issueToken issuer  >> JSON)              
            ]
             

        let securedMusicStore = context (jwtAuthenticate >> admin albumWebPart)
        

        startWebServer defaultConfig (choose [personWebPart;security;securedMusicStore])
        0 // return an integer exit code

