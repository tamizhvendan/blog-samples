namespace SuaveRestApi

module App =

    open SuaveRestApi.Db
    open SuaveRestApi.MusicStoreDb
    open SuaveRestApi.Rest
    open Suave.Web
    open Suave.Http

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

        startWebServer defaultConfig (choose [personWebPart;albumWebPart])
        0 // return an integer exit code

