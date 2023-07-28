#region SqlDocs - Copyright (C) 2023 STÜBER SYSTEMS GmbH
/*    
 *    SqlDocs
 *    
 *    Copyright (C) 2023 STÜBER SYSTEMS GmbH
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
                new Option<DatabaseEngine>(new[] { "--dbengine", "-db" }, "Name of a supported database engine")
                {
                    IsRequired = true
                },
                new Option<string>(new[] { "--dbconnection", "-c" }, "ADO.NET database connection string")
                {
                    IsRequired = true
                },
                new Option<FileInfo>(new[] { "--dbschemafile", "-s" }, "Path to database schema JSON file")
                {
                    IsRequired = true
                }
            };

            command.SetHandler(CommandHandlers.BuildJson, 
                command.Options[0] as Option<DatabaseEngine>,
                command.Options[1] as Option<string>,
                command.Options[2] as Option<FileInfo>);

            return command;
        }

        public static Command BuildJsonMkDocs()
        {
            var command = new Command("build-json-mkdocs", "Builds a JSON file with a database schema and generates or updates a MkDocs project out of it")
            {
                new Option<DatabaseEngine>(new[] { "--dbengine", "-db" }, "Name of a supported database engine")
                {
                    IsRequired = true
                },
                new Option<string>(new[] { "--dbconnection", "-c" }, "ADO.NET database connection string")
                {
                    IsRequired = true
                },
                new Option<FileInfo>(new[] { "--dbschemafile", "-s" }, "Path to database schema JSON file")
                {
                    IsRequired = true
                },
                new Option<DirectoryInfo>(new[] { "--outputfolder", "-o" }, "Path to MkDocs project folder")
                {
                    IsRequired = true
                },
                new Option<string>(new[] { "--language", "-l" }, "Supported language code")
                {
                    IsRequired = true
                }
            };

            command.SetHandler(CommandHandlers.BuildJsonAndMkDocs,
                command.Options[0] as Option<DatabaseEngine>,
                command.Options[1] as Option<string>,
                command.Options[2] as Option<FileInfo>,
                command.Options[3] as Option<DirectoryInfo>,
                command.Options[4] as Option<string>);

            return command;
        }

        public static Command BuildMkDocs()
        {
            var command = new Command("build-mkdocs", "Loads a JSON file with a database schema and generates or updates a MkDocs project out of it")
            {
                new Option<DatabaseEngine>(new[] { "--dbengine", "-db" }, "Name of a supported database engine")
                {
                    IsRequired = true
                },
                new Option<FileInfo>(new[] { "--dbschemafile", "-s" }, "Path to database schema JSON file")
                {
                    IsRequired = true
                },
                new Option<DirectoryInfo>(new[] { "--outputfolder", "-o" }, "Path to MkDocs project folder")
                {
                    IsRequired = true
                },
                new Option<string>(new[] { "--language", "-l" }, "Supported language code")
                {
                    IsRequired = true
                }
            };

            command.SetHandler(CommandHandlers.BuildMkDocs,
                command.Options[0] as Option<DatabaseEngine>,
                command.Options[1] as Option<FileInfo>,
                command.Options[2] as Option<DirectoryInfo>,
                command.Options[3] as Option<string>);

            return command;
        }
    }
}
