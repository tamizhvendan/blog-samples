module ReadModelApi
open Suave
open Suave.Successful
open Data
open JsonResponse

let getTables =
  warbler (fun _ -> getTables() |> toTablesJson)

let getChefToDos =
  warbler (fun _ -> getChefToDos() |> toChefToDosJson)

let getWaiterToDos =
  warbler (fun _ -> getWaiterToDos() |> toWaiterToDosJson)

let getCashierToDos =
  warbler (fun _ -> getCashierToDos() |> toCashierToDosJson)