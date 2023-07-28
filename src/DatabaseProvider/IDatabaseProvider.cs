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
using System.Threading;
using System.Threading.Tasks;

namespace SqlDocs.DatabaseProvider;

/// <summary>
/// For each supported database system, a class must be provided that implements this interface. 
/// </summary>
public interface IDatabaseProvider
{
    /// <summary>
    /// Gives back a database schema
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to cancel the operation</param>
    /// <returns>The schema for a database</returns>
    public Task<DbSchema> GetDbSchemaAsync(CancellationToken cancellationToken);
}