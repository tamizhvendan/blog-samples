module EventsStore
open NEventStore
open NEventStore.Persistence.InMemory
open Chessie.ErrorHandling
open Domain
open Events
open Aggregates
open Errors
open System

type EventStore = {
  GetState : Guid -> Result<State, Error>
  GetEvents : Guid -> Result<Event list, Error>
  SaveEvent : State * Event -> Result<State * Event, Error>
}
let saveEvent (eventStore : IStoreEvents) (state,event) =
  let tabId = function
    | ClosedTab None -> None
    | OpenedTab tab -> Some tab.Id
    | PlacedOrder po -> Some po.TabId
    | OrderInProgress ipo -> Some ipo.PlacedOrder.TabId
    | OrderServed payment -> Some payment.TabId
    | ClosedTab (Some tabId) -> Some tabId
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

let getEvents (eventStore : IStoreEvents) (tabId : System.Guid) =
  try
    use stream = eventStore.OpenStream (tabId.ToString())
    stream.CommittedEvents
    |> Seq.map (fun msg -> msg.Body)
    |> Seq.cast<Event>
    |> Seq.toList
    |> ok
  with
    | ex -> ErrorWhileRetrievingEvents ex |> fail

let getState (eventStore : IStoreEvents) (tabId : System.Guid) =
  match getEvents eventStore tabId with
  | Ok (events,_) ->
    events
    |> Seq.fold apply (ClosedTab None)
    |> ok
  | Bad x -> Bad x

type InMemoryEventStore () =
  static member Instance =
                  Wireup.Init()
                    .UsingInMemoryPersistence()
                    .Build()

let inMemoryEventStore =
  let instance = InMemoryEventStore.Instance
  {
      SaveEvent = saveEvent instance
      GetState = getState instance
      GetEvents = getEvents instance
  }