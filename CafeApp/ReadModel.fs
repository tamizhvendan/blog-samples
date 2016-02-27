module ReadModel
open Chessie.ErrorHandling
open Domain
open Events

type ChefTodo = {
  Tab : Tab
  FoodItems : FoodItem list
}

let projectReadModel e =
  match e with
  | TabOpened tab ->
      tab |> ignore
  | OrderPlaced placedOrder ->
      {
        Tab = placedOrder.Tab
        FoodItems = placedOrder.FoodItems
      } |> ignore
  | FoodPrepared pf ->
      pf |> ignore
  | TabClosed payment ->
      payment |> ignore
  | _ -> ()

let dispatchEvent e =
  projectReadModel (snd e)
  e |> ok