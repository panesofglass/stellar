namespace Stellar

module Util =
    type Microsoft.FSharp.Control.Async with
        static member AwaitTask(task: System.Threading.Tasks.Task) =
            task |> Async.AwaitIAsyncResult |> Async.Ignore
