﻿namespace AngleSharp.Extensions
{
    using AngleSharp.Dom;
    using AngleSharp.Dom.Css;
    using AngleSharp.Parser.Css;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines a set of extension methods involving selectors.
    /// </summary>
    public static class SelectorExtensions
    {
        #region Methods for Text Selectors

        /// <summary>
        /// Keeps elements that are matched by the provided selector.
        /// </summary>
        /// <param name="elements">The elements to be filtered.</param>
        /// <param name="selectorText">The CSS selector to use.</param>
        /// <returns>The filtered list of elements.</returns>
        public static IEnumerable<T> Is<T>(this IEnumerable<T> elements, String selectorText)
            where T : IElement
        {
            return elements.Filter(selectorText, true);
        }

        /// <summary>
        /// Keeps elements that are not matched by the provided selector.
        /// </summary>
        /// <param name="elements">The elements to be filtered.</param>
        /// <param name="selectorText">The CSS selector to use.</param>
        /// <returns>The filtered list of elements.</returns>
        public static IEnumerable<T> Not<T>(this IEnumerable<T> elements, String selectorText)
            where T : IElement
        {
            return elements.Filter(selectorText, false);
        }

        /// <summary>
        /// Gets the children of the provided elements. Optionally uses a CSS
        /// selector to filter the results.
        /// </summary>
        /// <param name="elements">The elements owning the children.</param>
        /// <param name="selectorText">The CSS selector to use, if any.</param>
        /// <returns>A filtered list containing the children.</returns>
        public static IEnumerable<IElement> Children(this IEnumerable<IElement> elements, String selectorText = null)
        {
            return elements.GetMany(m => m.Children, selectorText);
        }

        /// <summary>
        /// Gets the siblings of the provided elements. Optionally uses a CSS
        /// selector to filter the results.
        /// </summary>
        /// <param name="elements">The elements with siblings.</param>
        /// <param name="selectorText">The CSS selector to use, if any.</param>
        /// <returns>A filtered list containing the siblings.</returns>
        public static IEnumerable<IElement> Siblings(this IEnumerable<IElement> elements, String selectorText = null)
        {
            return elements.GetMany(m => m.Parent.ChildNodes.OfType<IElement>().Except(m), selectorText);
        }

        /// <summary>
        /// Gets the parents of the provided elements. Optionally uses a CSS
        /// selector to filter the results.
        /// </summary>
        /// <param name="elements">The elements with parents.</param>
        /// <param name="selectorText">The CSS selector to use, if any.</param>
        /// <returns>A filtered list containing the parents.</returns>
        public static IEnumerable<IElement> Parent(this IEnumerable<IElement> elements, String selectorText = null)
        {
            return elements.Get(m => m.ParentElement, selectorText);
        }

        /// <summary>
        /// Gets the following siblings of the provided elements. Optionally
        /// uses a CSS selector to filter the results.
        /// </summary>
        /// <param name="elements">The elements with siblings.</param>
        /// <param name="selectorText">The CSS selector to use, if any.</param>
        /// <returns>A filtered list containing the next siblings.</returns>
        public static IEnumerable<IElement> Next(this IEnumerable<IElement> elements, String selectorText = null)
        {
            return elements.Get(m => m.NextElementSibling, selectorText);
        }

        /// <summary>
        /// Gets the preceding siblings of the provided elements. Optionally
        /// uses a CSS selector to filter the results.
        /// </summary>
        /// <param name="elements">The elements with siblings.</param>
        /// <param name="selectorText">The CSS selector to use, if any.</param>
        /// <returns>A filtered list containing the previous siblings.</returns>
        public static IEnumerable<IElement> Previous(this IEnumerable<IElement> elements, String selectorText = null)
        {
            return elements.Get(m => m.PreviousElementSibling, selectorText);
        }

        #endregion

        #region Methods for Object Selectors

        /// <summary>
        /// Keeps elements that are matched by the provided selector.
        /// </summary>
        /// <param name="elements">The elements to be filtered.</param>
        /// <param name="selector">The CSS selector to use.</param>
        /// <returns>The filtered list of elements.</returns>
        public static IEnumerable<T> Is<T>(this IEnumerable<T> elements, ISelector selector)
            where T : IElement
        {
            return elements.Filter(selector, true);
        }

        /// <summary>
        /// Keeps elements that are not matched by the provided selector.
        /// </summary>
        /// <param name="elements">The elements to be filtered.</param>
        /// <param name="selector">The CSS selector to use.</param>
        /// <returns>The filtered list of elements.</returns>
        public static IEnumerable<T> Not<T>(this IEnumerable<T> elements, ISelector selector)
            where T : IElement
        {
            return elements.Filter(selector, false);
        }

        /// <summary>
        /// Gets the children of the provided elements. Optionally uses a CSS
        /// selector to filter the results.
        /// </summary>
        /// <param name="elements">The elements owning the children.</param>
        /// <param name="selector">The CSS selector to use, if any.</param>
        /// <returns>A filtered list containing the children.</returns>
        public static IEnumerable<IElement> Children(this IEnumerable<IElement> elements, ISelector selector = null)
        {
            return elements.GetMany(m => m.Children, selector);
        }

        /// <summary>
        /// Gets the siblings of the provided elements. Optionally uses a CSS
        /// selector to filter the results.
        /// </summary>
        /// <param name="elements">The elements with siblings.</param>
        /// <param name="selector">The CSS selector to use, if any.</param>
        /// <returns>A filtered list containing the siblings.</returns>
        public static IEnumerable<IElement> Siblings(this IEnumerable<IElement> elements, ISelector selector = null)
        {
            return elements.GetMany(m => m.Parent.ChildNodes.OfType<IElement>().Except(m), selector);
        }

        /// <summary>
        /// Gets the parents of the provided elements. Optionally uses a CSS
        /// selector to filter the results.
        /// </summary>
        /// <param name="elements">The elements with parents.</param>
        /// <param name="selector">The CSS selector to use, if any.</param>
        /// <returns>A filtered list containing the parents.</returns>
        public static IEnumerable<IElement> Parent(this IEnumerable<IElement> elements, ISelector selector = null)
        {
            return elements.Get(m => m.ParentElement, selector);
        }

        /// <summary>
        /// Gets the following siblings of the provided elements. Optionally
        /// uses a CSS selector to filter the results.
        /// </summary>
        /// <param name="elements">The elements with siblings.</param>
        /// <param name="selector">The CSS selector to use, if any.</param>
        /// <returns>A filtered list containing the next siblings.</returns>
        public static IEnumerable<IElement> Next(this IEnumerable<IElement> elements, ISelector selector = null)
        {
            return elements.Get(m => m.NextElementSibling, selector);
        }

        /// <summary>
        /// Gets the preceding siblings of the provided elements. Optionally
        /// uses a CSS selector to filter the results.
        /// </summary>
        /// <param name="elements">The elements with siblings.</param>
        /// <param name="selector">The CSS selector to use, if any.</param>
        /// <returns>A filtered list containing the previous siblings.</returns>
        public static IEnumerable<IElement> Previous(this IEnumerable<IElement> elements, ISelector selector = null)
        {
            return elements.Get(m => m.PreviousElementSibling, selector);
        }

        #endregion

        #region Helpers

        private static IEnumerable<IElement> GetMany(this IEnumerable<IElement> elements, Func<IElement, IEnumerable<IElement>> getter, ISelector selector)
        {
            if (selector == null)
            {
                selector = SimpleSelector.All;
            }

            foreach (var element in elements)
            {
                var children = getter(element);

                foreach (var child in children)
                {
                    if (selector.Match(child))
                    {
                        yield return child;
                    }
                }
            }
        }

        private static IEnumerable<IElement> GetMany(this IEnumerable<IElement> elements, Func<IElement, IEnumerable<IElement>> getter, String selectorText)
        {
            if (selectorText != null)
            {
                var selector = CreateSelector(selectorText);
                return elements.GetMany(getter, selector);
            }

            return elements.GetMany(getter, SimpleSelector.All);
        }

        private static IEnumerable<IElement> Get(this IEnumerable<IElement> elements, Func<IElement, IElement> getter, ISelector selector)
        {
            if (selector == null)
            {
                selector = SimpleSelector.All;
            }

            foreach (var element in elements)
            {
                var child = getter(element);

                while (child != null)
                {
                    if (selector.Match(child))
                    {
                        yield return child;
                        break;
                    }

                    child = getter(child);
                }
            }
        }

        private static IEnumerable<IElement> Get(this IEnumerable<IElement> elements, Func<IElement, IElement> getter, String selectorText)
        {
            if (selectorText != null)
            {
                var selector = CreateSelector(selectorText);
                return elements.Get(getter, selector);
            }

            return elements.Get(getter, SimpleSelector.All);
        }

        private static IEnumerable<IElement> Except(this IEnumerable<IElement> elements, IElement excluded)
        {
            foreach (var element in elements)
            {
                if (!Object.ReferenceEquals(element, excluded))
                {
                    yield return element;
                }
            }
        }

        private static IEnumerable<T> Filter<T>(this IEnumerable<T> elements, ISelector selector, Boolean result)
            where T : IElement
        {
            if (selector == null)
            {
                selector = SimpleSelector.All;
            }

            foreach (var element in elements)
            {
                if (selector.Match(element) == result)
                {
                    yield return element;
                }
            }
        }

        private static IEnumerable<T> Filter<T>(this IEnumerable<T> elements, String selectorText, Boolean result)
            where T : IElement
        {
            if (selectorText != null)
            {
                var selector = CreateSelector(selectorText);
                return elements.Filter(selector, result);
            }

            return elements.Filter(SimpleSelector.All, result);
        }

        private static ISelector CreateSelector(String selector)
        {
            return CssParser.Default.ParseSelector(selector);
        }

        #endregion
    }
}
