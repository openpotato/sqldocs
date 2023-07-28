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

namespace SqlDocs;

public static class Links
{
    public static bool TryGetDataTypeDocLink(DatabaseEngine databaseEngine, string dataType, out string link)
    {
        switch (databaseEngine)
        {
            case DatabaseEngine.Postgres:
                switch (dataType.CutFrom(new char[] { ' ', '[', '(' }))
                {
                    case "bigint":
                    case "bigserial":
                    case "decimal":
                    case "double":
                    case "integer": 
                    case "numeric": 
                    case "real":
                    case "serial":
                    case "smallint":
                    case "smallserial": 
                        link = "https://www.postgresql.org/docs/current/datatype-numeric.html";
                        return true;
                    case "money": 
                        link = "https://www.postgresql.org/docs/current/datatype-money.html";
                        return true;
                    case "character":
                    case "text": 
                        link = "https://www.postgresql.org/docs/current/datatype-character.html";
                        return true;
                    case "timestamp": 
                    case "date": 
                    case "time": 
                    case "interval": 
                        link = "https://www.postgresql.org/docs/current/datatype-datetime.html";
                        return true;
                    case "boolean":
                        link = "https://www.postgresql.org/docs/current/datatype-boolean.html";
                        return true;
                    case "bit":
                        link = "https://www.postgresql.org/docs/current/datatype-bit.html";
                        return true;
                    case "bita":
                        link = "https://www.postgresql.org/docs/current/datatype-binary.html";
                        return true;
                    case "tsvector":
                    case "tsquery":
                        link = "https://www.postgresql.org/docs/current/datatype-textsearch.html";
                        return true;
                    case "box":
                    case "circle":
                    case "line":
                    case "lseg":
                    case "path":
                    case "point":
                    case "polygon":
                        link = "https://www.postgresql.org/docs/current/datatype-geometric.html";
                        return true;
                    case "json": 
                    case "jsonb": 
                        link = "https://www.postgresql.org/docs/current/datatype-json.html";
                        return true;
                    case "xml": 
                        link = "https://www.postgresql.org/docs/current/datatype-xml.html";
                        return true;
                    case "int8range":
                    case "int8multirange":
                    case "numrange":
                    case "nummultirange":
                    case "tsrange":
                    case "tsmultirange":
                    case "tstzrange":
                    case "tstzmultirange":
                    case "daterange":
                    case "datemultirange":
                        link = "https://www.postgresql.org/docs/current/rangetypes.html";
                        return true;
                    case "cidr": 
                    case "inet": 
                    case "macaddr": 
                    case "macaddr8": 
                        link = "https://www.postgresql.org/docs/current/datatype-net-types.html";
                        return true;
                    case "uuid": 
                        link = "https://www.postgresql.org/docs/current/datatype-uuid.html";
                        return true;
                    default:
                        link = null;
                        return false;
                };
            case DatabaseEngine.Firebird:
                switch (dataType.CutFrom(new char[] { ' ', '[', '(' }))
                {
                    case "SMALLINT":
                    case "INTEGER":
                    case "BIGINT":
                    case "INT128":
                        link = "https://firebirdsql.org/file/documentation/html/en/refdocs/fblangref40/firebird-40-language-reference.html#fblangref40-datatypes-inttypes";
                        return true;
                    case "FLOAT":
                    case "DOUBLE":
                    case "DECFLOAT(16)":
                    case "DECFLOAT(34)":
                        link = "https://firebirdsql.org/file/documentation/html/en/refdocs/fblangref40/firebird-40-language-reference.html#fblangref40-datatypes-floattypes";
                        return true;
                    case "NUMERIC":
                    case "DECIMAL":
                        link = "https://firebirdsql.org/file/documentation/html/en/refdocs/fblangref40/firebird-40-language-reference.html#fblangref40-datatypes-fixedtypes";
                        return true;
                    case "DATE":
                    case "TIME":
                    case "TIMESTAMP":
                        link = "https://firebirdsql.org/file/documentation/html/en/refdocs/fblangref40/firebird-40-language-reference.html#fblangref40-datatypes-datetime";
                        return true;
                    case "CHAR":
                    case "VARCHAR":
                        link = "https://firebirdsql.org/file/documentation/html/en/refdocs/fblangref40/firebird-40-language-reference.html#fblangref40-datatypes-chartypes";
                        return true;
                    case "BOOLEAN":
                        link = "https://firebirdsql.org/file/documentation/html/en/refdocs/fblangref40/firebird-40-language-reference.html#fblangref40-datatypes-booleantypes";
                        return true;
                    case "BLOB":
                        link = "https://firebirdsql.org/file/documentation/html/en/refdocs/fblangref40/firebird-40-language-reference.html#fblangref40-datatypes-bnrytypes";
                        return true;
                    default:
                        link = null;
                        return false;
                };
            default: 
                link = null;
                return false;
        }
    }
}