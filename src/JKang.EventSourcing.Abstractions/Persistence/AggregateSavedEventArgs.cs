﻿using JKang.EventSourcing.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JKang.EventSourcing.Persistence
{
    public class AggregateSavedEventArgs : EventArgs
    {
        public AggregateSavedEventArgs(IEnumerable<IAggregateEvent> events)
        {
            Events = events.ToList().AsReadOnly();
        }

        public IEnumerable<IAggregateEvent> Events { get; }
    }
}
