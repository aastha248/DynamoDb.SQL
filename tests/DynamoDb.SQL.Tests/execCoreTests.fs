﻿// Author : Yan Cui (twitter @theburningmonk)

// Email : theburningmonk@gmail.com
// Blog  : http://theburningmonk.com

namespace DynamoDb.SQL.execCore.Tests

open FsUnit
open NUnit.Framework
open DynamoDb.SQL

[<TestFixture>]
type ``Given a DynamoQuery`` () =
    [<Test>]
    member this.``when there is only a equality condition it should be captured in Where clause`` () =
        let query = parseDynamoQuery "SELECT * FROM Employees WHERE FirstName = \"Yan\""

        let (Where(QueryCondition(lst))) = query.Where
        lst |> should equal [ "FirstName", Equal(S "Yan") ]

    [<Test>]
    member this.``when there are multiple conditions they should all be captured in Where clause`` () =
        let query = parseDynamoQuery "SELECT * FROM Employees WHERE Title = \"Developer\" AND Age >= 30 AND FirstName BEGINS WITH \"Y\" AND Age BETWEEN 30 AND 40 "

        let (Where(QueryCondition(lst))) = query.Where
        lst |> should equal [ 
            ("Title",     Equal (S "Developer"));
            ("Age",       GreaterThanOrEqual (NBigInt 30I));
            ("FirstName", BeginsWith (S "Y"));
            ("Age",       Between (NBigInt 30I, NBigInt 40I)) ]

    [<Test>]
    [<ExpectedException(typeof<InvalidQueryException>)>]
    member this.``when there is no filter conditions it should except`` () =
        parseDynamoQuery "SELECT * FROM Employees" |> should throw typeof<InvalidQueryException>

    [<Test>]
    [<ExpectedException(typeof<InvalidQueryFormatException>)>]
    member this.``when there is no equality condition it should except`` () =
        let query = parseDynamoQuery "SELECT * FROM Employees WHERE Age > 30"

        match query with 
        | { Where = Where(QueryCondition _) } -> ()
        |> should throw typeof<InvalidQueryFormatException>

    [<Test>]
    member this.``when there is only an asterisk (*) in the SELECT clause it should return null as attribtue values`` () =
        let query = parseDynamoQuery "SELECT * FROM Employees WHERE FirstName = \"Yan\""

        let (Select(SelectAttributes(lst))) = query.Action
        lst |> should equal null

    [<Test>]
    member this.``when there is an asterisk (*) and other attribute names in the SELECT clause it should return null as attribtue values`` () =
        let query = parseDynamoQuery "SELECT *, Name, Age FROM Employees WHERE FirstName = \"Yan\""

        let (Select(SelectAttributes(lst))) = query.Action
        lst |> should equal null

    [<Test>]
    member this.``when there is no asterisk (*) in the SELECT clause it should return a list of attribtue values`` () =
        let query = parseDynamoQuery "SELECT Name, Age FROM Employees WHERE FirstName = \"Yan\""

        let (Select(SelectAttributes(lst))) = query.Action
        lst |> should equal [ "Name"; "Age" ]
                
[<TestFixture>]
type ``Given a DynamoScan`` () =
    [<Test>]
    member this.``when there are multiple attributes in filter it should be interpreted as a Scan operation`` () =
        let scan = parseDynamoScan "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND LastName != \"Cui\" AND Age >= 30"

        let (Some(Where(ScanCondition lst))) = scan.Where
        lst |> should equal <| [ 
            ("FirstName", Equal (S "Yan"))
            ("LastName", NotEqual (S "Cui"))
            ("Age", GreaterThanOrEqual (NBigInt 30I)) ]

[<TestFixture>]
type ``Given some array of query options`` () =
    // #region NoConsistentRead

    [<Test>]
    member this.``when there is a NoConsistentRead option specified 'isConsistentRead' should return false`` () =
        isConsistentRead (Some [| NoConsistentRead; QueryNoReturnedCapacity |]) 
        |> should equal false

    [<Test>]
    member this.``when there are multiple NoConsistentRead options specified 'isConsistentRead' should return false`` () =
        isConsistentRead (Some [| NoConsistentRead; QueryNoReturnedCapacity; NoConsistentRead |]) 
        |> should equal false

    [<Test>]
    member this.``when no NoConsistentRead option is specified 'isConsistentRead' should return true`` () =
        isConsistentRead (Some [| QueryNoReturnedCapacity |]) |> should equal true

    [<Test>]
    member this.``when there are no query options specified 'isConsistentRead' should return true`` () =
        isConsistentRead None 
        |> should equal true

    // #endregion

    // #region QueryNoReturnedCapacity

    [<Test>]
    member this.``when there is a NoReturnedCapacity option specified 'returnQueryConsumedCapacity' should return false`` () =
        returnQueryConsumedCapacity (Some [| QueryNoReturnedCapacity; NoConsistentRead |]) 
        |> should equal false

    [<Test>]
    member this.``when there are multiple NoReturnedCapacity options specified 'returnQueryConsumedCapacity' should return false`` () =
        returnQueryConsumedCapacity (Some [| QueryNoReturnedCapacity; NoConsistentRead; QueryNoReturnedCapacity |]) 
        |> should equal false

    [<Test>]
    member this.``when no NoReturnedCapacity option is specified 'returnQueryConsumedCapacity' should return true`` () =
        returnQueryConsumedCapacity (Some [| NoConsistentRead |]) 
        |> should equal true

    [<Test>]
    member this.``when there are no query options specified 'returnQueryConsumedCapacity' should return true`` () =
        returnQueryConsumedCapacity None 
        |> should equal true

    // #endregion

    // #region QueryPageSize

    [<Test>]
    member this.``when there is a QueryPageSize(5) option specified 'tryGetQueryPageSize' should return Some 5`` () =
        tryGetQueryPageSize (Some [| QueryPageSize 5; QueryNoReturnedCapacity |]) 
        |> should equal <| Some 5

    [<Test>]
    member this.``when there are multiple QueryPageSize options specified 'tryGetQueryPageSize' should return the first page size`` () =
        tryGetQueryPageSize (Some [| QueryPageSize 5; QueryPageSize 10 |]) 
        |> should equal <| Some 5

    [<Test>]
    member this.``when no QueryPageSize option is specified 'tryGetQueryPageSize' should return None`` () =
        tryGetQueryPageSize (Some [| NoConsistentRead |]) 
        |> should equal None

    [<Test>]
    member this.``when there are no query options specified 'tryGetQueryPageSize' should return None`` () =
        tryGetQueryPageSize None 
        |> should equal None

    // #endregion

    // #region Index

    [<Test>]
    member this.``when there is a Index(\"MyIndex\", false) option specified 'tryGetQueryIndex' should return Some(\"MyIndex\", false)`` () =
        tryGetQueryIndex (Some [| Index("MyIndex", false) |]) 
        |> should equal <| Some("MyIndex", false)

    [<Test>]
    member this.``when there is a Index(\"MyIndex\", true) option specified 'tryGetQueryIndex' should return Some(\"MyIndex\", true)`` () =
        tryGetQueryIndex (Some [| Index("MyIndex", true) |]) 
        |> should equal <| Some("MyIndex", true)

    [<Test>]
    member this.``when there are multiple Index options specified 'tryGetQueryIndex' should return the first index`` () =
        tryGetQueryIndex (Some [| Index("MyIndex", false); Index("MyIndex2", true) |]) 
        |> should equal <| Some("MyIndex", false)

    [<Test>]
    member this.``when no Index option is specified 'tryGetQueryIndex' should return None`` () =
        tryGetQueryIndex (Some [| NoConsistentRead |]) 
        |> should equal None

    [<Test>]
    member this.``when there are no query options specified 'tryGetQueryIndex' should return None`` () =
        tryGetQueryIndex None 
        |> should equal None

    // #endregion
    
    // #region ScanNoReturnedCapacity

    [<Test>]
    member this.``when there is a NoReturnedCapacity option specified 'returnScanConsumedCapacity' should return false`` () =
        returnScanConsumedCapacity (Some [| ScanNoReturnedCapacity |]) 
        |> should equal false

    [<Test>]
    member this.``when there are multiple NoReturnedCapacity options specified 'returnScanConsumedCapacity' should return false`` () =
        returnScanConsumedCapacity (Some [| ScanNoReturnedCapacity; ScanNoReturnedCapacity |]) 
        |> should equal false

    [<Test>]
    member this.``when no NoReturnedCapacity option is specified 'returnScanConsumedCapacity' should return true`` () =
        returnScanConsumedCapacity (Some [| |]) 
        |> should equal true

    [<Test>]
    member this.``when there are no query options specified 'returnScanConsumedCapacity' should return true`` () =
        returnScanConsumedCapacity None 
        |> should equal true

    // #endregion
    
    // #region ScanPageSize

    [<Test>]
    member this.``when there is a ScanPageSize(5) option specified 'tryGetScanPageSize' should return Some 5`` () =
        tryGetScanPageSize (Some [| ScanPageSize 5 |]) 
        |> should equal <| Some 5

    [<Test>]
    member this.``when there are multiple ScanPageSize options specified 'tryGetScanPageSize' should return the first page size`` () =
        tryGetScanPageSize (Some [| ScanPageSize 5; ScanPageSize 10 |]) 
        |> should equal <| Some 5

    [<Test>]
    member this.``when no ScanPageSize option is specified 'tryGetScanPageSize' should return None`` () =
        tryGetScanPageSize (Some [||]) 
        |> should equal None

    [<Test>]
    member this.``when there are no query options specified 'tryGetScanPageSize' should return None`` () =
        tryGetScanPageSize None 
        |> should equal None

    // #endregion