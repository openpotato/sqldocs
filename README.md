![GitHub](https://img.shields.io/github/license/openpotato/sqldocs)

# SQLDocs

## Introduction

SQLDocs is a cross-platform console application build with [.NET 7](https://dotnet.microsoft.com/) to create nice looking [MkDocs](https://www.mkdocs.org/) schema documentations for relational databases. Currently the following database systems are supported:

+ [Firebird](https://firebirdsql.org/)
+ [PostgreSQL](https://www.postgresql.org/)

SQLDocs first extracts the metadata of the desired database and stores it in a normalised JSON file. This JSON file can be manually extended with additional information (descriptions, valid data values, etc.). 

In a second step, the information from the JSON file is used to create or update a MkDocs project (with [Material for MkDocs](https://squidfunk.github.io/mkdocs-material/) as theme).

Both the JSON file and the MkDocs project can be versioned in a Git repository.

## Examples

If you would like to see what documentation created with SQLDocs looks like:

+ [Sample documentation for a Firebird database](https://openpotato.github.io/sqldocs.sample/firebird)
+ [Sample documentation for a PostgreSQL database](https://openpotato.github.io/sqldocs.sample/postgres)

## Documentation

Documentation is available in the [GitHub wiki](https://github.com/openpotato/sqldocs/wiki).

## To-do list

+ Support for additional DBMS (e.g. MySQL or MS SQL Server)
+ Support for additional table types (e.g. external tables, temporary tables)
+ Support for domains and custom types
+ Support for generators/sequences
+ Support for triggers
+ Support for stored procedures
+ (Maybe) support for alternative static-site generators (e.g. [Docusaurus](https://docusaurus.io/))

## Can I help?

Yes, that would be much appreciated. The best way to help is to post a response via the Issue Tracker and/or submit a Pull Request.
