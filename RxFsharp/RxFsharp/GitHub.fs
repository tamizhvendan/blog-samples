module GitHub

open Http
open FSharp.Data

type GitHubUser = JsonProvider<"user.json">
type GitHubUserRepos = JsonProvider<"repos.json">

type Profile = {
    Name : string
    AvatarUrl : string
    PopularRepositories : Repository seq
} and Repository = {
    Name : string
    Stars : int
    Languages : string[]
}

let host = "https://api.github.com"
let userUrl = sprintf "%s/users/%s" host
let reposUrl = sprintf "%s/users/%s/repos" host
let languagesUrl repoName userName = sprintf "%s/repos/%s/%s/languages" host userName repoName

let parseUser = GitHubUser.Parse
let parseUserRepos = GitHubUserRepos.Parse



let popularRepos (repos : GitHubUserRepos.Root []) =
    
    let ownRepos = repos |> Array.filter (fun repo -> not repo.Fork)
    let takeCount = if ownRepos.Length > 3 then 3 else repos.Length

    ownRepos
    |> Array.sortBy (fun r -> -r.StargazersCount)
    |> Array.toSeq
    |> Seq.take takeCount
    |> Seq.toArray

let parseLanguages languagesJson =
    languagesJson
    |> JsonValue.Parse
    |> JsonExtensions.Properties
    |> Array.map fst
    
let languageResponseToRepoWithLanguages (repo : GitHubUserRepos.Root) = function
    |Ok(l) -> {Name = repo.Name; Languages = (parseLanguages l); Stars = repo.StargazersCount}
    |_ -> {Name = repo.Name; Languages = Array.empty; Stars = repo.StargazersCount}

let reposResponseToPopularRepos = function
    |Ok(r) -> r |> parseUserRepos |> popularRepos
    |_ -> [||]

let toProfile  = function
    |Ok(u), repos -> 
        let user = parseUser u
        {Name = user.Name; PopularRepositories = repos; AvatarUrl = user.AvatarUrl} |> Some
    | _ -> None



