namespace Suave

module Http =

  type RequestType = GET | POST

  type Request = {
    Route : string
    Type : RequestType
  }

  type Response = {
    Content : string
    StatusCode : int
  }

  type Context = {
    Request : Request
    Response : Response
  }

  type WebPart = Context -> Async<Context option>

module Succuessful =
  open Http

  let OK content context =
    {context with Response = {Content = content; StatusCode = 200}}
    |> Some
    |> async.Return


module Combinators =

  let bind f asyncX = async {
    let! x = asyncX
    match x with
    | None -> return None
    | Some y ->
      let z = f y
      return! z
  }

  let compose first second x =
    bind second (first x)

  let (>=>) first second =
    compose first second

module Filters =
  open Http

  let iff condition context =
    if condition context then
      context |> Some |> async.Return
    else
      None |> async.Return

  let GET = iff (fun context -> context.Request.Type = GET)
  let POST = iff (fun context -> context.Request.Type = POST)
  let Path path = iff (fun context -> context.Request.Route = path)

  let rec Choose webparts context = async {
    match webparts with
    | [] -> return None
    | x :: xs ->
      let! result = x context
      match result with
      | Some x -> return Some x
      | None -> return! Choose xs context
  }

module Console =
  open Http
  let execute inputContext webpart =
    async {
      let! outputContext = webpart inputContext
      match outputContext with
      | Some context ->
        printfn "--------------"
        printfn "Code : %d" context.Response.StatusCode
        printfn "Output : %s" context.Response.Content
        printfn "--------------"
      | None ->
        printfn "No Output"
    } |> Async.RunSynchronously

  let parseRequest (input : System.String) =
    let parts = input.Split([|';'|])
    let rawType = parts.[0]
    let route = parts.[1]
    match rawType with
    | "GET" -> {Type = GET; Route = route}
    | "POST" -> {Type = POST; Route = route}
    | _ -> failwith "invalid request"

  let executeInLoop inputContext webpart =
    let mutable continueLooping = true
    while continueLooping do
      printf "Enter Input Route : "
      let input = System.Console.ReadLine()
      try
        if input = "exit" then
          continueLooping <- false
        else
          let context = {inputContext with Request = parseRequest input}
          execute context webpart
      with
        | ex ->
          printfn "Error : %s" ex.Message
