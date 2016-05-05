using System;
using System.Linq;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Hapikit.Tests
{
    public class ConsoleLogger : IObserver<KeyValuePair<string, object>>
    {
        readonly ITestOutputHelper output;

        public ConsoleLogger(ITestOutputHelper output)
        {
            this.output = output;
        }

        public void Close()
        {
         //   this.output = null;
        }
        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            if (this.output != null)
            {
                this.output.WriteLine($"{value.Key} {value.Value}");
            }
        }
    }
}