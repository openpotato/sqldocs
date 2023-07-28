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

using Npgsql;
using SqlDocs.DataModel;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SqlDocs.DatabaseProvider;

/// <summary>
/// A database provider for PostgreSQL
/// </summary>
public class PostgresProvider : IDatabaseProvider
{
    private readonly string _connectionString;

    public PostgresProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<DbSchema> GetDbSchemaAsync(CancellationToken cancellationToken)
    {
        using DbDataSource datasource = NpgsqlDataSource.Create(_connectionString);
        using DbConnection connection = datasource.CreateConnection();

        connection.Open();

        var dbSchema = new DbSchema
        {
            Name = "SqlDocs",
            Description = connection.Database,
            DbmsName = "PostgreSQL",
            DbmsVersion = connection.ServerVersion,
        };

        await ReadSchemataAsync(connection, dbSchema, cancellationToken);

        return dbSchema;
    }

    private async Task ReadColumnsAsync(DbConnection connection, Schema schema, IRelation relation, CancellationToken cancellationToken)
    {
        using DbCommand cmd = connection.CreateCommand();

        cmd.CommandText =
            """
            SELECT
              col.attname AS name,
              format_type(col.atttypid, col.atttypmod) AS data_type,
              col.attnotnull AS not_null,
              col.attndims > 0 AS is_array,
              pg_get_expr(col_def.adbin, col_def.adrelid) AS default
            FROM
              pg_attribute col
            INNER JOIN
              pg_class tbl ON tbl.oid = col.attrelid
            INNER JOIN
              pg_namespace sch ON sch.oid = tbl.relnamespace
            LEFT JOIN
              pg_attrdef col_def
            ON  
              col_def.adrelid = col.attrelid AND col_def.adnum = col.attnum AND col.atthasdef = TRUE
            WHERE
              col.attnum > 0 AND NOT col.attisdropped AND 
              sch.nspname = @schema AND 
              tbl.relname = @table
            ORDER BY
              col.attnum ASC
            """;

        cmd.CommandType = CommandType.Text;
        cmd.AddParameter("schema", schema.Name);
        cmd.AddParameter("table", relation.Name);

        using (DbDataReader rdr = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await rdr.ReadAsync(cancellationToken))
            {
                relation.Columns.Add(new Column()
                {
                    Name = rdr.GetString("name"),
                    DataType = rdr.GetString("data_type"),
                    IsNullable = rdr.GetBoolean("not_null") == false,
                    IsArray = rdr.GetBoolean("is_array") == true,
                    Default = rdr.IsDBNull("default") ? null : rdr.GetString("default"),
                });
            }
        }
    }

    private async Task ReadForeignKeysAsync(DbConnection connection, Schema schema, Table table, CancellationToken cancellationToken)
    {
        using DbCommand cmd = connection.CreateCommand();

        cmd.CommandText =
            """
            SELECT
              c.conname AS name,
              c.contype AS type,
              ARRAY_AGG(col.attname ORDER BY u.attposition) AS table_columns,
              f_sch.nspname AS foreign_table_schema,
              f_tbl.relname AS foreign_table_name,
              ARRAY_AGG(f_col.attname ORDER BY f_u.attposition) AS foreign_table_columns,
              CASE c.confupdtype
                WHEN 'a' THEN 0
                WHEN 'r' THEN 1
                WHEN 'c' THEN 2
                WHEN 'n' THEN 3
                WHEN 'd' THEN 4
                ELSE NULL
              END AS update_action,
              CASE c.confdeltype
                WHEN 'a' THEN 0
                WHEN 'r' THEN 1
                WHEN 'c' THEN 2
                WHEN 'n' THEN 3
                WHEN 'd' THEN 4
                ELSE NULL
              END AS delete_action
            FROM
              pg_constraint c
            LEFT JOIN
              LATERAL UNNEST(c.conkey) WITH ORDINALITY AS u(attnum, attposition) ON TRUE
            LEFT JOIN
              LATERAL UNNEST(c.confkey) WITH ORDINALITY AS f_u(attnum, attposition) ON f_u.attposition = u.attposition
            JOIN
              pg_class tbl ON tbl.oid = c.conrelid
            JOIN
              pg_namespace sch ON sch.oid = tbl.relnamespace
            LEFT JOIN
              pg_attribute col ON col.attrelid = tbl.oid AND col.attnum = u.attnum
            LEFT JOIN
              pg_class f_tbl ON f_tbl.oid = c.confrelid
            LEFT JOIN
              pg_namespace f_sch ON f_sch.oid = f_tbl.relnamespace
            LEFT JOIN
              pg_attribute f_col ON f_col.attrelid = f_tbl.oid AND f_col.attnum = f_u.attnum
            WHERE
              c.contype = 'f' AND
              sch.nspname = @schema AND
              tbl.relname = @table
            GROUP BY
              name,
              type,
              foreign_table_schema,
              foreign_table_name,
              update_action,
              delete_action 
            ORDER BY
              type, name
            """;

        cmd.CommandType = CommandType.Text;
        cmd.AddParameter("schema", schema.Name);
        cmd.AddParameter("table", table.Name);

        using (DbDataReader rdr = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await rdr.ReadAsync(cancellationToken))
            {
                var newForeignKey = new ForeignKey()
                {
                    Name = rdr.GetString("name"),
                    ForeignTableName = rdr.GetString("foreign_table_name"),
                    ForeignTableSchema = rdr.GetString("foreign_table_schema"),
                    DeleteAction = rdr.IsDBNull("delete_action") ? null : (ForeignKeyAction)rdr.GetByte("delete_action"),
                    UpdateAction = rdr.IsDBNull("update_action") ? null : (ForeignKeyAction)rdr.GetByte("update_action")
                };

                var columnArray = (string[])rdr["table_columns"];

                foreach (var columnName in columnArray)
                {
                    newForeignKey.Columns.Add(new ColumnReference()
                    {
                        Name = columnName
                    });
                }

                columnArray = (string[])rdr["foreign_table_columns"];

                foreach (var columnName in columnArray)
                {
                    newForeignKey.ForeignTableColumns.Add(new ColumnReference()
                    {
                        Name = columnName
                    });
                }

                table.ForeignKeys.Add(newForeignKey);
            }
        }
    }

    private async Task ReadIndicesAsync(DbConnection connection, Schema schema, Table table, CancellationToken cancellationToken)
    {
        using DbCommand cmd = connection.CreateCommand();

        cmd.CommandText =
            """
            SELECT
              i_rel.relname AS index_name,
              idx.indisprimary AS index_primary,
              idx.indisunique AS index_unique,
              ARRAY_AGG(col.attname ORDER BY col.attnum) AS index_columns 
            FROM
              pg_index idx 
            JOIN
              pg_class tbl ON tbl.oid = idx.indrelid 
            JOIN
              pg_namespace sch ON sch.oid = tbl.relnamespace 
            JOIN
              pg_class i_rel ON i_rel.oid = idx.indexrelid 
            LEFT JOIN
              pg_attribute col ON col.attrelid = tbl.oid 
            WHERE
              tbl.relkind = 'r' AND 
              col.attnum = ANY(idx.indkey) AND 
              sch.nspname = @schema AND 
              tbl.relname = @table 
            GROUP BY
              index_name, index_primary, index_unique 
            ORDER BY
              index_primary DESC
            """;

        cmd.CommandType = CommandType.Text;
        cmd.AddParameter("schema", schema.Name);
        cmd.AddParameter("table", table.Name);

        using (DbDataReader rdr = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await rdr.ReadAsync(cancellationToken))
            {
                var newIndex = new Index()
                {
                    Name = rdr.GetString("index_name"),
                    IsUnique = rdr.GetBoolean("index_unique")
                };

                var columnArray = (string[])rdr["index_columns"];

                foreach (var columnName in columnArray)
                {
                    newIndex.Columns.Add(new ColumnReference()
                    {
                        Name = columnName
                    });
                }

                table.Indices.Add(newIndex);
            }
        }
    }

    private async Task ReadPrimaryKeysAsync(DbConnection connection, Schema schema, Table table, CancellationToken cancellationToken)
    {
        using DbCommand cmd = connection.CreateCommand();

        cmd.CommandText =
            """
            SELECT
              c.conname AS constraint_name,
              c.contype AS constraint_type,
              ARRAY_AGG(col.attname ORDER BY u.attposition) AS table_columns
            FROM
              pg_constraint c
            LEFT JOIN
              LATERAL UNNEST(c.conkey) WITH ORDINALITY AS u(attnum, attposition) ON TRUE
            INNER JOIN
              pg_class tbl ON tbl.oid = c.conrelid 
            INNER JOIN
              pg_namespace sch ON sch.oid = tbl.relnamespace
            LEFT JOIN
              pg_attribute col ON col.attrelid = tbl.oid AND col.attnum = u.attnum 
            WHERE
              c.contype = 'p' AND 
              sch.nspname = @schema AND 
              tbl.relname = @table 
            GROUP BY
              constraint_name,
              constraint_type 
            ORDER BY
              constraint_type, constraint_name
            """;

        cmd.CommandType = CommandType.Text;
        cmd.AddParameter("schema", schema.Name);
        cmd.AddParameter("table", table.Name);

        using (DbDataReader rdr = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await rdr.ReadAsync(cancellationToken))
            {
                table.PrimaryKey = new PrimaryKey()
                {
                    Name = rdr.GetString("constraint_name")
                };

                var columnArray = (string[])rdr["table_columns"];

                foreach (var columnName in columnArray)
                {
                    table.PrimaryKey.Columns.Add(new ColumnReference()
                    {
                        Name = columnName
                    });
                }
            }
        }
    }

    private async Task ReadSchemataAsync(DbConnection connection, DbSchema dbSchema, CancellationToken cancellationToken)
    {
        using DbCommand cmd = connection.CreateCommand();

        cmd.CommandText =
            """
            SELECT
              sch.nspname AS schema_name 
            FROM
              pg_namespace sch
            WHERE
              NOT (sch.nspname IN ('pg_toast', 'pg_catalog', 'information_schema'))
            ORDER BY
              schema_name
            """;

        cmd.CommandType = CommandType.Text;

        using (DbDataReader rdr = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await rdr.ReadAsync(cancellationToken))
            {
                dbSchema.Schemata.Add(new Schema()
                {
                    Name = rdr.GetString("schema_name")
                });
            }
        }

        foreach (var schema in dbSchema.Schemata)
        {
            await ReadTablesAsync(connection, schema, cancellationToken);
            await ReadViewsAsync(connection, schema, cancellationToken);
        }
    }

    private async Task ReadTablesAsync(DbConnection connection, Schema schema, CancellationToken cancellationToken)
    {
        using DbCommand cmd = connection.CreateCommand();

        cmd.CommandText =
            """
            SELECT
              tbl.relname AS table_name 
            FROM
              pg_class tbl 
            JOIN
              pg_namespace sch ON sch.oid = tbl.relnamespace 
            WHERE
              tbl.relkind = 'r' AND sch.nspname=@schema 
            ORDER BY
              table_name
            """;

        cmd.CommandType = CommandType.Text;
        cmd.AddParameter("schema", schema.Name);

        using (DbDataReader rdr = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await rdr.ReadAsync(cancellationToken))
            {
                schema.Tables.Add(new Table()
                {
                    Name = rdr.GetString("table_name")
                });
            }
        }

        foreach (var table in schema.Tables)
        {
            await ReadColumnsAsync(connection, schema, table, cancellationToken);
            await ReadPrimaryKeysAsync(connection, schema, table, cancellationToken);
            await ReadForeignKeysAsync(connection, schema, table, cancellationToken);
            await ReadIndicesAsync(connection, schema, table, cancellationToken);
        }
    }
    private async Task ReadViewsAsync(DbConnection connection, Schema schema, CancellationToken cancellationToken)
    {
        using DbCommand cmd = connection.CreateCommand();

        cmd.CommandText =
            """
            SELECT
              rel.relname AS view_name,
              viw.definition AS view_source
            FROM
              pg_class rel 
            JOIN
              pg_namespace sch ON sch.oid = rel.relnamespace 
            JOIN
              pg_views viw ON viw.schemaname = sch.nspname AND viw.viewname = rel.relname              
            WHERE
              rel.relkind = 'v' AND sch.nspname=@schema 
            ORDER BY
              view_name
            """;

        cmd.CommandType = CommandType.Text;
        cmd.AddParameter("schema", schema.Name);

        using (DbDataReader rdr = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await rdr.ReadAsync(cancellationToken))
            {
                schema.Views.Add(new View()
                {
                    Name = rdr.GetString("view_name"),
                    Source = rdr.GetString("view_source")
                });
            }
        }

        foreach (var view in schema.Views)
        {
            await ReadColumnsAsync(connection, schema, view, cancellationToken);
        }
    }
}
