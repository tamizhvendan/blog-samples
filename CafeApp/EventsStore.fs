module EventsStore
open NEventStore
open NEventStore.Persistence.InMemory
open Chessie.ErrorHandling
open Domain
open Events
open Aggregates
open Errors

type InMemoryEventStore () =
  static let instance() =
    lazy(Wireup.Init()
      .UsingInMemoryPersistence()
      .Build())
  static member Instance =
    instance().Force()


let saveEvent (eventStore : IStoreEvents) (state,event) =
  let tabId = function
    | ClosedTab -> None
    | OpenedTab tab -> Some tab.Id
    | PlacedOrder op -> Some op.Tab.Id
    | OrderInProgress ipo -> Some ipo.PlacedOrder.Tab.Id
    | OrderServed payment -> Some payment.Tab.Id
  match tabId state with
  | Some id ->
      try
        use stream = eventStore.OpenStream(id.ToString())
        stream.Add(EventMessage(Body = event))
        stream.CommitChanges(System.Guid.NewGuid())
        (state,event) |> ok
      with
        | ex -> ErrorWhileSavingEvent ex |> fail
  | None ->
      InvalidStateForSavingEvent |> fail