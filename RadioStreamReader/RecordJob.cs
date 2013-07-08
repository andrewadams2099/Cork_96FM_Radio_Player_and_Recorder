using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Collection;
using Quartz;
using System.Diagnostics;

namespace RadioStreamReader
{
    public class RecordJob : IJob
    {
        private ReadFullyStream rfs;

        public RecordJob()
        {
            this.rfs = ReadFullyStream.Instance; // Tie reference to singleton class
        }

        public void Execute(IJobExecutionContext context)
        {
            Debug.WriteLine("RecordJob is executing.");
            Debug.WriteLine("Creating wave file...");

            // Create filename to use for this hours news, weather and cash call
            DateTime now = DateTime.Now;
            string format = "MMM_d_yyyy_hhtt";    // Use this format
            String filename = "STREAM_96FM_" + now.ToString(format).ToUpper() + ".wav";
            Debug.WriteLine("Filename: " + filename);

            // Call the createWaveFile method with a duration of 15 minutes
            rfs.createWaveFile(filename, 15);
        }
    }
}
