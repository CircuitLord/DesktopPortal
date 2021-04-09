namespace AngleSharp.Extensions
{
    using AngleSharp.Dom;
    using AngleSharp.Dom.Collections;
    using AngleSharp.Dom.Css;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A set of extension methods for style / related methods.
    /// </summary>
    static class StyleExtensions
    {
        /// <summary>
        /// Computes the declarations for the given element in the context of
        /// the specified styling rules.
        /// </summary>
        /// <param name="rules">The styles to use.</param>
        /// <param name="element">The element that is questioned.</param>
        /// <param name="pseudoSelector">The optional pseudo selector to use.</param>
        /// <returns>The style declaration containing all the declarations.</returns>
        public static CssStyleDeclaration ComputeDeclarations(this StyleCollection rules, IElement element, String pseudoSelector = null)
        {
            var computedStyle = new CssStyleDeclaration();
            var pseudoElement = PseudoElement.Create(element, pseudoSelector);

            if (pseudoElement != null)
            {
                element = pseudoElement;
            }

            computedStyle.SetDeclarations(rules.ComputeCascadedStyle(element).Declarations);

            var nodes = element.GetAncestors().OfType<IElement>();

            foreach (var node in nodes)
            {
                var style = rules.ComputeCascadedStyle(node);
                computedStyle.UpdateDeclarations(style.Declarations);
            }

            return computedStyle;
        }

        /// <summary>
        /// Gets all possible style sheet sets from the list of style sheets.
        /// </summary>
        /// <param name="sheets">The list of style sheets.</param>
        /// <returns>An enumeration over all sets.</returns>
        public static IEnumerable<String> GetAllStyleSheetSets(this IStyleSheetList sheets)
        {
            var existing = new List<String>();

            foreach (var sheet in sheets)
            {
                var title = sheet.Title;

                if (String.IsNullOrEmpty(title) || existing.Contains(title))
                {
                    continue;
                }

                existing.Add(title);
                yield return title;
            }
        }

        /// <summary>
        /// Gets the enabled style sheet sets from the list of style sheets.
        /// </summary>
        /// <param name="sheets">The list of style sheets.</param>
        /// <returns>An enumeration over the enabled sets.</returns>
        public static IEnumerable<String> GetEnabledStyleSheetSets(this IStyleSheetList sheets)
        {
            var excluded = new List<String>();

            foreach (var sheet in sheets)
            {
                var title = sheet.Title;

                if (String.IsNullOrEmpty(title) || excluded.Contains(title))
                {
                    continue;
                }
                else if (sheet.IsDisabled)
                {
                    excluded.Add(title);
                }
            }

            return sheets.GetAllStyleSheetSets().Except(excluded);
        }

        /// <summary>
        /// Sets the enabled style sheet sets in the list of style sheets.
        /// </summary>
        /// <param name="sheets">The list of style sheets.</param>
        /// <param name="name">The name of the set to enabled.</param>
        public static void EnableStyleSheetSet(this IStyleSheetList sheets, String name)
        {
            foreach (var sheet in sheets)
            {
                var title = sheet.Title;

                if (!String.IsNullOrEmpty(title))
                {
                    sheet.IsDisabled = title != name;
                }
            }
        }

        /// <summary>
        /// Creates a new StyleSheetList instance for the given node.
        /// </summary>
        /// <param name="parent">The node to get the StyleSheets from.</param>
        /// <returns>The new StyleSheetList instance.</returns>
        public static IStyleSheetList CreateStyleSheets(this INode parent)
        {
            var list = parent.GetStyleSheets();
            return new StyleSheetList(list);
        }

        /// <summary>
        /// Creates a new StringList instance with stylesheet sets for the given
        /// node.
        /// </summary>
        /// <param name="parent">The node to get the sets from.</param>
        /// <returns>The new StringList instance.</returns>
        public static IStringList CreateStyleSheetSets(this INode parent)
        {
            var list = parent.GetStyleSheets().Select(m => m.Title).Where(m => m != null);
            return new StringList(list);
        }

        /// <summary>
        /// Gets an enumeration over all the stylesheets from the given parent.
        /// </summary>
        /// <param name="parent">The parent to use.</param>
        /// <returns>The enumeration over all stylesheets.</returns>
        public static IEnumerable<IStyleSheet> GetStyleSheets(this INode parent)
        {
            foreach (var child in parent.ChildNodes)
            {
                if (child.NodeType == NodeType.Element)
                {
                    var linkStyle = child as ILinkStyle;

                    if (linkStyle != null)
                    {
                        var sheet = linkStyle.Sheet;

                        if (sheet != null && !sheet.IsDisabled)
                        {
                            yield return sheet;
                        }
                    }
                    else
                    {
                        foreach (var sheet in child.GetStyleSheets())
                        {
                            yield return sheet;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tries to find the matching namespace url for the given prefix.
        /// </summary>
        /// <param name="sheets">The list of style sheets.</param>
        /// <param name="prefix">The prefix of the namespace to find.</param>
        public static String LocateNamespace(this IStyleSheetList sheets, String prefix)
        {
            foreach (var sheet in sheets)
            {
                var css = sheet as CssStyleSheet;

                if (sheet.IsDisabled || css == null)
                {
                    continue;
                }

                foreach (var rule in css.Rules.OfType<CssNamespaceRule>())
                {
                    if (rule.Prefix.Is(prefix))
                    {
                        return rule.NamespaceUri;
                    }
                }
            }

            return null;
        }
    }
}
