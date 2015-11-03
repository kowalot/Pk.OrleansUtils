/*
Project Orleans Cloud Service SDK ver. 1.0
 
Copyright (c) Microsoft Corporation
 
All rights reserved.
 
MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
associated documentation files (the ""Software""), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS
OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Orleans;
using Pk.OrleansUtils.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pk.OrleansUtils.TestClient
{
    /// <summary>
    /// Orleans test silo host
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {

            ConsoleKeyInfo key;
            int activeClientWorkers = 0;
            int maxClientWorkers = 16;
            var workers = new TestWorker[maxClientWorkers];
            for (int i = 0; i < workers.Length; i++)
                workers[i] = new TestWorker(i,"DevTestClientConfiguration.xml");

            var clientVersion = typeof(IAccount).Module.Assembly.GetName().Version.ToString();
            do
            {
                do
                {
                    for (int i = 0; i < workers.Length; i++)
                        workers[i].Active = (i < activeClientWorkers) ? true : false;
                    Console.Clear();
                    Console.WriteLine($"Active client threads:{activeClientWorkers.ToString().PadLeft(8)} Client ver:{clientVersion}");
                    for (int i = 0; i < workers.Length; i++)
                    {
                        var w = workers[i];
                        Console.WriteLine($"{w.Id.ToString().PadRight(4)} Active:{w.Active.ToString().PadLeft(8)} IterTime:{w.IterationTime.ToString().PadLeft(8)}ms CallsCount:{w.CallsCount.ToString().PadLeft(6)} Ver:{w.VersionString.PadLeft(10)}");
                    }
                    Thread.Sleep(100);
                } while (!Console.KeyAvailable);
                key = Console.ReadKey();
                while (Console.KeyAvailable)//clear key buffer
                    Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (activeClientWorkers<maxClientWorkers) activeClientWorkers++;
                        break;
                    case ConsoleKey.DownArrow:
                        if (activeClientWorkers>0) activeClientWorkers--;
                        break;
                }

            } while (key.Key != ConsoleKey.Escape);

        }
    }

}
