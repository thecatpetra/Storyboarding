﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MapsetParser.statics
{
    public static class EventStatic
    {
        /// <summary> Called whenever loading of something is started. </summary>
        public static Func<string, Task> OnLoadStart { get; set; } = message => { return Task.CompletedTask; };

        /// <summary> Called whenever loading of something is completed. </summary>
        public static Func<string, Task> OnLoadComplete { get; set; } = message => { return Task.CompletedTask; };
    }
}
