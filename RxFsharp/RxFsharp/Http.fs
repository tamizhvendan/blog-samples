module Http

open System.Net
open System.Net.Http
open FSharp.Control.Reactive

type HttpResponse =
    | Ok of string
    | Error of HttpStatusCode
    
let getResponseAsync (url: string) =
    async {
        let client = new HttpClient()
        client.DefaultRequestHeaders.Add("User-Agent", "FsharpRx");
        let! response = client.GetAsync(url) |> Async.AwaitTask
        match response.StatusCode with
        | HttpStatusCode.OK ->
           let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
           return (content |> Ok) 
        | _ ->
           return (response.StatusCode |> Error)
    }
    
let asyncResponseToObservable = getResponseAsync >> Observable.ofAsync
