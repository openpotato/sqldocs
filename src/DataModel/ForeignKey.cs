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
/// A database foreign key constraint
/// </summary>
public class ForeignKey : IKey
{
    /// <summary>
    /// List of column references
    /// </summary>
    [JsonPropertyOrder(3)]
    public List<ColumnReference> Columns { get; set; } = new();

    /// <summary>
    /// Description of the key
    /// </summary>
    [JsonPropertyOrder(2)]
    public string Description { get; set; }

    /// <summary>
    /// Name of the key
    /// </summary>
    [JsonPropertyOrder(1)]
    public string Name { get; set; }

    /// <summary>
    /// List of reference columns of the foreign table
    /// </summary>
    [JsonPropertyOrder(6)]
    public List<ColumnReference> ForeignTableColumns { get; set; } = new();

    /// <summary>
    /// Name of the foreign table 
    /// </summary>
    [JsonPropertyOrder(5)]
    public string ForeignTableName { get; set; }

    /// <summary>
    /// Name of the foreign table schema
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(4)]
    public string ForeignTableSchema { get; set; }
    
    /// <summary>
    /// Foreign key delete action
    /// </summary>
    [JsonPropertyOrder(8)]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ForeignKeyAction? UpdateAction { get; set; }

    /// <summary>
    /// Foreign key update action
    /// </summary>
    [JsonPropertyOrder(7)]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ForeignKeyAction? DeleteAction { get; set; }
}