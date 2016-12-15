module Suave.TwoFactorAuth.Main

open Suave
open Suave.Web
open Suave.Successful
open System.IO
open System.Reflection
open DotLiquid
open Suave.DotLiquid
open Suave.TwoFactorAuth.Web

let currentDirectory =
    let mainExeFileInfo = new FileInfo(Assembly.GetEntryAssembly().Location)
    mainExeFileInfo.Directory
let viewsDirectory = Path.Combine(currentDirectory.FullName, "views")
setTemplatesDir viewsDirectory

[<EntryPoint>]
let main argv =  
  startWebServer defaultConfig app
  0 // return an integer exit code
