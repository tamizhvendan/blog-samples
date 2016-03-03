module CafeApp

open Suave.Web
open Api
open EventsStore
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Reactive.Concurrency
open Data

[<EntryPoint>]
let main argv =
    let asyncEventStream =
      asyncSubject inMemoryEventStore.EventStream
    use subscription = asyncEventStream.Subscribe projectReadModel
    startWebServer defaultConfig (api inMemoryEventStore)
    0 // return an integer exit code