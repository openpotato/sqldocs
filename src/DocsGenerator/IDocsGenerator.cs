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

using SqlDocs.DataModel;
using System.IO;
using System.Threading.Tasks;

namespace SqlDocs.DocsGenerator;

/// <summary>
/// For each supported static website generator, a class must be provided that implements this interface. 
/// </summary>
public interface IDocsGenerator
{
    /// <summary>
    /// Generates or updates a static website project from a given database schema.
    /// </summary>
    /// <param name="dbSchema">A database schema object</param>
    /// <param name="ouputDirectory">The directory of the static website project</param>
    /// <returns>The asynchronous operation</returns>
    public Task GenerateAsync(DbSchema dbSchema, DirectoryInfo ouputDirectory);
}