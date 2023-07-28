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
/// A database table
/// </summary>
public class Table : IRelation
{
    /// <summary>
    /// List of columns
    /// </summary>
    [JsonPropertyOrder(3)]
    public List<Column> Columns { get; set; } = new();

    /// <summary>
    /// Description of the table 
    /// </summary>
    [JsonPropertyOrder(2)]
    public string Description { get; set; }

    /// <summary>
    /// List of foreign keys
    /// </summary>
    [JsonPropertyOrder(5)]
    public List<ForeignKey> ForeignKeys { get; set; } = new();

    /// <summary>
    /// List of indices
    /// </summary>
    [JsonPropertyOrder(6)]
    public List<Index> Indices { get; set; } = new();

    /// <summary>
    /// Name of the table 
    /// </summary>
    [JsonPropertyOrder(1)]
    public string Name { get; set; }

    /// <summary>
    /// List of primary keys
    /// </summary>
    [JsonPropertyOrder(4)]
    public PrimaryKey PrimaryKey { get; set; }
}