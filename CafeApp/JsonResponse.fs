module JsonResponse
open FSharp.Data
open Fleece
open Fleece.Operators
open Suave
open Suave.Successful
open Suave.Operators
open Suave.RequestErrors
open Suave.ServerErrors

let private jsonHeaderWebPart =
  Writers.setMimeType "application/json; charset=utf-8"

let JSON jsonString = OK jsonString >=> jsonHeaderWebPart

let private toErrorJObj (msg : string) =
  jobj [
    "error" .= msg
  ]

let toRequestErrorJson msg =
  msg |> toErrorJObj |> string |> BAD_REQUEST >=> jsonHeaderWebPart

let toInternalErrorJson msg =
  msg |> toErrorJObj |> string |> INTERNAL_ERROR >=> jsonHeaderWebPart