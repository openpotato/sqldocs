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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace SqlDocs.DataModel;

/// <summary>
/// The structural schema of a database
/// </summary>
public class DbSchema : ISchema
{
    /// <summary>
    /// Database server engine (e.g. PostgreSQL)
    /// </summary>
    [JsonPropertyOrder(4)]
    public string DbmsName { get; set; }

    /// <summary>
    /// Database server version
    /// </summary>
    [JsonPropertyOrder(5)]
    public string DbmsVersion { get; set; }

    /// <summary>
    /// Description of the database schema
    /// </summary>
    [JsonPropertyOrder(3)]
    public string Description { get; set; }

    /// <summary>
    /// Name of the schema 
    /// </summary>
    [JsonPropertyOrder(1)]
    public string Name { get; set; }

    /// <summary>
    /// List of database schemata
    /// </summary>
    [JsonPropertyOrder(6)]
    public List<Schema> Schemata { get; set; } = new();

    /// <summary>
    /// List of tables (for databases without schema support)
    /// </summary>
    [JsonPropertyOrder(7)]
    public List<Table> Tables { get; set; } = new();

    /// <summary>
    /// Version of the database schema
    /// </summary>
    [JsonPropertyOrder(2)]
    public string Version { get; set; } = "0.0.1";

    /// <summary>
    /// List of views (for databases without schema support)
    /// </summary>
    [JsonPropertyOrder(8)]
    public List<View> Views { get; set; } = new();

    /// <summary>
    /// Does this schema contains any database objects?
    /// </summary>
    /// <returns>TRUE, if no database objects available</returns>
    public bool IsEmpty()
    {
        return Tables.Count == 0 && Views.Count == 0 && Schemata.Count == 0;
    }

    /// <summary>
    /// Save this document instance as JSON file
    /// </summary>
    /// <param name="fileInfo">file info</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public async Task SaveAsync(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            // Pretty printing
            WriteIndented = true,
            // Less strict encoding
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            // Ignore empty collections
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { (JsonTypeInfo type_info) =>
                    {
                        foreach (var property in type_info.Properties)
                        {
                            if (typeof(ICollection).IsAssignableFrom(property.PropertyType))
                            {
                                property.ShouldSerialize = (_, val) => val is ICollection collection && collection.Count > 0;
                            }
                        }
                    } 
                }
            },
        };

        using FileStream fileStream = fileInfo.Create();
        await JsonSerializer.SerializeAsync(fileStream, this, jsonOptions, cancellationToken);
    }
}