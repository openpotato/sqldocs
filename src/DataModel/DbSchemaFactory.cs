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

using SqlDocs.DatabaseProvider;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SqlDocs.DataModel;

/// <summary>
/// A factory of <see cref="DbSchema"/> instances
/// </summary>
public static class DbSchemaFactory
{
    /// <summary>
    /// Creates a new database schema object form a given database
    /// </summary>
    /// <param name="databaseProvider">A database provider</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to cancel the operation</param>
    /// <returns>The database schema object</returns>
    public static async Task<DbSchema> CreateDbSchemaAsync(IDatabaseProvider databaseProvider, CancellationToken cancellationToken)
    {
        return await databaseProvider.GetDbSchemaAsync(cancellationToken);
    }

    /// <summary>
    /// Loads a database schema object from a given JSON file and merges it with a new database schema object
    /// </summary>
    /// <param name="fileInfo">The JSON file</param>
    /// <param name="newDbSchema">A new database schema object</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to cancel the operation</param>
    /// <returns>The merged database schema object</returns>
    public static async Task<DbSchema> LoadAndMergeDbSchemaAsync(FileInfo fileInfo, DbSchema newDbSchema, CancellationToken cancellationToken)
    {
        var targetInfo = await LoadDbSchemaAsync(fileInfo, cancellationToken);
        Merge(targetInfo, newDbSchema);
        return targetInfo;
    }

    /// <summary>
    /// Loads a database schema object from a given JSON file
    /// </summary>
    /// <param name="fileInfo">The JSON file</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to cancel the operation</param>
    /// <returns>The loaded database schema object</returns>
    public static async Task<DbSchema> LoadDbSchemaAsync(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        using FileStream fileStream = fileInfo.OpenRead();
        return await JsonSerializer.DeserializeAsync<DbSchema>(fileStream, cancellationToken: cancellationToken);
    }

    private static void Merge(DbSchema targetDbSchema, DbSchema sourceDbSchema)
    {
        if (string.IsNullOrEmpty(targetDbSchema.Name)) targetDbSchema.Name = sourceDbSchema.Name;
        if (string.IsNullOrEmpty(targetDbSchema.Description)) targetDbSchema.Description = sourceDbSchema.Description;
        if (string.IsNullOrEmpty(targetDbSchema.Version)) targetDbSchema.Version = sourceDbSchema.Version;
        if (string.IsNullOrEmpty(targetDbSchema.DbmsName)) targetDbSchema.DbmsName = sourceDbSchema.DbmsName;
        if (string.IsNullOrEmpty(targetDbSchema.DbmsVersion)) targetDbSchema.DbmsVersion = sourceDbSchema.DbmsVersion;

        MergeSchemata(targetDbSchema, sourceDbSchema);
    }

    private static void MergeColumnReferences(IKey targetKey, IKey sourceKey)
    {
        if (targetKey.Columns.Count == 0)
        {
            targetKey.Columns.AddRange(sourceKey.Columns);
        }
        else
        {
            targetKey.Columns.RemoveAll(t => sourceKey.Columns.Find(s => s.Name == t.Name) == null);

            foreach (var sourceColumnReference in sourceKey.Columns)
            {
                var targetColumnReference = targetKey.Columns.Find(x => x.Name == sourceColumnReference.Name);
                if (targetColumnReference == null)
                {
                    targetKey.Columns.Add(sourceColumnReference);
                }
                else
                {
                    if (sourceColumnReference.Name != null) targetColumnReference.Name = sourceColumnReference.Name;
                    if (sourceColumnReference.Sorting != null) targetColumnReference.Sorting = sourceColumnReference.Sorting;
                }
            }
        }
    }

    private static void MergeColumns(IRelation targetTable, IRelation sourceTable)
    {
        if (targetTable.Columns.Count == 0)
        {
            targetTable.Columns.AddRange(sourceTable.Columns);
        }
        else
        {
            targetTable.Columns.RemoveAll(t => sourceTable.Columns.Find(s => s.Name == t.Name) == null);

            foreach (var sourceColumn in sourceTable.Columns)
            {
                var targetColumn = targetTable.Columns.Find(x => x.Name == sourceColumn.Name);
                if (targetColumn == null)
                {
                    targetTable.Columns.Add(sourceColumn);
                }
                else
                {
                    if (sourceColumn.Name != null) targetColumn.Name = sourceColumn.Name;
                    if (sourceColumn.Description != null) targetColumn.Description = sourceColumn.Description;
                    if (sourceColumn.DataType != null) targetColumn.DataType = sourceColumn.DataType;
                    if (sourceColumn.IsNullable != null) targetColumn.IsNullable = sourceColumn.IsNullable;
                    if (sourceColumn.Default != null) targetColumn.Default = sourceColumn.Default;

                    MergeValidValues(targetColumn, sourceColumn);
                }
            }
        }
    }

    private static void MergeForeignKeys(Table targetTable, Table sourceTable)
    {
        if (targetTable.ForeignKeys.Count == 0)
        {
            targetTable.ForeignKeys.AddRange(sourceTable.ForeignKeys);
        }
        else
        {
            targetTable.ForeignKeys.RemoveAll(t => sourceTable.ForeignKeys.Find(s => s.Name == t.Name) == null);

            foreach (var sourceForeignKey in sourceTable.ForeignKeys)
            {
                var targetForeignKey = targetTable.ForeignKeys.Find(x => x.Name == sourceForeignKey.Name);
                if (targetForeignKey == null)
                {
                    targetTable.ForeignKeys.Add(sourceForeignKey);
                }
                else
                {
                    if (sourceForeignKey.Name != null) targetForeignKey.Name = sourceForeignKey.Name;
                    if (sourceForeignKey.Description != null) targetForeignKey.Description = sourceForeignKey.Description;
                    if (sourceForeignKey.ForeignTableSchema != null) targetForeignKey.ForeignTableSchema = sourceForeignKey.ForeignTableSchema;
                    if (sourceForeignKey.ForeignTableName != null) targetForeignKey.ForeignTableName = sourceForeignKey.ForeignTableName;
                    if (sourceForeignKey.UpdateAction != null) targetForeignKey.UpdateAction = sourceForeignKey.UpdateAction;
                    if (sourceForeignKey.DeleteAction != null) targetForeignKey.DeleteAction = sourceForeignKey.DeleteAction;

                    MergeColumnReferences(targetForeignKey, sourceForeignKey);
                }
            }
        }
    }

    private static void MergeIndices(Table targetTable, Table sourceTable)
    {
        if (targetTable.Indices.Count == 0)
        {
            targetTable.Indices.AddRange(sourceTable.Indices);
        }
        else
        {
            targetTable.Indices.RemoveAll(t => sourceTable.Indices.Find(s => s.Name == t.Name) == null);

            foreach (var sourceIndex in sourceTable.Indices)
            {
                var targetIndex = targetTable.Indices.Find(x => x.Name == sourceIndex.Name);
                if (targetIndex == null)
                {
                    targetTable.Indices.Add(sourceIndex);
                }
                else
                {
                    if (sourceIndex.Name != null) targetIndex.Name = sourceIndex.Name;
                    if (sourceIndex.Description != null) targetIndex.Description = sourceIndex.Description;
                    if (sourceIndex.IsUnique != null) targetIndex.IsUnique = sourceIndex.IsUnique;

                    MergeColumnReferences(targetIndex, sourceIndex);
                }
            }
        }
    }

    private static void MergePrimaryKey(Table targetTable, Table sourceTable)
    {
        if (targetTable.PrimaryKey == null)
        {
            targetTable.PrimaryKey = sourceTable.PrimaryKey;
        }
        else
        {
            if (sourceTable.PrimaryKey == null)
            {
                targetTable.PrimaryKey = null;
            }
            else
            { 
                if (sourceTable.PrimaryKey.Name != null) targetTable.PrimaryKey.Name = sourceTable.Name;
                if (sourceTable.PrimaryKey.Description != null) targetTable.PrimaryKey.Description = sourceTable.Description;

                MergeColumnReferences(targetTable.PrimaryKey, sourceTable.PrimaryKey);
            }
        }
    }

    private static void MergeSchemata(DbSchema targetDbSchema, DbSchema sourceDbSchema)
    {
        if (targetDbSchema.Schemata.Count == 0)
        {
            targetDbSchema.Schemata.AddRange(sourceDbSchema.Schemata);
        }
        else
        {
            targetDbSchema.Schemata.RemoveAll(t => sourceDbSchema.Schemata.Find(s => s.Name == t.Name) == null);

            foreach (var sourceSchema in sourceDbSchema.Schemata)
            {
                var targetSchema = targetDbSchema.Schemata.Find(x => x.Name == sourceSchema.Name);
                if (targetSchema == null)
                {
                    targetDbSchema.Schemata.Add(sourceSchema);
                }
                else
                {
                    if (sourceSchema.Name != null) targetSchema.Name = sourceSchema.Name;
                    if (sourceSchema.Description != null) targetSchema.Description = sourceSchema.Description;

                    MergeTables(targetSchema, sourceSchema);
                    MergeViews(targetSchema, sourceSchema);
                }
            }
        }

        MergeTables(targetDbSchema, sourceDbSchema);
        MergeViews(targetDbSchema, sourceDbSchema);
    }

    private static void MergeTables(ISchema targetSchema, ISchema sourceSchema)
    {
        if (targetSchema.Tables.Count == 0)
        {
            targetSchema.Tables.AddRange(sourceSchema.Tables);
        }
        else
        {
            targetSchema.Tables.RemoveAll(t => sourceSchema.Tables.Find(s => s.Name == t.Name) == null);

            foreach (var sourceTable in sourceSchema.Tables)
            {
                var targetTable = targetSchema.Tables.Find(x => x.Name == sourceTable.Name);
                if (targetTable == null)
                {
                    targetSchema.Tables.Add(sourceTable);
                }
                else
                {
                    if (sourceTable.Name != null) targetTable.Name = sourceTable.Name;
                    if (sourceTable.Description != null) targetTable.Description = sourceTable.Description;

                    MergeColumns(targetTable, sourceTable);
                    MergePrimaryKey(targetTable, sourceTable);
                    MergeForeignKeys(targetTable, sourceTable);
                    MergeIndices(targetTable, sourceTable);
                }
            }
        }
    }

    private static void MergeValidValues(Column targetColumn, Column sourceColumn)
    {
        if (sourceColumn.ValidValues != null && sourceColumn.ValidValues.Count > 0)
        {
            if (targetColumn.ValidValues == null)
            {
                targetColumn.ValidValues = sourceColumn.ValidValues;
            }
            else if (targetColumn.ValidValues.Count == 0)
            {
                targetColumn.ValidValues.AddRange(sourceColumn.ValidValues);
            }
            else
            {
                targetColumn.ValidValues.RemoveAll(t => sourceColumn.ValidValues.Find(s => s.Value == t.Value) == null);

                foreach (var sourceValue in sourceColumn.ValidValues)
                {
                    var targetValue = targetColumn.ValidValues.Find(x => x.Value == sourceValue.Value);
                    if (targetValue == null)
                    {
                        targetColumn.ValidValues.Add(sourceValue);
                    }
                    else
                    {
                        if (sourceValue.Value != null) targetValue.Value = sourceValue.Value;
                        if (sourceValue.Description != null) targetValue.Description = sourceValue.Description;
                    }
                }
            }
        }
    }

    private static void MergeViews(ISchema targetSchema, ISchema sourceSchema)
    {
        if (targetSchema.Views.Count == 0)
        {
            targetSchema.Views.AddRange(sourceSchema.Views);
        }
        else
        {
            targetSchema.Views.RemoveAll(t => sourceSchema.Views.Find(s => s.Name == t.Name) == null);

            foreach (var sourceTable in sourceSchema.Views)
            {
                var targetTable = targetSchema.Views.Find(x => x.Name == sourceTable.Name);
                if (targetTable == null)
                {
                    targetSchema.Views.Add(sourceTable);
                }
                else
                {
                    if (sourceTable.Name != null) targetTable.Name = sourceTable.Name;
                    if (sourceTable.Description != null) targetTable.Description = sourceTable.Description;
                    if (sourceTable.Source != null) targetTable.Source = sourceTable.Source;

                    MergeColumns(targetTable, sourceTable);
                }
            }
        }
    }
}