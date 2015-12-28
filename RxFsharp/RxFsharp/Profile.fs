module Profile
open Http
open GitHub
open FSharp.Control.Reactive
open System.Reactive.Threading.Tasks
open ObservableExtensions

let getProfile username =

    let userStream = username |> userUrl |> asyncResponseToObservable

    let toRepoWithLanguagesStream (repo : GitHubUserRepos.Root) =    
        username
        |> languagesUrl repo.Name
        |> asyncResponseToObservable
        |> Observable.map (languageResponseToRepoWithLanguages repo)

    let popularReposStream = 
        username
        |> reposUrl 
        |> asyncResponseToObservable 
        |> Observable.map reposResponseToPopularRepos
        |> flatmap2 toRepoWithLanguagesStream
    
    async {
        return! popularReposStream
                |> Observable.zip userStream
                |> Observable.map toProfile
                |> TaskObservableExtensions.ToTask 
                |> Async.AwaitTask
    }