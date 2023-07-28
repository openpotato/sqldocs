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

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SqlDocs.DataModel;

/// <summary>
/// A database schema grouping tables and views 
/// </summary>
public class Schema : ISchema
{
    /// <summary>
    /// Description of the schema 
    /// </summary>
    [JsonPropertyOrder(2)]
    public string Description { get; set; }

    /// <summary>
    /// Name of the schema
    /// </summary>
    [JsonPropertyOrder(1)]
    public string Name { get; set; }

    /// <summary>
    /// List of schema tables
    /// </summary>
    [JsonPropertyOrder(3)]
    public List<Table> Tables { get; set; } = new();

    /// <summary>
    /// List of schema tables
    /// </summary>
    [JsonPropertyOrder(4)]
    public List<View> Views { get; set; } = new();

    /// <summary>
    /// Does this schema contains any database objects?
    /// </summary>
    /// <returns>TRUE, if no database objects available</returns>
    public bool IsEmpty()
    {
        return Tables.Count == 0 && Views.Count == 0;
    }
}