namespace Suave

module Http =

  type RequestType = GET | POST

  type Request = {
    Route : string
    Type : RequestType
  }

  type Response = {
    Output : string
    Code : string
  }

  type Context = {
    Request : Request
    Response : Response
  }

  type WebPart = Context -> Async<Context option>

module Succuessful =
  open Http

  let OK s context =
    {context with Response = {Output = s; Code = "200"}}
    |> Some
    |> async.Return


module Console =
  open Http
  let execute inputContext webpart =
    async {
      let! outputContext = webpart inputContext
      match outputContext with
      | Some context ->
        printfn "--------------"
        printfn "Code : %s" context.Response.Code
        printfn "Output : %s" context.Response.Output
        printfn "--------------"
      | None ->
        printfn "No Output"
    } |> Async.RunSynchronously
