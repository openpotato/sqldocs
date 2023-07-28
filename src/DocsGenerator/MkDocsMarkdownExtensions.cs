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
using System.Text.RegularExpressions;

namespace SqlDocs;

public static partial class MkDocsMarkdownExtensions
{
    public static MarkdownParagraph AppendDescription(this MarkdownParagraph paragraph, string pathPrefix, string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            var matches = DescriptionRegEx().Matches(text);
            var textPos = 0;

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var textBeforeMatch = text[textPos..match.Index];

                paragraph.AppendText(textBeforeMatch);

                if (match.Groups["bold"].Success)
                {
                    paragraph.AppendStrong($"{match.Groups["bold"].Value}");
                }
                else
                if (match.Groups["italic"].Success)
                {
                    paragraph.AppendEmphasis($"{match.Groups["italic"].Value}");
                }
                else
                if (match.Groups["code"].Success)
                {
                    paragraph.AppendCodeSpan($"{match.Groups["code"].Value}");
                }
                else
                if (match.Groups["type"].Success)
                {
                    if (string.IsNullOrEmpty(match.Groups["schema"].Value))
                    {
                        switch (match.Groups["type"].Value.ToLowerInvariant())
                        {
                            case "schema":
                                paragraph.AppendLink($"{match.Groups["name"].Value}",
                                    $"{pathPrefix}../../{match.Groups["name"].Value.ToLowerInvariant()}/schema");
                                break;
                            case "table":
                                paragraph.AppendLink($"{match.Groups["name"].Value}",
                                    $"{pathPrefix}../tables/{match.Groups["name"].Value.ToLowerInvariant()}");
                                break;
                            case "view":
                                paragraph.AppendLink($"{match.Groups["name"].Value}",
                                    $"{pathPrefix}../views/{match.Groups["name"].Value.ToLowerInvariant()}");
                                break;
                            case "column":
                                paragraph.AppendCodeSpan(match.Groups["name"].Value);
                                break;
                        }
                    }
                    else
                    {
                        switch (match.Groups["type"].Value.ToLowerInvariant())
                        {
                            case "table":
                                paragraph.AppendLink($"{match.Groups["schema"].Value}.{match.Groups["name"].Value}",
                                    $"{pathPrefix}../../{match.Groups["schema"].Value.ToLowerInvariant()}/tables/{match.Groups["name"].Value.ToLowerInvariant()}");
                                break;
                            case "view":
                                paragraph.AppendLink($"{match.Groups["schema"].Value}.{match.Groups["name"].Value}",
                                    $"{pathPrefix}../../{match.Groups["schema"].Value.ToLowerInvariant()}/views/{match.Groups["name"].Value.ToLowerInvariant()}");
                                break;
                        }
                    }
                }

                textPos = match.Index + match.Length;
            }

            var remainingText = text[textPos..text.Length];

            paragraph.AppendText(remainingText);
        }
        return paragraph;
    }

    public static IMarkdownContainerBlock AppendDescription(this IMarkdownContainerBlock block, string pathPrefix, string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            block.AppendParagraph(x => x.AppendDescription(pathPrefix, text));
        }
        return block;
    }

    public static MarkdownTableRow AppendDescription(this MarkdownTableRow row, string pathPrefix, string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            row.Append(x => x.AppendDescription(pathPrefix, text));
        }
        return row;
    }

    [GeneratedRegex(
      """
      @((?<type>\w+)\:((?<schema>\w+)\.)?(?<name>\w+))|(<code>(?<code>.*?)<\/code>)|(<b>(?<bold>.*?)<\/b>)|(<i>(?<italic>.*?)<\/i>)
      """, 
      RegexOptions.IgnoreCase)]
    private static partial Regex DescriptionRegEx();
}
