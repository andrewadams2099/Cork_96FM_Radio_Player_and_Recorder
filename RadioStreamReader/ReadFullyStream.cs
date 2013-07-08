using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using NAudio.Wave;
using System.Timers;

namespace RadioStreamReader
{
    public class ReadFullyStream : Stream
    {
        private Stream sourceStream;
        private long pos; // psuedo-position
        private byte[] readAheadBuffer;
        private int readAheadLength;
        private int readAheadOffset;

        private WaveFileWriter m_waveFileWriter;
        private static ReadFullyStream instance;
        private System.Timers.Timer checkForTime;

        private ReadFullyStream() { }

        // Make the ReadFullyStream a singleton class - User will only listen to one radio station at a time anyway.
        // Also it will allow our Quartz Job to call the writeWaveFile method.
        public static ReadFullyStream Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ReadFullyStream();
                }
                return instance;
            }
        }

        public void setStream(Stream sourceStream)
        {
            this.sourceStream = sourceStream;
            this.readAheadBuffer = new byte[4096];
        }
        
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new InvalidOperationException();
        }

        public override long Length
        {
            get { return pos; }
        }

        public override long Position
        {
            get
            {
                return pos;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public void setWaveFileWriter(WaveFileWriter wfw)
        {
            this.m_waveFileWriter = wfw;
        }

        public void createWaveFile(String filename, int duration)
        {
            WaveFormat wf = new Mp3WaveFormat(44100, 2, 418, 128000);
            
            // Save to file
            WaveFileWriter wfw = new WaveFileWriter(filename, wf);
            setWaveFileWriter(wfw);

            // Create interval with duration in milliseconds
            double interval = duration * 60 * 1000; // milliseconds to one hour

            // Create a timer to close the wave file after the duration has elapsed
            checkForTime = new System.Timers.Timer(interval);
            checkForTime.Elapsed += new ElapsedEventHandler(duration_Elapsed);
            checkForTime.Enabled = true;
        }

        private void duration_Elapsed(object sender, ElapsedEventArgs e)
        {
            closeWaveFile(); // Close wave file
            checkForTime.Enabled = false; // disable the timer
            checkForTime = null; // set reference to null
        }

        private void closeWaveFile()
        {
            try
            {
                Debug.WriteLine("Closing wave file...");
                this.m_waveFileWriter.Close();
                setWaveFileWriter(null);
            }
            catch
            {
                Debug.WriteLine("exception when trying to close writer");
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            while (bytesRead < count)
            {
                int readAheadAvailableBytes = readAheadLength - readAheadOffset;
                int bytesRequired = count - bytesRead;

                if (readAheadAvailableBytes > 0)
                {
                    int toCopy = Math.Min(readAheadAvailableBytes, bytesRequired);
                    Array.Copy(readAheadBuffer, readAheadOffset, buffer, offset + bytesRead, toCopy);

                    // Write the contents of the readAheadBuffer to the file
                    if (m_waveFileWriter != null)
                    {
                        try
                        {
                            m_waveFileWriter.Write(readAheadBuffer, readAheadOffset, toCopy);
                        }
                        catch
                        {
                            Debug.WriteLine("Write Exeception Caught");
                        }
                    }

                    bytesRead += toCopy;
                    readAheadOffset += toCopy;
                }
                else
                {
                    readAheadOffset = 0;
                    readAheadLength = sourceStream.Read(readAheadBuffer, 0, readAheadBuffer.Length);
                    //Debug.WriteLine(String.Format("Read {0} bytes (requested {1})", readAheadLength, readAheadBuffer.Length));
                    if (readAheadLength == 0)
                    {
                        break;
                    }
                }
            }
            this.pos += bytesRead;

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}