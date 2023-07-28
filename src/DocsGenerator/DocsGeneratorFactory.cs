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

using NGettext;
using SqlDocs.DatabaseProvider;

namespace SqlDocs.DocsGenerator;

/// <summary>
/// A static factory class for creating a supported docs generator
/// </summary>
public static class DocsGeneratorFactory
{
    /// <summary>
    /// Creates a MkDocs generator
    /// </summary>
    /// <param name="databaseEngine">The database engine</param>
    /// <param name="catalog">Text translation catalog</param>
    /// <returns>New instance of the MkDocs generator</returns>
    public static IDocsGenerator CreateMkDocsGenerator(DatabaseEngine databaseEngine, ICatalog catalog)
    {
        return new MkDocsGenerator(databaseEngine, catalog);
    }
}