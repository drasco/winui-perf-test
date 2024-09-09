using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinGDSTesterFramework
{
    public partial class Form1 : Form
    {
        private PerformanceCounter backgroundCS;
        private PerformanceCounter switchCounter;
        private Stopwatch stopwatch;
        private Process us;
        private long lastCpuTimeSample;
        private float lastGCSSample;
        private long lastTimeSample;
        private ulong lastCyclesSample;
        private ManagementObject usPerf;

        //private IntPtr originalAffinity;

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool QueryProcessCycleTime(IntPtr ProcessHandle, out ulong CycleTime);
        public Form1()
        {
            InitializeComponent();
            Process.GetCurrentProcess().Refresh();
            //Doesnt work, just gives 15, which is the first 4 cores.
            //originalAffinity = Process.GetCurrentProcess().ProcessorAffinity;
            backgroundCS = new PerformanceCounter("System", "Context Switches/sec", "");
            backgroundCS.NextSample();
            switchCounter = new PerformanceCounter("System", "Context Switches/sec", "");//slow call.
            stopwatch = new Stopwatch();
            us = Process.GetCurrentProcess();
            var mos = new ManagementObjectSearcher("SELECT * FROM Win32_Process where Handle = " + us.Id);
            foreach (ManagementObject obj in mos.Get()) { usPerf = obj; break; }
            //usPerf.Get();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var start = DateTime.Now;
            var rnd = new Random();
            for (var i = 0; i < 15000; i++)
            {
                listBox1.Items.Add(rnd.NextDouble().ToString());
            }

            button1.Text = (DateTime.Now - start).ToString();
        }

        
        private void SwitchTest(int iters, int cores)
        {
            var m = cores;
            var t = m*2;
            var muts = new List<Mutex>();
            var ts = new List<Thread>();
            for (var mi = 0; mi < m; mi++)
                muts.Add(new Mutex());
            for(var i =  0; i < t; i++)
            {
                var nt = new Thread((param) =>
                {
                    var tid = (int)param;
                    var mut = muts[tid % m];
                    for (var j = 0; j < iters; j++)
                    {
                        mut.WaitOne();
                        mut.ReleaseMutex();
                    }
                });
                nt.Priority = ThreadPriority.AboveNormal;
                ts.Add(nt);
            }

            ulong beginCycles, endCycles;

            //these values arent refreshing.
            var mos = new ManagementObjectSearcher("SELECT * FROM Win32_Process where Handle = " + us.Id);
            foreach (ManagementObject obj in mos.Get()) { usPerf = obj; break; }
            var beginCpuTime = (ulong)usPerf.Properties["KernelModeTime"].Value + (ulong)usPerf.Properties["UserModeTime"].Value;
            stopwatch.Restart();
            switchCounter.NextValue();
            QueryProcessCycleTime(us.Handle, out beginCycles);
            for (var i = 0; i < t; i++)
                ts[i].Start(i);
            for (var i = 0; i < t; i++)
                ts[i].Join();

            //Dont do this under the debugger.
            //var k= 0;
            //for(ulong p = 0; p < (ulong)int.MaxValue*3; p++) { k++; }
            
            QueryProcessCycleTime(us.Handle, out endCycles);
            lastGCSSample = switchCounter.NextValue();
            lastTimeSample = stopwatch.ElapsedMilliseconds;

            mos = new ManagementObjectSearcher("SELECT * FROM Win32_Process where Handle = " + us.Id);
            foreach (ManagementObject obj in mos.Get()) { usPerf = obj; break; }
            lastCpuTimeSample = (long)((ulong)usPerf.Properties["KernelModeTime"].Value + (ulong)usPerf.Properties["UserModeTime"].Value) - (long)beginCpuTime;
            
            lastCyclesSample = endCycles - beginCycles;

            for (var mi = 0; mi < m; mi++)
                muts[mi].Dispose();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            //GC.TryStartNoGCRegion( 50 * 1024 *1024 );

            var start = DateTime.Now;            
            ResultsList.Items.Clear();
            stopwatch.Start();

            var cores = (int)numericUpDown1.Value;
            
            var procTime = us.TotalProcessorTime;            

            Action<int> DoTest = delegate (int iters)
            {
                SwitchTest( iters, cores);

                //This figure is some how half as much as it should be when we select the first 4 CPUs (on a 5600)
                var cpuPercent = (lastCpuTimeSample / (long)lastTimeSample)/100.0;

                var globalCSpersec = (long)lastGCSSample;
                
                //There were 2 threads runing a loop of 'iter' count, so total calls was 2x
                iters = iters * 2 * cores;

                var totalGCS = (globalCSpersec * lastTimeSample) /1000;

                var listItem = new ListViewItem( iters/1000 + "k");
                listItem.SubItems.Add(globalCSpersec.ToString("#,##0"));
                listItem.SubItems.Add((iters / lastTimeSample).ToString("#,##0"));
                listItem.SubItems.Add((lastCyclesSample / (ulong)iters).ToString("#,##0"));
                listItem.SubItems.Add(((double)totalGCS/iters).ToString("#,##0.###"));
                listItem.SubItems.Add(cpuPercent.ToString("0.###"));
                ResultsList.Items.Add(listItem);
                
            };

            DoTest(50000);
            DoTest(200000);
            DoTest(50000);
            DoTest(500000);
            DoTest(50000);

            button2.Text = (DateTime.Now - start).ToString();

            switchCounter.Dispose();

            //GC.EndNoGCRegion();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)1;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var cpus = Environment.ProcessorCount;
            var mask = (Int64)Math.Ceiling( Math.Pow(2, cpus)) - 1;
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)mask;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            labelBackgroundCS.Text = "Background CS/s: " + backgroundCS.NextValue().ToString("#,##0");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)0xF;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var cpus = Environment.ProcessorCount;
            var bits = Math.Pow(2, cpus - 1) + Math.Pow(2, cpus - 2) + Math.Pow(2, cpus - 3) + Math.Pow(2, cpus - 4);
            var mask = (Int64)Math.Ceiling( bits);
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)mask;
        }
    }
}
