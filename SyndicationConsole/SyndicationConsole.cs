﻿using System.Threading;
using System.Diagnostics;
using System;

namespace SyndicationConsole
{
    internal class SyndicationConsole
    {
        private static void Main(string[] args)
        {
            using (Process p = Process.GetCurrentProcess())
                p.PriorityClass = ProcessPriorityClass.BelowNormal;
            var s = new SyndicateService.Service();
            s.Start();
            while (true)
                Thread.Sleep(500);
        }
    }
}