using BenchmarkDotNet.Running;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BenchmarkAsyncNotAwaitInterface>();


        }
    }
}
