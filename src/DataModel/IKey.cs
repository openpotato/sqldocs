﻿#region SqlDocs - Copyright (C) 2023 STÜBER SYSTEMS GmbH
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

namespace SqlDocs.DataModel;

/// <summary>
/// An abstract interface for a database key.
/// </summary>
public interface IKey
{
    /// <summary>
    /// List of column references
    /// </summary>
    public List<ColumnReference> Columns { get; set; }

    /// <summary>
    /// Description of the key
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Name of the key
    /// </summary>
    public string Name { get; set; }
}