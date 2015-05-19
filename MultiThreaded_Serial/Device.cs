using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ESUM_Logger
{
    class Device
    {
        SerialPort port;
        private Thread thread;
        //
        DeviceName deviceName;
        DeviceComPort deviceComPort;

        //
        int baudRate;
        //
        int frameCounter = 0;
        string inDataBuffer = "";
        //
        bool isOpen = false; //port open
        public bool IsOpen { get { return isOpen; } }
        bool isSynched = false;//data stream synched
        bool IsSynched { get { return isSynched; } }

        string startPhrase;

        // note: this event is fired in the background thread
        // public event EventHandler<DataEventArgs> DataReceived;

        //object text id
        public String id { get { return deviceName.ToString() + " @ " + deviceComPort.ToString(); } }

        //Manage thread
        public void Start() { if (thread.IsAlive == false) thread.Start(); }
        public void Kill() { if (thread != null && thread.IsAlive) thread.Abort(); }

        public string Status()
        {
            string result = id + "\t / Open:" + IsOpen.ToString() + " / Synched:" + IsSynched.ToString();
            return result;
        }

        public Device(DeviceName devName, DeviceComPort port, string startPhrase, int baudRate)
        {
            this.deviceComPort = port;
            this.deviceName = devName;
            this.baudRate = baudRate;
            this.startPhrase = startPhrase;
            thread = new Thread(FireDevice);
        }

        /*
         * Instead of using a Data Event Handler we need to use a Separate Thread for each device so we can receive the data
         * asynchronously. If we don't we can only read 1 serial port only.
         */

        void FireDevice()
        {
            //
            port = new SerialPort(deviceComPort.ToString(), baudRate, Parity.None, 8, StopBits.One);
            inDataBuffer = "";
            frameCounter = 0;

            try
            {
                port.Open();
                Console.WriteLine("Opened port:" + id);
                isOpen = true;

            }
            catch
            {
                Console.WriteLine("Unable to open port:" + id);
            }


            while (isOpen)
            {
                string incomingLine;
                try
                {
                    incomingLine = port.ReadLine();
                    UseIncoming(incomingLine);
                }
                catch
                {
                    Console.WriteLine(id +"Read error");

                    //FIXME
                    //try to reconnect
                }

            }
        }

        private void UseIncoming(string incomingLine)
        {

            //if its the first line of the frame
            bool isSynchLine = incomingLine.StartsWith(startPhrase);

            //raise synched flag
            if (!isSynched && isSynchLine)
            {
                isSynched = true;
                Console.WriteLine("Synched: " + id);

                //beep on synch
                if (Util.DoBeeps)
                Util.BeepSynch((int)(deviceName));
            }

            //manage data
            if (isSynched == true)
            {
                if (isSynchLine)
                {
                    frameCounter++;
                    //save buffer data to file if it reached the maximum amount of data frames
                    if (frameCounter == Util.NumOfFramesToExport)
                    {
                        //save data
                        Util.ExportText(deviceName.ToString(), inDataBuffer);
                        //clear buffer and reset counter
                        frameCounter = 0;
                        inDataBuffer = "";
                    }
                    // Console.WriteLine(id + " New Frame");
                    //start new line with timestamp
                    inDataBuffer += "\n" + Util.GetTimeStamp() + ",";
                }

                //remove new line from input line
                //incomingLine = incomingLine.Replace(System.Environment.NewLine, ","); // <-- this does not seem to work
                incomingLine = incomingLine.Substring(0, incomingLine.Length - 2);

                //add data to buffer and separate with comma
                inDataBuffer += incomingLine + ",";
            }

            //print input in console
            if (Util.monitorInput)
            {
                Console.WriteLine(id + ">>" + incomingLine);
            }
        }


        //DEPRECATED // Single threaded
        //private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    Thread.Sleep(100);
        //    string readLine = port.ReadLine();
        //    Console.WriteLine(deviceComPort + ":" + readLine);
        //}

    }
}
