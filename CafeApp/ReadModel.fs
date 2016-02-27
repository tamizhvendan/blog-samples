module ReadModel
open Chessie.ErrorHandling
open Domain
open Events
open Data

let projectReadModel e =
  match e with
  | TabOpened tab ->
      updateTableStatus tab.TableNumber (Open tab.Id)
  | OrderPlaced placedOrder ->
      //{
      //  Tab = placedOrder.Tab
      //  FoodItems = placedOrder.FoodItems
      //} |> ignore
      ()
  | FoodPrepared pf ->
      pf |> ignore
  | TabClosed payment ->
      updateTableStatus payment.Tab.TableNumber Closed
  | _ -> ()



let dispatchEvent e =
  projectReadModel (snd e)
  e |> ok