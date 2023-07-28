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

namespace SqlDocs;

public static class MarkdownExtensions
{
    public static MarkdownTableHeader AppendRawColumn(this MarkdownTableHeader header, string rawText)
    {
        if (!string.IsNullOrEmpty(rawText))
        {
            header.AppendColumn(x => x.Assign(x => x.AppendRawText(rawText)));
        }
        return header;
    }

    public static IMarkdownContainerBlock AppendRawHeading(this IMarkdownContainerBlock block, byte level, string rawText)
    {
        if (!string.IsNullOrEmpty(rawText))
        {
            block.AppendHeading(level, x => x.AppendRawText(rawText));
        }
        return block;
    }

    public static IMarkdownContainerBlock AppendRawParagraph(this IMarkdownContainerBlock block, string rawText)
    {
        if (!string.IsNullOrEmpty(rawText))
        {
            block.AppendParagraph(x => x.AppendRawText(rawText));
        }
        return block;
    }
}
