﻿// Copyright (c) Yan Cui 2012

// Email : theburningmonk@gmail.com
// Blog  : http://theburningmonk.com

namespace DynamoDb.SQL.Execution

open System.Collections.Generic
open System.Runtime.CompilerServices
open DynamoDb.SQL
open Amazon.DynamoDB.DataModel
open Amazon.DynamoDB.DocumentModel

[<AutoOpen>]
module internal Cxt =
    /// Active pattern for getting the query operation config out of a DynamoQuery
    val (|GetQueryConfig|) : DynamoQuery -> QueryOperationConfig
    
    /// Active pattern for getting the scan operation config out of a DynamoScan
    val (|GetScanConfig|)  : DynamoScan  -> ScanOperationConfig

/// Extension methods for the DynamoDBContext class to be used in F#
[<AutoOpen>]
module ContextExt =
    type DynamoDBContext with
        /// Executes a query synchronously and returns the results
        member ExecQuery<'T>    : string -> IEnumerable<'T>

        /// Executes a scan synchronously and returns the results
        member ExecScan<'T>     : string -> IEnumerable<'T>

/// Extension methods for the DynamoDBContext class to be used in C#
[<Extension>]
[<AbstractClass>]
[<Sealed>]
type DynamoDBContextExt =
    /// Executes a query synchronously and returns the results
    [<Extension>]
    static member ExecQuery<'T>    : DynamoDBContext * string -> IEnumerable<'T>

    /// Executes a scan synchronously and returns the results
    [<Extension>]
    static member ExecScan<'T>     : DynamoDBContext * string -> IEnumerable<'T>