#r "System.dll"
#r "packages/FParsec/lib/net40-client/FParsecCS.dll"
#r "packages/FParsec/lib/net40-client/FParsec.dll"
#r "packages/Fsharp.Data/lib/net40/FSharp.Data.dll"
#r "packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
#load "QueryAST.fs"
#load "QueryParser.fs"
#load "Csv.fs"


open Csv

storeCsv "People" "id,name,age\n1,Tam,27\n2,Ram,28"
queryCsv "select name,age from People"
