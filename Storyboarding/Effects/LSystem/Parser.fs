namespace Storyboarding.Effects.LSystem

open System
open Storyboarding.Effects.LSystem.LSystem

module Parser =
    let explode = Seq.toList
    let implode (x: char list) = System.String.Concat(Array.ofList x)

    let isWhitespace c =
        List.contains c [' '; '\n'; '\r'; '\t']

    type input = char list

    (* Parsing results *)
    type 'a ParseResult =
        | Failed of string * string
        | Parsed of 'a * input

    type 'a Parser = input -> 'a ParseResult

    (* return parser *)
    let ret x : _ Parser = fun s -> Parsed(x, s)
    let fail message i = Failed (message, implode i)

    (* Parse char if cond returns true *)
    let parseSymbol cond =
        function
        | h :: t when cond h -> ret h t
        | h :: t -> fail $"symbol \"%c{h}\" not resolved" t
        | _ -> fail "unexpected EOF" []

    let (>>=) p f s =
        match p s with
        | Failed (msg, t) -> Failed (msg, t)
        | Parsed(h, t) -> f h t

    let ( *> ) p1 p2 = p1 >>= fun _ -> p2
    let (<*) p1 p2 = p1 >>= fun h -> p2 *> ret h

    let (>>>) p1 p2 s =
        match p1 s with
        | Parsed(_, t) -> p2 t
        | _ -> p2 s

    (* or operator *)
    let (<|>) p1 p2 s =
        match p1 s with
        | Failed _ -> p2 s
        | res -> res

    let (<<<) p1 p2 s = (p1 <* p2 <|> p1) s

    (* if the parser fails will return None, else ruturs Some 'a *)
    let wrap p i =
        match p i with
        | Parsed(h, t) -> ret (Some h) t
        | Failed _ -> ret None i

    let failIfParsed p inp =
        match p inp with
        | Parsed _ -> fail "success" inp
        | Failed _ -> ret () inp

    (* Parses many that are parsed by parser given *)
    let parseMany (p : 'a Parser) : 'a list Parser =
        let rec helper s =
            match p s with
            | Failed _ -> ret [] s
            | Parsed(h, t) -> (helper >>= fun xs -> ret (h :: xs)) t
        helper

    let parseMany1 p =
        parseMany p >>= function
            | [] -> fail "many1"
            | e -> ret e
    let parse_ws = parseSymbol (fun x -> x = ' ') |> parseMany

    let parseConst (value : string) (result: 'a) =
        let rec inner (c : input) : 'a Parser =
            match c with
            | h :: tl -> parseSymbol ((=) h) *> inner tl
            | _ -> ret result
        inner (explode value)

    let skipLine = parseMany <| parseSymbol (fun x -> List.contains x ['\r'; '\n'])
    let skipWs = parseMany <| parseSymbol (fun x -> List.contains x [' '; '\t'])

    let parseFloat =
        let isDigit x = x >= '0' && x <= '9'
        parseMany1 (parseSymbol isDigit) >>= fun first ->
        parseSymbol ((=) '.') *> parseMany1 (parseSymbol isDigit) >>= fun second ->
        $"{implode first}.{implode second}" |> (float32 >> ret)
        <|> (parseMany1 (parseSymbol isDigit) >>= (implode >> float32 >> ret))

    let parseString =
        let parseNonWhitespace = parseSymbol (isWhitespace >> not)
        parseMany1 parseNonWhitespace >>= fun clst -> ret (implode clst)

    let parseStringList =
        parseMany (parseString <<< skipWs) <<< skipLine

    let parseRule : (LSystemChar * LSystemExpr) Parser =
        parseString <<< skipWs <* parseConst "->" () <<< skipWs >>= (fun i -> parseStringList <<< skipLine >>= (fun l -> ret (i, l)))

    let parseAxiom : LSystemExpr Parser =
        (parseConst "Axiom" () <<< skipWs) *> parseStringList <<< skipLine

    let parseAngle : float32 Parser =
        (parseConst "Angle" () <<< skipWs) *> parseFloat <<< skipLine

    let parseDistance : float32 Parser =
        (parseConst "Distance" () <<< skipWs) *> parseFloat <<< skipLine

    let parseProgram : LSystemProgram Parser =
        parseAxiom <<< skipWs >>= fun axiom ->
        parseAngle <<< skipWs >>= fun angle ->
        parseDistance <<< skipWs >>= fun distance ->
        parseMany parseRule >>= fun rules ->
        ret { axiom = axiom; angle = angle; distance = distance; rules = dict rules }

    let parse<'a> = explode >> parseProgram