module Http

open HttpClient
open FSharp.Control.Reactive

type HttpResponse =
| Ok of string
| Error of int

let getResponseAsync url =
    async {
        let! response = 
            createRequest Get url
            |> withHeader (UserAgent "FsharpRx")
            |> HttpClient.getResponseAsync
        let httpResponse = 
            match response.StatusCode with
            | 200 -> response.EntityBody.Value |> Ok
            | _ -> response.StatusCode |> Error
        return httpResponse
    }

let asyncResponseToObservable = getResponseAsync >> Observable.ofAsync


