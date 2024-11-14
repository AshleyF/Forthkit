open System.IO
open System.Text

let reg = function
    |  0uy -> "n"
    |  1uy -> "x"
    |  2uy -> "d"
    |  3uy -> "lnk"
    |  4uy -> "m"
    |  5uy -> "zero"
    |  6uy -> "one"
    |  7uy -> "two"
    |  8uy -> "ten"
    |  9uy -> "true"
    | 10uy -> "false"
    | 11uy -> "zeroch"
    | 12uy -> "rparch"
    | 13uy -> "spch"
    | 14uy -> "negch"
    | 15uy -> "tib"
    | 16uy -> "len"
    | 17uy -> "len'"
    | 18uy -> "nm"
    | 19uy -> "p"
    | 20uy -> "c"
    | 21uy -> "p'"
    | 22uy -> "c'"
    | 23uy -> "cur"
    | 24uy -> "s"
    | 25uy -> "ldc"
    | 26uy -> "call"
    | 27uy -> "ret"
    | 28uy -> "comp"
    | 29uy -> "sign"
    | 30uy -> "y"
    | 31uy -> "z"
    | r -> "???"

let hex = sprintf "%04X"
let bytehex = int >> sprintf "%02X"
let valuexy x y = int x ||| (int y <<< 8)
let value x y = valuexy x y |> sprintf "%04X"

let known = Map.ofList [
    0x0043, "'skipws"
    0x004B, "'name"
    0x0054, "'skipnone"
    0x0064, "'token"
    0x0070, "'nomatch"
    0x0074, "'compcs"
    0x008B, "'nextw"
    0x008E, "'comp"
    0x00B2, "'find"
    0x00BD, "'error"
    0x00D4, "'negate"
    0x00DA, "'digits"
    0x0100, "'parsenum"
    0x0115, "'pushn"
    0x011D, "'popn"
    0x0125, "'litn"
    0x014A, "'num"
    0x0155, "'exec"
    0x015C, "'compw"
    0x0179, "'word"
    0x0181, "'eval"
    0x018C, "'repl"
    0x02BD, "PAST KERNEL (02BD)"]

let name labels addr =
    match Map.tryFind addr labels with
    | Some name -> name
    | None -> $"???????????? {hex addr} ???????????? "

let rec word labels addr nm = function
    | len :: l0 :: l1 :: immediate :: t when len <= 32uy ->
        if int len <> List.length nm then printfn "UNKNOWN"; disassemble labels addr (List.tail t)
        let name' = Encoding.ASCII.GetString(nm |> List.rev |> Array.ofList)
        printfn $"---- {name'} -----------------------------"
        printfn $"{hex addr} WORD {name'} {bytehex len} {value l0 l1} {bytehex immediate} --> {valuexy l0 l1 |> (fun x -> x + 3) |> name labels}"
        let addr' = addr + int len + 1 + 2 + 1
        disassemble (Map.add addr' name' labels) addr' t
    | c :: t -> word labels addr (c :: nm) t
    | [] -> printfn "END"
    | t -> printfn "UNKNOWN"; disassemble labels addr (List.tail t)

and disassemble labels addr =
    match Map.tryFind addr labels with
    | Some name when name.StartsWith("'") && name <> "'" -> printfn $"---- {name} -----------------------------"
    | _ -> ()
    let nm x y = valuexy x y |> name labels
    function
    |  0uy :: 0uy              :: t -> printfn "EMPTY EMPTY"                                             ; disassemble labels (addr + 2) t
    |  0uy                     :: t -> printfn $"{hex addr} HALT"                                        ; disassemble labels (addr + 1) t
    |  1uy :: x :: y :: z      :: t -> printfn $"{hex addr} LDC  {reg z} = {value x y}"                  ; disassemble labels (addr + 4) t
    |  2uy :: x :: y           :: t -> printfn $"{hex addr} LD   {reg x} = m[{reg y}]"                   ; disassemble labels (addr + 3) t
    |  3uy :: x :: y           :: t -> printfn $"{hex addr} ST   m[{reg x}] = {reg y}"                   ; disassemble labels (addr + 3) t
    |  4uy :: x :: y           :: t -> printfn $"{hex addr} LDB  {reg x} = m[{reg y}]"                   ; disassemble labels (addr + 3) t
    |  5uy :: x :: y           :: t -> printfn $"{hex addr} STB  m[{reg x}] = {reg y}"                   ; disassemble labels (addr + 3) t
    |  6uy :: x :: y           :: t -> printfn $"{hex addr} CP   {reg x} = {reg y}"                      ; disassemble labels (addr + 3) t
    |  7uy :: x                :: t -> printfn $"{hex addr} IN   {reg x} = getc()"                       ; disassemble labels (addr + 2) t
    |  8uy :: x                :: t -> printfn $"{hex addr} OUT  putc({reg x})"                          ; disassemble labels (addr + 2) t
    |  9uy :: x :: y           :: t -> printfn $"{hex addr} INC  {reg x} = ++{reg y}"                    ; disassemble labels (addr + 3) t
    | 10uy :: x :: y           :: t -> printfn $"{hex addr} DEC  {reg x} = --{reg y}"                    ; disassemble labels (addr + 3) t
    | 11uy :: x :: y :: z      :: t -> printfn $"{hex addr} ADD  {reg x} = {reg y} + {reg z}"            ; disassemble labels (addr + 4) t
    | 12uy :: x :: y :: z      :: t -> printfn $"{hex addr} SUB  {reg x} = {reg y} - {reg z}"            ; disassemble labels (addr + 4) t
    | 13uy :: x :: y :: z      :: t -> printfn $"{hex addr} MUL  {reg x} = {reg y} * {reg z}"            ; disassemble labels (addr + 4) t
    | 14uy :: x :: y :: z      :: t -> printfn $"{hex addr} DIV  {reg x} = {reg y} / {reg z}"            ; disassemble labels (addr + 4) t
    | 15uy :: x :: y :: z      :: t -> printfn $"{hex addr} MOD  {reg x} = {reg y} m {reg z}"            ; disassemble labels (addr + 4) t
    | 16uy :: x :: y :: z      :: t -> printfn $"{hex addr} AND  {reg x} = {reg y} & {reg z}"            ; disassemble labels (addr + 4) t
    | 17uy :: x :: y :: z      :: t -> printfn $"{hex addr} OR   {reg x} = {reg y} | {reg z}"            ; disassemble labels (addr + 4) t
    | 18uy :: x :: y :: z      :: t -> printfn $"{hex addr} XOR  {reg x} = {reg y} ^ {reg z}"            ; disassemble labels (addr + 4) t
    | 19uy :: x :: y           :: t -> printfn $"{hex addr} NOT  {reg x} = ~{reg y}"                     ; disassemble labels (addr + 3) t
    | 20uy :: x :: y :: z      :: t -> printfn $"{hex addr} SHL  {reg x} = {reg y} << {reg z}"           ; disassemble labels (addr + 4) t
    | 21uy :: x :: y :: z      :: t -> printfn $"{hex addr} SHR  {reg x} = {reg y} >> {reg z}"           ; disassemble labels (addr + 4) t
    | 22uy :: x :: y :: z :: w :: t -> printfn $"{hex addr} BEQ  {nm x y} if {reg z} = {reg w}"          ; disassemble labels (addr + 5) t
    | 23uy :: x :: y :: z :: w :: t -> printfn $"{hex addr} BNE  {nm x y} if {reg z} <> {reg w}"         ; disassemble labels (addr + 5) t
    | 24uy :: x :: y :: z :: w :: t -> printfn $"{hex addr} BGT  {nm x y} if {reg z} > {reg w}"          ; disassemble labels (addr + 5) t
    | 25uy :: x :: y :: z :: w :: t -> printfn $"{hex addr} BGE  {nm x y} if {reg z} >= {reg w}"         ; disassemble labels (addr + 5) t
    | 26uy :: x :: y :: z :: w :: t -> printfn $"{hex addr} BLT  {nm x y} if {reg z} < {reg w}"          ; disassemble labels (addr + 5) t
    | 27uy :: x :: y :: z :: w :: t -> printfn $"{hex addr} BLE  {nm x y} if {reg z} <= {reg w}"         ; disassemble labels (addr + 5) t
    | 28uy :: x :: y           :: t -> printfn $"{hex addr} JUMP {nm x y}"                               ; disassemble labels (addr + 3) t
    | 29uy :: x :: y           :: t -> printfn $"{hex addr} CALL {nm x y}"                               ; disassemble labels (addr + 3) t
    | 30uy :: x                :: t -> printfn $"{hex addr} EXEC pc = {reg x}"                           ; disassemble labels (addr + 2) t
    | 31uy                     :: t -> printfn $"{hex addr} RET"                                         ; disassemble labels (addr + 1) t
    | 32uy :: x :: y :: z      :: t -> printfn $"{hex addr} READ ({x},{y},{z})"                          ; disassemble labels (addr + 1) t
    | 33uy :: x :: y :: z      :: t -> printfn $"{hex addr} WRITE ({x},{y},{z})"                         ; disassemble labels (addr + 1) t
    | 255uy :: x :: y          :: t -> printfn $"{hex addr} ALLOT {valuexy x y}"                         ; disassemble labels (addr + 3 + (valuexy x y)) (List.skip (valuexy x y) t)
    |                             t -> word labels addr [] t

File.ReadAllBytes("../../../block0.bin")
|> List.ofArray
|> disassemble known 0
