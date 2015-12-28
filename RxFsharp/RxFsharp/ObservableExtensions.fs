module ObservableExtensions
open FSharp.Control.Reactive

let flatmap2 f observable =
    observable
    |> Observable.flatmap (Array.map f >> Observable.mergeArray)
    |> Observable.toArray
