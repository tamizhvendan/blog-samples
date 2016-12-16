module Suave.TwoFactorAuth.Main

open Suave
open System.IO
open System.Reflection
open Suave.DotLiquid

open Web

let initializeDotLiquid () =
  let currentDirectory =
    let mainExeFileInfo = 
      new FileInfo(Assembly.GetEntryAssembly().Location)
    mainExeFileInfo.Directory
  Path.Combine(currentDirectory.FullName, "views") 
  |> setTemplatesDir

[<EntryPoint>]
let main argv =  
  initializeDotLiquid ()
  startWebServer defaultConfig app
  0 // return an integer exit code
