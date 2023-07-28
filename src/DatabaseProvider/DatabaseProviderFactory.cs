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

namespace SqlDocs.DatabaseProvider;

/// <summary>
/// A static factory class for creating a supported database provider. 
/// </summary>
public static class DatabaseProviderFactory
{
    /// <summary>
    /// Creates a database provider
    /// </summary>
    /// <param name="dbEngine">The supported database engine</param>
    /// <param name="dbConnection">The ADO.NET database connection string</param>
    /// <returns>A new instance of a database provider</returns>
    /// <exception cref="NotSupportedDatabaseException"></exception>
    public static IDatabaseProvider CreateDatabaseProvider(DatabaseEngine dbEngine, string dbConnection) 
    { 
        switch (dbEngine)
        {
            case DatabaseEngine.Firebird:
                return new FirebirdProvider(dbConnection);
            case DatabaseEngine.Postgres:
                return new PostgresProvider(dbConnection);
            default:
                throw new NotSupportedDatabaseException(dbEngine);
        }
    }
}