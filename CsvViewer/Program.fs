module CsvViewer

open QueryParser
open Web
open Suave.Web

[<EntryPoint>]
let main argv =
    startWebServer defaultConfig app
    0 // return an integer exit code
