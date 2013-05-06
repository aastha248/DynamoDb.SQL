﻿// Copyright (c) Yan Cui 2012

// Email : theburningmonk@gmail.com
// Blog  : http://theburningmonk.com

module DynamoDbV2.SQL.Execution.LowLevel.Tests

open FsUnit
open NUnit.Framework
open DynamoDb.SQL
open DynamoDb.SQL.Parser
open DynamoDbV2.SQL.Execution

let equal = FsUnit.equal

[<TestFixture>]
type ``Given a V2 DynamoQuery`` () =
    [<Test>]
    member this.``when there is only an equality filter then KeyConditions should contain a single key condition`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "SELECT * FROM Employees WHERE FirstName = \"Yan\""

        req.TableName                                            |> should equal "Employees"
        req.KeyConditions.Count                                  |> should equal 1
        req.KeyConditions.ContainsKey("FirstName")               |> should equal true
        req.KeyConditions.["FirstName"].AttributeValueList.Count |> should equal 1
        req.KeyConditions.["FirstName"].AttributeValueList.[0].S |> should equal "Yan"
        req.KeyConditions.["FirstName"].ComparisonOperator       |> should equal "EQ"
        req.AttributesToGet                                      |> should equal null
        
    [<Test>]
    member this.``when asterisk is used and no Index option is specified then Select should default to 'ALL_ATTRIBUTES'`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "SELECT * FROM Employees WHERE FirstName = \"Yan\""

        req.TableName                                            |> should equal "Employees"
        req.KeyConditions.Count                                  |> should equal 1
        req.KeyConditions.ContainsKey("FirstName")               |> should equal true
        req.KeyConditions.["FirstName"].AttributeValueList.Count |> should equal 1
        req.KeyConditions.["FirstName"].ComparisonOperator       |> should equal "EQ"
        req.AttributesToGet                                      |> should equal null
        req.Select                                               |> should equal "ALL_ATTRIBUTES"

    [<Test>]
    member this.``when a number of attributes were specified in the SELECT clause then they should be captured in AttributesToGet and Select should be set to 'SPECIFIC_ATTRIBUTES'`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "SELECT FirstName, LastName, Age FROM Employees WHERE FirstName = \"Yan\""

        req.TableName                   |> should equal "Employees"
        req.AttributesToGet.Count       |> should equal 3
        req.AttributesToGet.ToArray()   |> should equal [| "FirstName"; "LastName"; "Age" |]
        req.Select                      |> should equal "SPECIFIC_ATTRIBUTES"

    [<Test>]
    member this.``when there are more than one filter condition specified then they should all be captured in KeyConditions`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND Age BETWEEN 30 AND 40 And LastName BEGINS WITH \"C\""

        req.TableName                                               |> should equal "Employees"
        req.KeyConditions.Count                                     |> should equal 3

        req.KeyConditions.ContainsKey("FirstName")                  |> should equal true
        req.KeyConditions.["FirstName"].AttributeValueList.Count    |> should equal 1
        req.KeyConditions.["FirstName"].ComparisonOperator          |> should equal "EQ"

        req.KeyConditions.ContainsKey("Age")                        |> should equal true
        req.KeyConditions.["Age"].AttributeValueList.Count          |> should equal 2
        req.KeyConditions.["Age"].AttributeValueList.[0].N          |> should equal "30"
        req.KeyConditions.["Age"].AttributeValueList.[1].N          |> should equal "40"
        req.KeyConditions.["Age"].ComparisonOperator                |> should equal "BETWEEN"

        req.KeyConditions.ContainsKey("LastName")                   |> should equal true
        req.KeyConditions.["LastName"].AttributeValueList.Count     |> should equal 1
        req.KeyConditions.["LastName"].AttributeValueList.[0].S     |> should equal "C"
        req.KeyConditions.["LastName"].ComparisonOperator           |> should equal "BEGINS_WITH"

    [<Test>]
    member this.``when an Index option is specified with AllAttributes set to true then Select should be set to 'ALL_ATTRIBUTES'`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "SELECT * FROM Employees WHERE FirstName = \"Yan\" WITH (INDEX(MyIndex, true))"

        req.TableName    |> should equal "Employees"

        req.Select       |> should equal "ALL_ATTRIBUTES"
        req.IndexName    |> should equal "MyIndex"

    [<Test>]
    member this.``when an Index option is specified with AllAttributes set to false then Select should be set to 'ALL_PROJECTED_ATTRIBUTES'`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "SELECT * FROM Employees WHERE FirstName = \"Yan\" WITH (INDEX(MyIndex, false))"

        req.TableName   |> should equal "Employees"

        req.Select      |> should equal "ALL_PROJECTED_ATTRIBUTES"
        req.IndexName   |> should equal "MyIndex"

    [<Test>]
    member this.``when the ASC order is specified then ScanIndexForward should be set to true`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "SELECT * FROM Employees WHERE FirstName = \"Yan\" ORDER ASC"

        req.TableName        |> should equal "Employees"
        req.ScanIndexForward |> should equal true

    [<Test>]
    member this.``when the DESC order is specified then ScanIndexForward should be set to false`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "SELECT * FROM Employees WHERE FirstName = \"Yan\" ORDER DESC"

        req.TableName        |> should equal "Employees"
        req.ScanIndexForward |> should equal false

    [<Test>]
    member this.``when the NoConsistentRead option is specified then ConsistentRead should be set to false`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "SELECT * FROM Employees WHERE FirstName = \"Yan\" WITH (NoConsistentRead)"

        req.TableName       |> should equal "Employees"
        req.ConsistentRead  |> should equal false

    [<Test>]
    member this.``when no NoConsistentRead option is specified then ConsistentRead should be default to true`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "SELECT * FROM Employees WHERE FirstName = \"Yan\""

        req.TableName       |> should equal "Employees"
        req.ConsistentRead  |> should equal true

    [<Test>]
    member this.``when the NoReturnedCapacity option is specified then ReturnConsumedCapacity should be set to 'None'`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "SELECT * FROM Employees WHERE FirstName = \"Yan\" WITH (NoReturnedCapacity)"

        req.TableName               |> should equal "Employees"
        req.ReturnConsumedCapacity  |> should equal "NONE"

    [<Test>]
    member this.``when no NoReturnedCapacity option is specified then ReturnConsumedCapacity should be default to 'TOTAL'`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "SELECT * FROM Employees WHERE FirstName = \"Yan\""

        req.TableName               |> should equal "Employees"
        req.ReturnConsumedCapacity  |> should equal "TOTAL"

    [<Test>]
    member this.``when the QueryPageSize option is specified to be 5 then Limit should be set to 5`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "SELECT * FROM Employees WHERE FirstName = \"Yan\" WITH (PageSize(5))"

        req.TableName   |> should equal "Employees"
        req.Limit       |> should equal 5

    [<Test>]
    member this.``when the query is a Count query then Select should be set to 'COUNT'`` () =
        let (GetQueryReq req) = parseDynamoQueryV2 "COUNT * FROM Employees WHERE FirstName = \"Yan\""

        req.TableName   |> should equal "Employees"
        req.Select      |> should equal "COUNT"

[<TestFixture>]
type ``Given a V2 DynamoScan`` () =
    [<Test>]
    member this.``when there is no where clause it should return a ScanRequest with empty ScanFilter`` () =
        let (GetScanReq req) = parseDynamoScanV2 "SELECT * FROM Employees"
               
        req.TableName                                            |> should equal "Employees"
        req.Limit                                                |> should equal 0
        req.AttributesToGet                                      |> should equal null
        req.ScanFilter.Count                                     |> should equal 0

    [<Test>]
    member this.``when asterisk is used then Select should default to 'ALL_ATTRIBUTES'`` () =
        let (GetScanReq req) = parseDynamoScanV2 "SELECT * FROM Employees"

        req.TableName                                            |> should equal "Employees"
        req.Select                                               |> should equal "ALL_ATTRIBUTES"

    [<Test>]
    member this.``when a number of attributes were specified in the SELECT clause then they should be captured in AttributesToGet and Select should be set to 'SPECIFIC_ATTRIBUTES'`` () =
        let (GetScanReq req) = parseDynamoScanV2 "SELECT FirstName, LastName, Age FROM Employees"

        req.TableName                                            |> should equal "Employees"
        req.Select                                               |> should equal "SPECIFIC_ATTRIBUTES"

    [<Test>]
    member this.``when filter conditions are specified they should all be captured in ScanFilter`` () =
        let (GetScanReq req) = parseDynamoScanV2 "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND Age BETWEEN 30 AND 40 And LastName BEGINS WITH \"C\""

        req.TableName                                               |> should equal "Employees"
        req.ScanFilter.Count                                     |> should equal 3

        req.ScanFilter.ContainsKey("FirstName")                  |> should equal true
        req.ScanFilter.["FirstName"].AttributeValueList.Count    |> should equal 1
        req.ScanFilter.["FirstName"].ComparisonOperator          |> should equal "EQ"

        req.ScanFilter.ContainsKey("Age")                        |> should equal true
        req.ScanFilter.["Age"].AttributeValueList.Count          |> should equal 2
        req.ScanFilter.["Age"].AttributeValueList.[0].N          |> should equal "30"
        req.ScanFilter.["Age"].AttributeValueList.[1].N          |> should equal "40"
        req.ScanFilter.["Age"].ComparisonOperator                |> should equal "BETWEEN"

        req.ScanFilter.ContainsKey("LastName")                   |> should equal true
        req.ScanFilter.["LastName"].AttributeValueList.Count     |> should equal 1
        req.ScanFilter.["LastName"].AttributeValueList.[0].S     |> should equal "C"
        req.ScanFilter.["LastName"].ComparisonOperator           |> should equal "BEGINS_WITH"

    [<Test>]
    member this.``when the ScanPageSize option is specified to be 5 then Limit should be set to 5`` () =
        let (GetScanReq req) = parseDynamoScanV2 "SELECT * FROM Employees WITH (PageSize(5))"

        req.TableName   |> should equal "Employees"
        req.Limit       |> should equal 5

    [<Test>]
    member this.``when the scan is a Count scan then Select should be set to 'COUNT'`` () =
        let (GetScanReq req) = parseDynamoScanV2 "COUNT * FROM Employees"

        req.TableName   |> should equal "Employees"
        req.Select      |> should equal "COUNT"