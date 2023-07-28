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
/// A database view
/// </summary>
public class View : IRelation
{
    /// <summary>
    /// List of columns
    /// </summary>
    [JsonPropertyOrder(3)]
    public List<Column> Columns { get; set; } = new();

    /// <summary>
    /// Description of the view 
    /// </summary>
    [JsonPropertyOrder(2)]
    public string Description { get; set; }

    /// <summary>
    /// Name of the view 
    /// </summary>
    [JsonPropertyOrder(1)]
    public string Name { get; set; }

    /// <summary>
    /// The SQL query
    /// </summary>
    [JsonPropertyOrder(4)]
    public string Source { get; set; }
}