module ReadModelApi
open Suave
open Suave.Successful
open Data

let getTables =
  warbler (fun _ -> getTables() |> sprintf "%A" |> OK)

let getChefToDos =
  warbler (fun _ -> getChefToDos() |> sprintf "%A" |> OK)

let getWaiterToDos =
  warbler (fun _ -> getWaiterToDos() |> sprintf "%A" |> OK)