module ReadModelApi
open Suave
open Suave.Successful
open Data

let getTables =
  warbler (fun _ -> getTables() |> sprintf "%A" |> OK)

let getChefTodos =
  warbler (fun _ -> getChefTodos() |> sprintf "%A" |> OK)