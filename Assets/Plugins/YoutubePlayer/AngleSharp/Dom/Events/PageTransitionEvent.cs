﻿namespace AngleSharp.Dom.Events
{
    using AngleSharp.Attributes;
    using System;

    /// <summary>
    /// Represents a page transition event argument.
    /// </summary>
    [DomName("PageTransitionEvent")]
    public class PageTransitionEvent : Event
    {
        #region ctor

        /// <summary>
        /// Creates a new event.
        /// </summary>
        public PageTransitionEvent()
        {
        }

        /// <summary>
        /// Creates a new event and initializes it.
        /// </summary>
        /// <param name="type">The type of the event.</param>
        /// <param name="bubbles">If the event is bubbling.</param>
        /// <param name="cancelable">If the event is cancelable.</param>
        /// <param name="persisted">Indicates if a webpage is loading from a cache.</param>
        [DomConstructor]
        [DomInitDict(offset: 1, optional: true)]
        public PageTransitionEvent(String type, Boolean bubbles = false, Boolean cancelable = false, Boolean persisted = false)
        {
            Init(type, bubbles, cancelable, persisted);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets if a webpage is loading from a cache..
        /// </summary>
        [DomName("persisted")]
        public Boolean IsPersisted
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the event.
        /// </summary>
        /// <param name="type">The type of the event.</param>
        /// <param name="bubbles">If the event is bubbling.</param>
        /// <param name="cancelable">If the event is cancelable.</param>
        /// <param name="persisted">Indicates if a webpage is loading from a cache.</param>
        [DomName("initPageTransitionEvent")]
        public void Init(String type, Boolean bubbles, Boolean cancelable, Boolean persisted)
        {
            Init(type, bubbles, cancelable);
            IsPersisted = persisted;
        }

        #endregion
    }
}
