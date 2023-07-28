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
/// A database index
/// </summary>
public class Index : IKey
{
    /// <summary>
    /// List of column references
    /// </summary>
    [JsonPropertyOrder(3)]
    public List<ColumnReference> Columns { get; set; } = new();

    /// <summary>
    /// Description of the index
    /// </summary>
    [JsonPropertyOrder(2)]
    public string Description { get; set; }

    /// <summary>
    /// Is this a unique index?
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyOrder(4)]
    public bool? IsUnique { get; set; }

    /// <summary>
    /// Name of the index
    /// </summary>
    [JsonPropertyOrder(1)]
    public string Name { get; set; }
}