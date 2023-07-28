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

using Enbrea.MdBuilder;
using NGettext;
using SqlDocs.DatabaseProvider;
using SqlDocs.DataModel;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SqlDocs.DocsGenerator;

/// <summary>
/// A generator for MkDocs projects
/// </summary>
public class MkDocsGenerator : IDocsGenerator
{
    private readonly DatabaseEngine _databaseEngine;
    private readonly ICatalog _catalog;

    /// <summary>
    /// Initializes a new instance of the <see cref="MkDocsGenerator"/> class.
    /// </summary>
    /// <param name="databaseEngine">The supported database engine</param>
    /// <param name="catalog">Translations of text templates</param>
    public MkDocsGenerator(DatabaseEngine databaseEngine, ICatalog catalog)
    {
        _databaseEngine = databaseEngine;
        _catalog = catalog;
    }

    /// <summary>
    /// Generates or updates a MKDocs project from a given database schema.
    /// </summary>
    /// <param name="dbSchema">A database schema object</param>
    /// <param name="ouputDirectory">The directory of the MkDocs project</param>
    /// <returns>The asynchronous operation</returns>
    public async Task GenerateAsync(DbSchema dbSchema, DirectoryInfo ouputDirectory)
    {
        await GenerateMkDocsProject(dbSchema, ouputDirectory);

        var currentDirectory = new DirectoryInfo(Path.Combine(ouputDirectory.FullName, "docs", "database"));

        if (currentDirectory.Exists)
        {
            foreach (var file in currentDirectory.EnumerateFiles())
            {
                file.Delete();
            }

            foreach (var directory in currentDirectory.EnumerateDirectories())
            {
                directory.Delete(true);
            }
        }
        else
        {
            currentDirectory.Create();
        }

        await GenerateFoldersAndFiles(dbSchema, currentDirectory);
    }

    private string GenerateColumnsListText(IList<ColumnReference> columnList)
    {
        var sb = new StringBuilder();

        foreach (var column in columnList)
        {
            if (sb.Length > 0)
            {
                sb.Append(", ");
            }
            sb.Append(column.Name);
        }

        return sb.ToString();
    }

    private MarkdownParagraph GenerateDataTypeAndConstraintsParagraph(Column column)
    {
        var mdParagraph = new MarkdownParagraph();

        if (Links.TryGetDataTypeDocLink(_databaseEngine, column.DataType, out var url))
        {
            mdParagraph.AppendLink(x => x.AppendCodeSpan(column.DataType), url);
        }
        else
        {
            mdParagraph.AppendCodeSpan(column.DataType);
        }

        if (column.IsNullable == false)
        {
            mdParagraph.AppendText(" · ");
            mdParagraph.AppendCodeSpan("NOT NULL");
        }

        if (column.Default != null)
        {
            mdParagraph.AppendText(" · ");
            mdParagraph.AppendCodeSpan($"{column.Default}");
        }

        return mdParagraph;
    }

    private async Task GenerateFoldersAndFiles(DbSchema dbSchema, DirectoryInfo ouputDirectory)
    {
        if (dbSchema.Schemata.Count == 0)
        {
            var currentDirectory = ouputDirectory;

            await GenerateTableFoldersAndFiles(null, dbSchema.Tables, currentDirectory);
            await GenerateViewFoldersAndFiles(null, dbSchema.Views, currentDirectory);
        }
        else
        {
            foreach (var schema in dbSchema.Schemata)
            {
                if (!schema.IsEmpty())
                {
                    var currentDirectory = new DirectoryInfo(Path.Combine(ouputDirectory.FullName, schema.Name));

                    if (!currentDirectory.Exists)
                    {
                        currentDirectory.Create();
                    }

                    await GenerateSchemaMarkdownFile(schema, currentDirectory);
                    await GenerateTableFoldersAndFiles(schema, schema.Tables, currentDirectory);
                    await GenerateViewFoldersAndFiles(schema, schema.Views, currentDirectory);
                    await GeneratePagesFile(schema, currentDirectory);
                }
            }
        }
        await GeneratePagesFile(dbSchema, ouputDirectory);
    }

    private string GenerateForeignKeyActionText(ForeignKeyAction? foreignKeyAction)
    {
        return foreignKeyAction switch
        {
            ForeignKeyAction.NoAction => "NO ACTION",
            ForeignKeyAction.Restrict => "RESTRICT",
            ForeignKeyAction.Cascade => "CASCADE",
            ForeignKeyAction.SetNull => "SET NULL",
            ForeignKeyAction.SetDefault => "SET DEFAULT",
            _ => "?",
        };
    }

    private MarkdownParagraph GenerateForeignKeyConstraintParagraph(ForeignKey foreignKey)
    {
        var mdParagraph = new MarkdownParagraph();

        mdParagraph.AppendCodeSpan(GenerateColumnsListText(foreignKey.Columns));
        mdParagraph.AppendText(" » ");

        if (string.IsNullOrEmpty(foreignKey.ForeignTableSchema))
        {
            mdParagraph.AppendLink(x =>
            {
                x.AppendCodeSpan($"{foreignKey.ForeignTableName} ({GenerateColumnsListText(foreignKey.ForeignTableColumns)})");
            }, $"../../tables/{foreignKey.ForeignTableName.ToLowerInvariant()}");
        }
        else
        {
            mdParagraph.AppendLink(x =>
            {
                x.AppendCodeSpan($"{foreignKey.ForeignTableSchema}.{foreignKey.ForeignTableName} ({GenerateColumnsListText(foreignKey.ForeignTableColumns)})");
            }, $"../../{foreignKey.ForeignTableSchema.ToLowerInvariant()}/tables/{foreignKey.ForeignTableName.ToLowerInvariant()}");
        }

        mdParagraph.AppendText(" · ");
        mdParagraph.AppendCodeSpan($"ON UPDATE {GenerateForeignKeyActionText(foreignKey.UpdateAction)}");
        mdParagraph.AppendText(" · ");
        mdParagraph.AppendCodeSpan($"ON DELETE {GenerateForeignKeyActionText(foreignKey.DeleteAction)}");

        return mdParagraph;
    }

    private string GenerateHomeMarkdownContent(DbSchema dbSchema)
    {
        var mdBuilder = new MarkdownBuilder();

        mdBuilder.AppendHeading(1, _catalog.GetString("Introduction"));
        mdBuilder.AppendParagraph(dbSchema.Description);

        return mdBuilder.ToString();
    }

    private async Task GenerateHomeMarkdownFile(DbSchema dbSchema, DirectoryInfo ouputDirectory)
    {
        var currentFile = new FileInfo(Path.Combine(ouputDirectory.FullName, $"index.md"));

        using var contentWriter = currentFile.CreateText();

        await contentWriter.WriteLineAsync(GenerateHomeMarkdownContent(dbSchema));
    }

    private async Task GenerateMkDocsProject(DbSchema dbSchema, DirectoryInfo ouputDirectory)
    {
        if (!ouputDirectory.Exists)
        {
            ouputDirectory.Create();
        }

        var currentFile = new FileInfo(Path.Combine(ouputDirectory.FullName, "mkdocs.yml"));

        if (!currentFile.Exists)
        {
            using var contentWriter = currentFile.CreateText();

            await contentWriter.WriteLineAsync($"site_name: {dbSchema.Name ?? "SqlDocs"}");
            await contentWriter.WriteLineAsync();
            await contentWriter.WriteLineAsync("theme:");
            await contentWriter.WriteLineAsync("   name: material");
            await contentWriter.WriteLineAsync("   language: de");
            await contentWriter.WriteLineAsync("   features:");
            await contentWriter.WriteLineAsync("   - navigation.footer");
            await contentWriter.WriteLineAsync("   - navigation.instant");
            await contentWriter.WriteLineAsync("   - navigation.sections");
            await contentWriter.WriteLineAsync("   - search.highlight");
            await contentWriter.WriteLineAsync("   static_templates:");
            await contentWriter.WriteLineAsync("   - 404.html");
            await contentWriter.WriteLineAsync();
            await contentWriter.WriteLineAsync("plugins:");
            await contentWriter.WriteLineAsync("   - search");
            await contentWriter.WriteLineAsync("   - awesome-pages");
            await contentWriter.WriteLineAsync();
            await contentWriter.WriteLineAsync("markdown_extensions:");
            await contentWriter.WriteLineAsync("   - admonition");
            await contentWriter.WriteLineAsync("   - attr_list");
            await contentWriter.WriteLineAsync("   - def_list");

            var currentDirectory = new DirectoryInfo(Path.Combine(ouputDirectory.FullName, "docs"));

            if (!currentDirectory.Exists)
            {
                currentDirectory.Create();
            }

            await GenerateHomeMarkdownFile(dbSchema, currentDirectory);
            await GeneratePagesFile(currentDirectory);
        }
    }

    private async Task GeneratePagesFile(DirectoryInfo ouputDirectory)
    {
        var currentFile = new FileInfo(Path.Combine(ouputDirectory.FullName, ".pages"));

        using var contentWriter = currentFile.CreateText();

        await contentWriter.WriteLineAsync("nav:");
        await contentWriter.WriteLineAsync($"   - {_catalog.GetString("Introduction")}: index.md");
        await contentWriter.WriteLineAsync($"   - {_catalog.GetString("Database")}: database");
    }

    private async Task GeneratePagesFile(DbSchema dbSchema, DirectoryInfo ouputDirectory)
    {
        var currentFile = new FileInfo(Path.Combine(ouputDirectory.FullName, ".pages"));

        using var contentWriter = currentFile.CreateText();

        await contentWriter.WriteLineAsync("nav:");

        if (dbSchema.Schemata.Count == 0)
        {
            if (dbSchema.Tables.Count > 0) await contentWriter.WriteLineAsync($"   - {_catalog.GetString("Tables")}: tables");
            if (dbSchema.Views.Count > 0) await contentWriter.WriteLineAsync($"   - {_catalog.GetString("Views")}: views");
        }
        else
        {
            foreach (var schema in dbSchema.Schemata)
            {
                if (!schema.IsEmpty())
                {
                    await contentWriter.WriteLineAsync($"   - {schema.Name}: {schema.Name.ToLowerInvariant()}");
                }
            }
        }
    }

    private async Task GeneratePagesFile(Schema schema, DirectoryInfo ouputDirectory)
    {
        var currentFile = new FileInfo(Path.Combine(ouputDirectory.FullName, ".pages"));

        using var contentWriter = currentFile.CreateText();

        await contentWriter.WriteLineAsync("nav:");
        await contentWriter.WriteLineAsync($"   - {_catalog.GetString("Schema")}: schema.md");

        if (schema.Tables.Count > 0) await contentWriter.WriteLineAsync($"   - {_catalog.GetString("Tables")}: tables");
        if (schema.Views.Count > 0) await contentWriter.WriteLineAsync($"   - {_catalog.GetString("Views")}: views");
    }

    private async Task GeneratePagesFile(IList<Table> tableList, DirectoryInfo ouputDirectory)
    {
        if (tableList.Count > 0)
        {
            var currentFile = new FileInfo(Path.Combine(ouputDirectory.FullName, ".pages"));

            using var contentWriter = currentFile.CreateText();

            await contentWriter.WriteLineAsync("nav:");

            foreach (var table in tableList)
            {
                await contentWriter.WriteLineAsync($"   - {table.Name}: {table.Name.ToLowerInvariant()}.md");
            }
        }
    }

    private async Task GeneratePagesFile(IList<View> viewList, DirectoryInfo ouputDirectory)
    {
        if (viewList.Count > 0)
        {
            var currentFile = new FileInfo(Path.Combine(ouputDirectory.FullName, ".pages"));

            using var contentWriter = currentFile.CreateText();

            await contentWriter.WriteLineAsync("nav:");

            foreach (var view in viewList)
            {
                await contentWriter.WriteLineAsync($"   - {view.Name}: {view.Name.ToLowerInvariant()}.md");
            }
        }
    }

    private string GenerateSchemaLink(string schemaName)
    {
        var mdBuilder = new MarkdownBuilder();

        mdBuilder.AppendParagraph(x => x.AppendLink(schemaName, $"../../schema"));

        return mdBuilder.ToString();
    }
    
    private string GenerateSchemaMarkdownContent(Schema schema)
    {
        var mdBuilder = new MarkdownBuilder();

        // Introduction
        mdBuilder.AppendRawHeading(1, _catalog.GetString("Schema", GenerateStrong(schema.Name)));
        mdBuilder.AppendDescription(null, schema.Description);

        // Tables
        if (schema.Tables.Count > 0)
        {
            mdBuilder.AppendRawHeading(2, _catalog.GetString("Tables"));
            mdBuilder.AppendRawParagraph(_catalog.GetPluralString(
                "This schema contains one table.",
                "This schema contains {0} tables.", schema.Tables.Count, schema.Tables.Count));

            mdBuilder.AppendUnorderedList(x =>
            {
                foreach (var table in schema.Tables)
                {
                    x.Append(x =>
                    {
                        x.AppendParagraph(x => x.AppendLink(table.Name, $"tables/{table.Name.ToLowerInvariant()}.md"));
                    });
                }
            });
        }

        // Views
        if (schema.Views.Count > 0)
        {
            mdBuilder.AppendRawHeading(2, _catalog.GetString("Views"));
            mdBuilder.AppendRawParagraph(_catalog.GetPluralString(
                "This schema contains one view.",
                "This schema contains {0} views.", schema.Views.Count, schema.Views.Count));

            mdBuilder.AppendUnorderedList(x =>
            {
                foreach (var view in schema.Views)
                {
                    x.Append(x =>
                    {
                        x.AppendParagraph(x => x.AppendLink(view.Name, $"views/{view.Name.ToLowerInvariant()}.md"));
                    });
                }
            });
        }

        return mdBuilder.ToString();
    }

    private async Task GenerateSchemaMarkdownFile(Schema schema, DirectoryInfo ouputDirectory)
    {
        var currentFile = new FileInfo(Path.Combine(ouputDirectory.FullName, $"schema.md"));

        using var contentWriter = currentFile.CreateText();

        await contentWriter.WriteLineAsync(GenerateSchemaMarkdownContent(schema));
    }

    private string GenerateStrong(string name)
    {
        var mdBuilder = new MarkdownBuilder();

        mdBuilder.AppendParagraph(x => x.AppendStrong(name));

        return mdBuilder.ToString();
    }

    private async Task GenerateTableFoldersAndFiles(Schema schema, IList<Table> tableList, DirectoryInfo ouputDirectory)
    {
        if (tableList.Count > 0)
        {
            var currentDirectory = new DirectoryInfo(Path.Combine(ouputDirectory.FullName, "tables"));

            if (!currentDirectory.Exists)
            {
                currentDirectory.Create();
            }

            foreach (var table in tableList)
            {
                await GenerateTableMarkdownFile(schema, table, currentDirectory);
            }

            await GeneratePagesFile(tableList, currentDirectory);
        }
    }

    private string GenerateTableMarkdownContent(Schema schema, Table table)
    {
        var mdBuilder = new MarkdownBuilder();

        // Introduction
        mdBuilder.AppendRawHeading(1, _catalog.GetString("Table {0}", GenerateStrong(table.Name)));
        mdBuilder.AppendDescription("../", table.Description);

        // Table schema
        if (schema != null)
        {
            mdBuilder.AppendHeading(2, _catalog.GetString("Schema"));
            mdBuilder.AppendRawParagraph(_catalog.GetString("This table belongs to schema {0}.", GenerateSchemaLink(schema.Name)));
        }

        // Table columns
        mdBuilder.AppendRawHeading(2, _catalog.GetString("Columns"));
        mdBuilder.AppendRawParagraph(_catalog.GetPluralString(
            "This table contains one column.",
            "This table contains {0} columns.", table.Columns.Count, table.Columns.Count));

        mdBuilder.AppendDefinitionList(x =>
        {
            foreach (var column in table.Columns)
            {
                x.Append(x =>
                {
                    x.AssignTitle(x =>
                    {
                        x.AppendStrong(x => x.AppendCodeSpan(column.Name));
                    });
                    x.Append(GenerateDataTypeAndConstraintsParagraph(column));
                    x.AppendDescription("../", column.Description);

                    if (column.ValidValues.Count > 0)
                    {
                        x.Append(GenerateValidValuesTable(column)); 
                    }
                });
            };
        });

        // Primary key
        if (table.PrimaryKey != null)
        {
            mdBuilder.AppendRawHeading(2, _catalog.GetString("Primary key"));
            mdBuilder.AppendRawParagraph(_catalog.GetString("This table has a primary key."));

            mdBuilder.AppendDefinitionList(x =>
            {
                x.Append(x =>
                {
                    x.AssignTitle(x =>
                    {
                        x.AppendStrong(x => x.AppendCodeSpan(table.PrimaryKey.Name));
                    });
                    x.AppendParagraph(x => x.AppendCodeSpan(GenerateColumnsListText(table.PrimaryKey.Columns)));
                    x.AppendDescription("../", table.PrimaryKey.Description);
                });
            });
        }

        // Foreign keys
        if (table.ForeignKeys.Count > 0)
        {
            mdBuilder.AppendRawHeading(2, _catalog.GetString("Foreign keys"));
            mdBuilder.AppendRawParagraph(_catalog.GetPluralString(
                "This table has one foreign key.",
                "This table has one foreign key.", table.ForeignKeys.Count, table.ForeignKeys.Count));

            mdBuilder.AppendDefinitionList(x =>
            {
                foreach (var foreignKey in table.ForeignKeys)
                {
                    x.Append(x =>
                    {
                        x.AssignTitle(x =>
                        {
                            x.AppendStrong(x => x.AppendCodeSpan(foreignKey.Name));
                        });
                        x.Append(GenerateForeignKeyConstraintParagraph(foreignKey));
                        x.AppendDescription("../", foreignKey.Description);
                    });
                };
            });
        }

        // Indices
        if (table.Indices.Count > 0)
        {
            mdBuilder.AppendRawHeading(2, _catalog.GetString("Indices"));
            mdBuilder.AppendRawParagraph(_catalog.GetPluralString(
                "This table has one index.",
                "This table has {0} indices.", table.Indices.Count, table.Indices.Count));

            mdBuilder.AppendDefinitionList(x =>
            {
                foreach (var index in table.Indices)
                {
                    x.Append(x =>
                    {
                        x.AssignTitle(x =>
                        {
                            x.AppendStrong(x => x.AppendCodeSpan(index.Name));
                        });
                        x.AppendParagraph(x => x.AppendCodeSpan(GenerateColumnsListText(index.Columns)));
                        x.AppendDescription("../", index.Description);
                    });
                };
            });
        }

        return mdBuilder.ToString();
    }

    private async Task GenerateTableMarkdownFile(Schema schema, Table table, DirectoryInfo ouputDirectory)
    {
        var currentFile = new FileInfo(Path.Combine(ouputDirectory.FullName, $"{table.Name.ToLowerInvariant()}.md"));

        using var contentWriter = currentFile.CreateText();

        await contentWriter.WriteLineAsync(GenerateTableMarkdownContent(schema, table));
    }

    private async Task GenerateTableMarkdownFile(Schema schema, View view, DirectoryInfo ouputDirectory)
    {
        var currentFile = new FileInfo(Path.Combine(ouputDirectory.FullName, $"{view.Name.ToLowerInvariant()}.md"));

        using var contentWriter = currentFile.CreateText();

        await contentWriter.WriteLineAsync(GenerateViewMarkdownContent(schema, view));
    }

    private MarkdownTable GenerateValidValuesTable(Column column)
    {
        var mdTable = new MarkdownTable();

        mdTable.AssignHeader(x =>
        {
            x.AppendRawColumn(_catalog.GetString("Value"));
            x.AppendRawColumn(_catalog.GetString("Description"));
        });

        foreach (var validValue in column.ValidValues)
        {
            mdTable.AppendRow(x =>
            {
                x.Append(validValue.Value);
                x.AppendDescription("../", validValue.Description);
            });
        }

        return mdTable;
    }
    private async Task GenerateViewFoldersAndFiles(Schema schema, IList<View> viewList, DirectoryInfo ouputDirectory)
    {
        if (viewList.Count > 0)
        {
            var currentDirectory = new DirectoryInfo(Path.Combine(ouputDirectory.FullName, "views"));

            if (!currentDirectory.Exists)
            {
                currentDirectory.Create();
            }

            foreach (var view in viewList)
            {
                await GenerateTableMarkdownFile(schema, view, currentDirectory);
            }

            await GeneratePagesFile(viewList, currentDirectory);
        }
    }

    private string GenerateViewMarkdownContent(Schema schema, View view)
    {
        var mdBuilder = new MarkdownBuilder();

        // Introduction
        mdBuilder.AppendRawHeading(1, _catalog.GetString("View {0}", GenerateStrong(view.Name)));
        mdBuilder.AppendDescription("../", view.Description);

        // View schema
        if (schema != null)
        {
            mdBuilder.AppendRawHeading(2, _catalog.GetString("Schema"));
            mdBuilder.AppendRawParagraph(_catalog.GetString(
                "This view belongs to schema {0}.", GenerateSchemaLink(schema.Name)));
        }

        // View source
        if (!string.IsNullOrEmpty(view.Source))
        {
            mdBuilder.AppendRawHeading(2, _catalog.GetString("Query"));
            mdBuilder.AppendAdmonition(MarkdownAdmonitionType.Collapsed, "info", _catalog.GetString("SQL"), x =>
            {
                x.AppendFencedCode("sql", view.Source);
            }); 
        }

        // View columns
        mdBuilder.AppendRawHeading(2, _catalog.GetString("Columns"));
        mdBuilder.AppendRawParagraph(_catalog.GetPluralString(
            "This view contains one column.", 
            "This view contains {0} columns.", view.Columns.Count, view.Columns.Count));

        mdBuilder.AppendDefinitionList(x =>
        {
            foreach (var column in view.Columns)
            {
                x.Append(x =>
                {
                    x.AssignTitle(x =>
                    {
                        x.AppendStrong(x => x.AppendCodeSpan(column.Name));
                    });
                    
                    x.Append(GenerateDataTypeAndConstraintsParagraph(column));
                    
                    x.AppendDescription("../", column.Description);

                    if (column.ValidValues.Count > 0)
                    {
                        x.Append(GenerateValidValuesTable(column));
                    }
                });
            };
        });

        return mdBuilder.ToString();
    }
}
