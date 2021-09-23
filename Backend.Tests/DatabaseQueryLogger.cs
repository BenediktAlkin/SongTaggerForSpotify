using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Tests
{
    public class DatabaseQueryLogger
    {
        public static DatabaseQueryLogger Instance { get; } = new DatabaseQueryLogger();
        private DatabaseQueryLogger() { }

        public int MessageCount { get; set; }
        private bool IsEnabled { get; set; }
        public void Information(string msg)
        {
            if (IsEnabled)
            {
                Log.Information(msg);
                MessageCount++;
            }
                
        }


        public class Context : IDisposable
        {
            public Context()
            {
                Instance.IsEnabled = true;
                Instance.MessageCount = 0;
            }
            public void Dispose()
            {
                Instance.IsEnabled = false;
                Instance.MessageCount = 0;
            }
        }

    }
}
