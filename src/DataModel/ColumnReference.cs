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

using System.Text.Json.Serialization;

namespace SqlDocs.DataModel;

/// <summary>
/// A refence to a table column 
/// </summary>
public class ColumnReference
{
    /// <summary>
    /// Name of the column
    /// </summary>
    [JsonPropertyOrder(1)]
    public string Name { get; set; }

    /// <summary>
    /// Sorring order of the column reference
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(2)]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Sorting? Sorting { get; set; }
}