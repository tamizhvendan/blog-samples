module ReadModel
open Chessie.ErrorHandling
open Domain
open Events
open System.Reactive.Subjects



let eventStream = new Subject<Event>();

let dispatchEvent e =
  eventStream.OnNext(snd e)
  e |> ok