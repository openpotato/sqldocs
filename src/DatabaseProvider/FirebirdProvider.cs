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

using FirebirdSql.Data.FirebirdClient;
using SqlDocs.DataModel;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SqlDocs.DatabaseProvider;

/// <summary>
/// A database provider for Firebird
/// </summary>
public class FirebirdProvider : IDatabaseProvider
{
    private readonly string _connectionString;

    public FirebirdProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<DbSchema> GetDbSchemaAsync(CancellationToken cancellationToken)
    {
        using DbConnection connection = new FbConnection(_connectionString);

        connection.Open();

        var dbSchema = new DbSchema
        {
            Name = "SqlDocs",
            Description = connection.Database,
            DbmsName = "Firebird",
            DbmsVersion = connection.ServerVersion,
        };

        await ReadTablesAsync(connection, dbSchema, cancellationToken);
        await ReadViewsAsync(connection, dbSchema, cancellationToken);

        return dbSchema;
    }

    private async Task ReadColumnsAsync(DbConnection connection, IRelation relation, CancellationToken cancellationToken)
    {
        using DbCommand cmd = connection.CreateCommand();

        cmd.CommandText =
            """
            SELECT
              TRIM(rel_fld.RDB$FIELD_NAME) AS FIELD_NAME,
              rel_fld.RDB$FIELD_POSITION AS FIELD_POS,
              rel_fld.RDB$NULL_FLAG AS FIELD_NULL_FLAG,
              CASE fld.RDB$FIELD_TYPE
                WHEN 7 THEN 
                  CASE fld.RDB$FIELD_SUB_TYPE 
                      WHEN 1 THEN 'NUMERIC(' || COALESCE(fld.RDB$FIELD_PRECISION, '?') || ', ' || (-fld.RDB$FIELD_SCALE) || ')'
                      WHEN 2 THEN 'DECIMAL'
                      ELSE 'SMALLINT'
                  END  
                  WHEN 8 THEN 
                  CASE fld.RDB$FIELD_SUB_TYPE 
                      WHEN 1 THEN 'NUMERIC(' || COALESCE(fld.RDB$FIELD_PRECISION, '?') || ', ' || (-fld.RDB$FIELD_SCALE) || ')'
                      WHEN 2 THEN 'DECIMAL'
                      ELSE 'INTEGER'
                  END  
                WHEN 9 THEN 'QUAD'
                WHEN 10 THEN 'FLOAT'
                WHEN 12 THEN 'DATE'
                WHEN 13 THEN 'TIME'
                WHEN 14 THEN 'CHAR(' || (TRUNC(fld.RDB$FIELD_LENGTH / fld_chs.RDB$BYTES_PER_CHARACTER)) || ')'
                WHEN 16 THEN 
                  CASE fld.RDB$FIELD_SUB_TYPE 
                      WHEN 1 THEN 'NUMERIC(' || COALESCE(fld.RDB$FIELD_PRECISION, '?') || ', ' || (-fld.RDB$FIELD_SCALE) || ')'
                      WHEN 2 THEN 'DECIMAL'
                      ELSE 'BIGINT'
                  END  
                WHEN 23 THEN 'BOOLEAN'
                WHEN 24 THEN 'DECFLOAT(16)'
                WHEN 25 THEN 'DECFLOAT(34)'
                WHEN 26 THEN 
                  CASE fld.RDB$FIELD_SUB_TYPE 
                      WHEN 1 THEN 'NUMERIC(' || COALESCE(fld.RDB$FIELD_PRECISION, '?') || ', ' || (-fld.RDB$FIELD_SCALE) || ')'
                      WHEN 2 THEN 'DECIMAL'
                      ELSE 'INT128'
                  END  
                WHEN 27 THEN 'DOUBLE PRECISION'
                WHEN 28 THEN 'TIME WITH TIME ZONE'
                WHEN 29 THEN 'TIMESTAMP WITH WITH TIME ZONE'
                WHEN 35 THEN 'TIMESTAMP'
                WHEN 37 THEN 
                  IIF (COALESCE(fld.RDB$COMPUTED_SOURCE,'') <> '',
                    'COMPUTED BY ' || CAST(fld.RDB$COMPUTED_SOURCE AS VARCHAR(250)),
                    'VARCHAR(' || (TRUNC(fld.RDB$FIELD_LENGTH / fld_chs.RDB$BYTES_PER_CHARACTER)) || ')')
                WHEN 40 THEN 'CSTRING' || (TRUNC(fld.RDB$FIELD_LENGTH / fld_chs.RDB$BYTES_PER_CHARACTER)) || ')'
                WHEN 45 THEN 'BLOB_ID'
                WHEN 261 THEN
                  CASE fld.RDB$FIELD_SUB_TYPE 
                    WHEN 0 THEN 'BLOB subtype binary'
                    WHEN 1 THEN 'BLOB subtype text'
                    ELSE 'BLOB'
                  END  
                ELSE NULL
              END AS FIELD_TYPE,
              fld_chs.RDB$CHARACTER_SET_NAME FIELD_CHARSET,
              fld_col.RDB$COLLATION_NAME FIELD_COLLATION,
              fld.RDB$DIMENSIONS AS ARRAY_DIMENSIONS,
              LIST(fld_dim.BOUNDS, ',') AS ARRAY_DIMENSION_BOUNDS,
              COALESCE(rel_fld.RDB$DEFAULT_SOURCE, fld.RDB$DEFAULT_SOURCE) AS DEFAULT_SOURCE
            FROM
              RDB$RELATION_FIELDS rel_fld 
            JOIN
              RDB$FIELDS fld 
            ON
              COALESCE(fld.RDB$SYSTEM_FLAG, 0) = 0 AND rel_fld.RDB$FIELD_SOURCE = fld.RDB$FIELD_NAME 
            LEFT JOIN
              (SELECT RDB$FIELD_NAME, (RDB$LOWER_BOUND || ':' || RDB$UPPER_BOUND) AS BOUNDS
               FROM RDB$FIELD_DIMENSIONS
               ORDER BY RDB$DIMENSION) AS fld_dim
            ON
              fld.RDB$FIELD_NAME = fld_dim.RDB$FIELD_NAME
            LEFT JOIN 
              RDB$CHARACTER_SETS fld_chs
            ON 
              fld_chs.RDB$CHARACTER_SET_ID = fld.RDB$CHARACTER_SET_ID
            LEFT JOIN 
              RDB$COLLATIONS fld_col 
            ON 
              fld_col.RDB$COLLATION_ID = fld.RDB$COLLATION_ID AND fld_col.RDB$CHARACTER_SET_ID = fld.RDB$CHARACTER_SET_ID
            WHERE
              rel_fld.RDB$RELATION_NAME=@table 
            GROUP BY
              FIELD_NAME,
              FIELD_POS,
              FIELD_NULL_FLAG,
              FIELD_TYPE,
              FIELD_CHARSET,
              FIELD_COLLATION,
              ARRAY_DIMENSIONS,
              DEFAULT_SOURCE
            ORDER BY
              FIELD_POS
            """;

        cmd.CommandType = CommandType.Text;
        cmd.AddParameter("table", relation.Name);

        using (DbDataReader rdr = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await rdr.ReadAsync(cancellationToken))
            {
                relation.Columns.Add(new Column()
                {
                    Name = rdr.GetString("FIELD_NAME"),
                    IsNullable = rdr.IsDBNull("FIELD_NULL_FLAG") ? true : rdr.GetFieldValue<int>("FIELD_NULL_FLAG") != 1,
                    IsArray = rdr.IsDBNull("ARRAY_DIMENSIONS") ? false : rdr.GetFieldValue<int>("ARRAY_DIMENSIONS") > 0,
                    DataType = rdr.IsDBNull("ARRAY_DIMENSION_BOUNDS") ? rdr.GetString("FIELD_TYPE").TrimEnd() : $"{rdr.GetString("FIELD_TYPE").TrimEnd()} [{rdr.GetString("ARRAY_DIMENSION_BOUNDS")}]",
                    Default = rdr.IsDBNull("DEFAULT_SOURCE") ? null : rdr.GetString("DEFAULT_SOURCE"),
                }); 
            }
        }
    }

    private async Task ReadForeignKeysAsync(DbConnection connection, Table table, CancellationToken cancellationToken)
    {
        using DbCommand cmd = connection.CreateCommand();

        cmd.CommandText =
            """
            SELECT
              TRIM(rel_con.RDB$CONSTRAINT_NAME) AS CONSTRAINT_NAME,
              LIST(DISTINCT TRIM(idx_seg.RDB$FIELD_NAME), ',') AS TABLE_COLUMNS,
              TRIM(foreign_rel_con.RDB$RELATION_NAME) AS FOREIGN_TABLE_NAME,
              LIST(DISTINCT TRIM(foreign_idx_seg.RDB$FIELD_NAME), ',') AS FOREIGN_TABLE_COLUMNS,
              CASE ref_con.RDB$UPDATE_RULE
                WHEN 'NO ACTION' THEN 0
                WHEN 'RESTRICT' THEN 1
                WHEN 'CASCADE' THEN 2
                WHEN 'SET NULL' THEN 3
                WHEN 'SET DEFAULT' THEN 4
                ELSE NULL
              END AS UPDATE_ACTION,
              CASE ref_con.RDB$DELETE_RULE
                WHEN 'NO ACTION' THEN 0
                WHEN 'RESTRICT' THEN 1
                WHEN 'CASCADE' THEN 2
                WHEN 'SET NULL' THEN 3
                WHEN 'SET DEFAULT' THEN 4
                ELSE NULL
              END AS DELETE_ACTION
            FROM
              RDB$RELATION_CONSTRAINTS rel_con 
            LEFT JOIN
              RDB$REF_CONSTRAINTS ref_con 
            ON
              rel_con.RDB$CONSTRAINT_NAME = ref_con.RDB$CONSTRAINT_NAME 
            LEFT JOIN
              RDB$RELATION_CONSTRAINTS foreign_rel_con 
            ON
              foreign_rel_con.RDB$CONSTRAINT_NAME = ref_con.RDB$CONST_NAME_UQ 
            LEFT JOIN
              (SELECT RDB$INDEX_NAME, RDB$FIELD_NAME
               FROM RDB$INDEX_SEGMENTS
               ORDER BY RDB$INDEX_NAME, RDB$FIELD_POSITION) AS idx_seg 
            ON
              idx_seg.RDB$INDEX_NAME = rel_con.RDB$INDEX_NAME 
            LEFT JOIN
              (SELECT RDB$INDEX_NAME, RDB$FIELD_NAME
               FROM RDB$INDEX_SEGMENTS
               ORDER BY RDB$INDEX_NAME, RDB$FIELD_POSITION) AS foreign_idx_seg 
            ON
              foreign_idx_seg.RDB$INDEX_NAME = foreign_rel_con.RDB$INDEX_NAME 
            WHERE
              rel_con.RDB$CONSTRAINT_TYPE = 'FOREIGN KEY' AND rel_con.RDB$RELATION_NAME = @table 
            GROUP BY
              CONSTRAINT_NAME, FOREIGN_TABLE_NAME, UPDATE_ACTION, DELETE_ACTION
            """;

        cmd.CommandType = CommandType.Text;
        cmd.AddParameter("table", table.Name);

        using (DbDataReader rdr = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await rdr.ReadAsync(cancellationToken))
            {
                var newForeignKey = new ForeignKey()
                {
                    Name = rdr.GetString("CONSTRAINT_NAME"),
                    ForeignTableName = rdr.GetString("FOREIGN_TABLE_NAME"),
                    DeleteAction = rdr.IsDBNull("DELETE_ACTION") ? null : (ForeignKeyAction)rdr.GetByte("DELETE_ACTION"),
                    UpdateAction = rdr.IsDBNull("UPDATE_ACTION") ? null : (ForeignKeyAction)rdr.GetByte("UPDATE_ACTION")
                };

                var columnArray = rdr.GetString("TABLE_COLUMNS").Split(','); 

                foreach (var columnName in columnArray)
                {
                    newForeignKey.Columns.Add(new ColumnReference()
                    {
                        Name = columnName
                    });
                }

                columnArray = rdr.GetString("FOREIGN_TABLE_COLUMNS").Split(',');

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

    private async Task ReadIndicesAsync(DbConnection connection, Table table, CancellationToken cancellationToken)
    {
        using DbCommand cmd = connection.CreateCommand();

        cmd.CommandText =
            """
            SELECT
              TRIM(idx.RDB$INDEX_NAME) AS INDEX_NAME,
              idx.RDB$UNIQUE_FLAG AS INDEX_UNIQUE,
              LIST(TRIM(idx_seg.RDB$FIELD_NAME), ',') AS INDEX_COLUMNS 
            FROM
              RDB$INDICES idx 
            LEFT JOIN
              (SELECT RDB$INDEX_NAME, RDB$FIELD_NAME
               FROM RDB$INDEX_SEGMENTS
               ORDER BY RDB$INDEX_NAME, RDB$FIELD_POSITION) AS idx_seg 
            ON
              idx.RDB$INDEX_NAME = idx_seg.RDB$INDEX_NAME 
            WHERE
              COALESCE(idx.RDB$SYSTEM_FLAG, 0) = 0 AND idx.RDB$RELATION_NAME=@table 
            GROUP BY
              INDEX_NAME, INDEX_UNIQUE
            """;

        cmd.CommandType = CommandType.Text;
        cmd.AddParameter("table", table.Name);

        using (DbDataReader rdr = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await rdr.ReadAsync(cancellationToken))
            {
                var newIndex = new Index()
                {
                    Name = rdr.GetString("INDEX_NAME"),
                    IsUnique = rdr.IsDBNull("INDEX_UNIQUE") ? false : rdr.GetFieldValue<int>("INDEX_UNIQUE") == 1
                };

                var columnArray = rdr.GetString("INDEX_COLUMNS").Split(',');

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

    private async Task ReadPrimaryKeysAsync(DbConnection connection, Table table, CancellationToken cancellationToken)
    {
        using DbCommand cmd = connection.CreateCommand();

        cmd.CommandText =
            """
            SELECT
              TRIM(rel_con.RDB$CONSTRAINT_NAME) AS CONSTRAINT_NAME,
              LIST(TRIM(idx_seg.RDB$FIELD_NAME), ',') AS TABLE_COLUMNS 
            FROM
              RDB$RELATION_CONSTRAINTS rel_con 
            LEFT JOIN
              RDB$REF_CONSTRAINTS ref_con 
            ON
              rel_con.RDB$CONSTRAINT_NAME = ref_con.RDB$CONSTRAINT_NAME
            LEFT JOIN
              (SELECT RDB$INDEX_NAME, RDB$FIELD_NAME
               FROM RDB$INDEX_SEGMENTS
               ORDER BY RDB$INDEX_NAME, RDB$FIELD_POSITION) AS idx_seg 
            ON
              idx_seg.RDB$INDEX_NAME = rel_con.RDB$INDEX_NAME 
            WHERE
              rel_con.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY' AND rel_con.RDB$RELATION_NAME = @table
            GROUP BY
              CONSTRAINT_NAME
            """;

        cmd.CommandType = CommandType.Text;
        cmd.AddParameter("table", table.Name);

        using (DbDataReader rdr = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await rdr.ReadAsync(cancellationToken))
            {
                table.PrimaryKey = new PrimaryKey()
                {
                    Name = rdr.GetString("CONSTRAINT_NAME")
                };

                var columnArray = rdr.GetString("TABLE_COLUMNS").Split(',');

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

    private async Task ReadTablesAsync(DbConnection connection, DbSchema dbSchema, CancellationToken cancellationToken)
    {
        using DbCommand cmd = connection.CreateCommand();

        cmd.CommandText =
            """
            SELECT
              TRIM(rel.RDB$RELATION_NAME) AS TABLE_NAME 
            FROM
              RDB$RELATIONS rel 
            WHERE
              COALESCE(rel.RDB$SYSTEM_FLAG, 0) = 0 AND rel.RDB$RELATION_TYPE = 0 
            ORDER BY
              TABLE_NAME
            """;

        cmd.CommandType = CommandType.Text;

        using (DbDataReader rdr = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await rdr.ReadAsync(cancellationToken))
            {
                dbSchema.Tables.Add(new Table()
                {
                    Name = rdr.GetString("TABLE_NAME")
                });
            }
        }

        foreach (var table in dbSchema.Tables)
        {
            await ReadColumnsAsync(connection, table, cancellationToken);
            await ReadPrimaryKeysAsync(connection, table, cancellationToken);
            await ReadForeignKeysAsync(connection, table, cancellationToken);
            await ReadIndicesAsync(connection, table, cancellationToken);
        }
    }

    private async Task ReadViewsAsync(DbConnection connection, DbSchema dbSchema, CancellationToken cancellationToken)
    {
        using DbCommand cmd = connection.CreateCommand();

        cmd.CommandText =
            """
            SELECT
              TRIM(rel.RDB$RELATION_NAME) AS VIEW_NAME,
              TRIM(rel.RDB$VIEW_SOURCE) AS VIEW_SOURCE
            FROM
              RDB$RELATIONS rel 
            WHERE
              COALESCE(rel.RDB$SYSTEM_FLAG, 0) = 0 AND rel.RDB$RELATION_TYPE = 1 
            ORDER BY
              VIEW_NAME
            """;

        cmd.CommandType = CommandType.Text;

        using (DbDataReader rdr = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await rdr.ReadAsync(cancellationToken))
            {
                dbSchema.Views.Add(new View()
                {
                    Name = rdr.GetString("VIEW_NAME"),
                    Source = rdr.GetString("VIEW_SOURCE")
                });
            }
        }

        foreach (var view in dbSchema.Views)
        {
            await ReadColumnsAsync(connection, view, cancellationToken);
        }
    }
}
