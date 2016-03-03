module CafeApp

open Data
open Suave.Web
open Api
open EventsStore
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Reactive.Concurrency

let asyncSubscribe<'a> (subject: ISubject<'a>) (f : 'a -> unit) =
  subject
    .ObserveOn(Scheduler.Default)
    .Subscribe f

[<EntryPoint>]
let main argv =     
    let es = inMemoryEventStore.EventStream
    use subscription = asyncSubscribe es projectReadModel
    startWebServer defaultConfig (api inMemoryEventStore)
    0 // return an integer exit code