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
/// A database table/view column
/// </summary>
public class Column
{
    /// <summary>
    /// Data type of the column
    /// </summary>
    [JsonPropertyOrder(2)]
    public string DataType { get; set; }

    /// <summary>
    /// Default expression of the column
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(6)]
    public string Default { get; set; }

    /// <summary>
    /// Description of the column 
    /// </summary>
    [JsonPropertyOrder(3)]
    public string Description { get; set; }

    /// <summary>
    /// Is the column an array type?
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyOrder(5)]
    public bool IsArray { get; set; }

    /// <summary>
    /// Is the column nullable?
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(4)]
    public bool? IsNullable { get; set; }

    /// <summary>
    /// Name of the column
    /// </summary>
    [JsonPropertyOrder(1)]
    public string Name { get; set; }

    /// <summary>
    /// Optional list of semantically valid values 
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(7)]
    public List<ValidValue> ValidValues { get; set; } = new();
}