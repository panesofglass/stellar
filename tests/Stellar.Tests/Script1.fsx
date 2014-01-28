#r "../../src/Stellar/bin/Debug/Stellar.dll"

open Stellar

printfn "%s" MyType.MyProperty

let item1 = MyType()
printfn "%s" item1.InnerState

let item2 = MyType("Different internal state")
printfn "%s" item2.InnerState
