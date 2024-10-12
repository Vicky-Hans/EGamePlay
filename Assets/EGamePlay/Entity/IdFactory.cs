﻿using System;
using System.Linq;

namespace EGamePlay
{
    public static class IdFactory
    {
        private static long BaseRevertTicks { get; set; }
        public static long NewInstanceId()
        {
            if (BaseRevertTicks == 0)
            {
                var now = DateTime.UtcNow.Ticks;
                var str = now.ToString().Reverse();
                BaseRevertTicks = long.Parse(string.Concat(str));
            }
            BaseRevertTicks++;
            return BaseRevertTicks;
        }
    }
}