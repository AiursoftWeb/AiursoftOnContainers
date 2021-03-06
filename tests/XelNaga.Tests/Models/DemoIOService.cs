﻿using Aiursoft.Scanner.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Aiursoft.XelNaga.Tests.Models
{
    public class DemoIOService : ITransientDependency
    {

        public int GetSomeCountSlow()
        {
            Thread.Sleep(200);
            return 3;
        }

        public async Task<int> GetSomeCountSlowAsync()
        {
            await Task.Delay(200);
            return 5;
        }
    }
}
