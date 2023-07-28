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

using System.CommandLine;
using System.Threading.Tasks;

namespace SqlDocs;

class Program
{
    public static async Task Main(string[] args)
    {
        // Build up command line api
        var rootCommand = new RootCommand(description: "Building nice looking documentions of relational database schemata")
        {
            CommandDefinitions.BuildJson(),
            CommandDefinitions.BuildMkDocs(),
            CommandDefinitions.BuildJsonMkDocs()
        };

        // Parse the incoming args and invoke the handler
        await rootCommand.InvokeAsync(args);
    }
}
