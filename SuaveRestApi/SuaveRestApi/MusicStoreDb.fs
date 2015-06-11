namespace SuaveRestApi.MusicStoreDb

open FSharp.Data.Sql

module MusicStoreDb = 
    type Album = 
        { 
          AlbumId : int
          ArtistId : int
          GenreId : int
          Title : string
          Price : decimal}
    
    type private Sql = SqlDataProvider< "Server=localhost;Database=SuaveMusicStore;Trusted_Connection=True;MultipleActiveResultSets=true;Integrated Security=SSPI;", DatabaseVendor=Common.DatabaseProviderTypes.MSSQLSERVER >
    
    type DbContext = Sql.dataContext
    
    type AlbumEntity = DbContext.``[dbo].[Albums]Entity``
    
    let private getContext() = Sql.GetDataContext()
    
    let firstOrNone s = s |> Seq.tryFind (fun _ -> true)

    let mapToAlbum (albumEntity : AlbumEntity) =
        {   
            AlbumId = albumEntity.AlbumId
            ArtistId = albumEntity.ArtistId
            GenreId = albumEntity.GenreId
            Title = albumEntity.Title
            Price = albumEntity.Price
        }

    let getAlbums () = 
        getContext().``[dbo].[Albums]``
        |> Seq.map mapToAlbum

    let getAlbumEntityById (ctx : DbContext) id = 
        query { 
        for album in ctx.``[dbo].[Albums]`` do
            where (album.AlbumId = id)
            select album
        } |> firstOrNone

    let getAlbumById id =
        getAlbumEntityById (getContext()) id |> Option.map mapToAlbum

    let createAlbum album =
        let ctx = getContext()
        let album = ctx.``[dbo].[Albums]``.Create(album.ArtistId, album.GenreId, album.Price, album.Title)
        ctx.SubmitUpdates()
        album |> mapToAlbum

    let updateAlbumById id album =
        let ctx = getContext()
        let albumEntity = getAlbumEntityById ctx album.AlbumId
        match albumEntity with
        | None -> None
        | Some a ->
            a.ArtistId <- album.AlbumId
            a.GenreId <- album.GenreId
            a.Price <- album.Price
            a.Title <- album.Title
            ctx.SubmitUpdates()
            Some album
        
    let updateAlbum album =
        updateAlbumById album.AlbumId album


    let deleteAlbum id =
        let ctx = getContext()
        let albumEntity = getAlbumEntityById ctx id
        match albumEntity with
        | None -> ()
        | Some a ->
            a.Delete()
            ctx.SubmitUpdates()

    let isAlbumExists id =
        let ctx = getContext()
        let albumEntity = getAlbumEntityById ctx id
        match albumEntity with
        | None -> false
        | Some _ -> true
    
