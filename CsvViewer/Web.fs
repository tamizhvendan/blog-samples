module Web

open Suave
open Suave.Successful
open Suave.RequestErrors
open Suave.Operators
open Suave.Filters
open System.IO
open Csv
open Suave.Files

let handleCsvFileUpload (request : HttpRequest) =
  //printfn "%A" request
  match request.["tableName"] with
  | Some tableName ->
    if List.length request.files = 1 then
      let file = List.head request.files
      file.tempFilePath
      |> File.ReadAllText
      |> storeCsv tableName
      Redirection.redirect "/?msg=File Uploaded Successfully"
    else
      BAD_REQUEST "Upload a CSV file"
  | None->
    BAD_REQUEST "Request doesn't contain Table Name "

let handleCsvQuery (request : HttpRequest) =
  match request.["query"] with
  | Some query ->
    match queryCsv query with
    | Choice1Of2 json -> OK json
    | Choice2Of2 err -> BAD_REQUEST err
  | None ->
    BAD_REQUEST "Request doesn't contain Query"


let app =
  choose [
    POST >=> path "/" >=> (request handleCsvFileUpload)
    GET >=> path "/view" >=> (request handleCsvQuery)
    GET >=> path "/" >=> file "index.html"
    GET >=> path "/index.js" >=> file "index.js"
  ]
