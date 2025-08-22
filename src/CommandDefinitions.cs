#region SqlDocs - Copyright (C) STÜBER SYSTEMS GmbH
/*    
 *    SqlDocs
 *    
 *    Copyright (C) STÜBER SYSTEMS GmbH
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU Affero General Public License, version 3,
 *    as published by the Free Software Foundation.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *    GNU Affero General Public License for more details.
 *
 *    You should have received a copy of the GNU Affero General Public License
 *    along with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 */
#endregion

using SqlDocs.DatabaseProvider;
using System.CommandLine;
using System.IO;

namespace SqlDocs
{
    public static class CommandDefinitions
    {
        public static Command BuildJson()
        {
            var command = new Command("build-json", "Builds a JSON file with a database schema")
            {
                new Option<DatabaseEngine>("--dbengine", "-db")
                {
                    Description = "Name of a supported database engine",
                    Required = true
                },
                new Option<string>("--dbconnection", "-c")
                {
                    Description = "ADO.NET database connection string",
                    Required = true
                },
                new Option<FileInfo>("--dbschemafile", "-s")
                {
                    Description = "Path to database schema JSON file",
                    Required = true
                }
            };

            command.SetAction(parseResult => CommandHandlers.BuildJson(
                parseResult.GetValue(command.Options[0] as Option<DatabaseEngine>),
                parseResult.GetValue(command.Options[1] as Option<string>),
                parseResult.GetValue(command.Options[2] as Option<FileInfo>))
            );

            return command;
        }

        public static Command BuildJsonMkDocs()
        {
            var command = new Command("build-json-mkdocs", "Builds a JSON file with a database schema and generates or updates a MkDocs project out of it")
            {
                new Option<DatabaseEngine>("--dbengine", "-db")
                {
                    Description = "Name of a supported database engine",
                    Required = true
                },
                new Option<string>("--dbconnection", "-c")
                {
                    Description = "ADO.NET database connection string",
                    Required = true
                },
                new Option<FileInfo>("--dbschemafile", "-s")
                {
                    Description = "Path to database schema JSON file",
                    Required = true
                },
                new Option<DirectoryInfo>("--outputfolder", "-o")
                {
                    Description = "Path to MkDocs project folder",
                    Required = true
                },
                new Option<string>("--language", "-l")
                {
                    Description = "Supported language code",
                    Required = true
                }
            };

            command.SetAction(parseResult => CommandHandlers.BuildJsonAndMkDocs(
                parseResult.GetValue(command.Options[0] as Option<DatabaseEngine>),
                parseResult.GetValue(command.Options[1] as Option<string>),
                parseResult.GetValue(command.Options[2] as Option<FileInfo>),
                parseResult.GetValue(command.Options[3] as Option<DirectoryInfo>),
                parseResult.GetValue(command.Options[4] as Option<string>))
            );

            return command;
        }

        public static Command BuildMkDocs()
        {
            var command = new Command("build-mkdocs", "Loads a JSON file with a database schema and generates or updates a MkDocs project out of it")
            {
                new Option<DatabaseEngine>("--dbengine", "-db")
                {
                    Description = "Name of a supported database engine",
                    Required = true
                },
                new Option<FileInfo>("--dbschemafile", "-s")
                {
                    Description = "Path to database schema JSON file",
                    Required = true
                },
                new Option<DirectoryInfo>("--outputfolder", "-o")
                {
                    Description = "Path to MkDocs project folder",
                    Required = true
                },
                new Option<string>("--language", "-l")
                {
                    Description = "Supported language code",
                    Required = true
                }
            };

            command.SetAction(parseResult => CommandHandlers.BuildMkDocs(
                parseResult.GetValue(command.Options[0] as Option<DatabaseEngine>),
                parseResult.GetValue(command.Options[1] as Option<FileInfo>),
                parseResult.GetValue(command.Options[2] as Option<DirectoryInfo>),
                parseResult.GetValue(command.Options[3] as Option<string>))
            );

            return command;
        }
    }
}
