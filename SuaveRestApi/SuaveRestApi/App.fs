namespace SuaveRestApi

module App =   
    open SuaveRestApi.Rest    
    open Suave.Http    
    open Suave.Web   
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

       
        let app = choose[personWebPart;albumWebPart]                    

        startWebServer defaultConfig app
            
        0

        

