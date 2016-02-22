module Csv
open System.Collections.Generic
open QueryAST
open QueryParser
open FSharp.Data
open Newtonsoft.Json.Linq

let private storage  =
  new Dictionary<string,string>()

let private toLower (s : string) = s.ToLower()

let storeCsv csvName content =
  storage.Add(toLower csvName, content)

let retrieveCsv (From csvName) = storage.[toLower csvName]

let filterRows condition (rows : CsvRow seq) =
  let (Attribute attribute) = condition.Attribute
  let compFn operant =
    match condition.Operator with
    | Equal -> (=) operant
    | NotEqual -> (<>) operant
  let isFilterable (row : CsvRow) =
    match condition.Operant with
    | Int i ->
      row.GetColumn attribute |> int |> compFn i
    | String str ->
      row.GetColumn attribute |> compFn str
  rows
  |> Seq.filter isFilterable

let toJObject headers (Select identifier) (csvRow : CsvRow) =
  let jObject = new JObject()
  match identifier with
  | Asterisk ->
    Seq.zip headers csvRow.Columns
    |> Seq.map (fun (h,v) -> new JProperty(h,v))
    |> Seq.iter jObject.Add
  | Attributes attributes ->
    attributes
    |> Seq.map (fun (Attribute attr) ->
                  new JProperty(attr, csvRow.GetColumn attr))
    |> Seq.iter jObject.Add
  jObject

let applyQuery query csvContent =

  let csv = CsvFile.Parse csvContent
  let rows =
    match query.Where with
    | Some (Where c) ->
        filterRows c csv.Rows
    | None ->
        csv.Rows
  let jArray = new JArray()
  rows
  |> Seq.map (toJObject csv.Headers.Value query.Action)
  |> Seq.iter jArray.Add
  jArray.ToString()

let queryCsv queryDSL =
  match parse queryDSL with
  | Choice1Of2 query ->
      retrieveCsv query.From
      |> applyQuery query
      |> Choice1Of2
  | Choice2Of2 msg ->
      Choice2Of2 msg
