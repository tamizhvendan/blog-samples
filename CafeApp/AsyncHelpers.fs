module AsyncHelpers
open System

/// Helper that can be used for writing CPS-style code that resumes
/// on the same thread where the operation was started.
let internal synchronize f =
  let ctx = System.Threading.SynchronizationContext.Current
  f (fun g ->
    let nctx = System.Threading.SynchronizationContext.Current
    if isNull ctx && ctx <> nctx then ctx.Post((fun _ -> g()), null)
    else g() )

type Microsoft.FSharp.Control.Async with
  /// Creates an asynchronous workflow that will be resumed when the
  /// specified observables produces a value. The workflow will return
  /// the value produced by the observable.
  static member AwaitObservable(ev1:IObservable<'T1>) =
    synchronize (fun f ->
      Async.FromContinuations((fun (cont,econt,ccont) ->
        let called = ref false
        let rec finish cont value =
          remover.Dispose()
          f (fun () -> lock called (fun () ->
              if not called.Value then
                 cont value
                 called.Value <- true) )
        and remover : IDisposable =
          ev1.Subscribe
            ({ new IObserver<_> with
                  member x.OnNext(v) = finish cont v
                  member x.OnError(e) = finish econt e
                  member x.OnCompleted() =
                    let msg = "Cancelling the workflow, because the Observable awaited using AwaitObservable has completed."
                    finish ccont (new System.OperationCanceledException(msg)) })
        () )))