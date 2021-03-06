﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Specialized;
using System.Threading;
using System.Diagnostics;
using CUE.NET;
using CUE.NET.Devices.Keyboard;
using CUE.NET.Devices.Keyboard.Enums;
using CUE.NET.Devices.Generic.Enums;
using CUE.NET.Exceptions;
using CUE.NET.Gradients;
using CUE.NET.Brushes;

namespace HDDLED
{
    public partial class InvisibleForm : Form
    {
        #region Global Variables
        NotifyIcon hddNotifyIcon;
        Icon busyIcon;
        Icon idleIcon;
        CorsairKeyboard keyboard;
        Thread hddInfoWorkerThread;
        #endregion

        #region Main Form (entry point)
        /// <summary>
        /// 
        /// </summary>
        public InvisibleForm()
        {
            InitializeComponent();

            // Load icons from files into objects
            busyIcon = new Icon("HDD_Busy.ico");
            idleIcon = new Icon("HDD_Idle.ico");

      

            // Create notify icons and assign idle icon and show it
            hddNotifyIcon = new NotifyIcon();
            hddNotifyIcon.Icon = idleIcon;
            hddNotifyIcon.Visible = true;

            // Create all context menu items and add them to notification tray icon
            MenuItem progNameMenuItem = new MenuItem("Hard Drive LED v1.0 BETA by: Barnacules");
            MenuItem breakMenuItem = new MenuItem("-");
            MenuItem quitMenuItem = new MenuItem("Quit");
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(progNameMenuItem);
            contextMenu.MenuItems.Add(breakMenuItem);
            contextMenu.MenuItems.Add(quitMenuItem);
            hddNotifyIcon.ContextMenu = contextMenu;

            // Wire up quit button to close application
            quitMenuItem.Click += quitMenuItem_Click;

            // 
            //  Hide the form because we don't need it, this is a notification tray application
            //
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            try
            {
                CueSDK.Initialize();
                Debug.WriteLine("Initialized with " + CueSDK.LoadedArchitecture + "-SDK");
                
                keyboard = CueSDK.KeyboardSDK;
                if (keyboard == null)
                {
                    throw new WrapperException("No keyboard found");
                }

                keyboard.UpdateMode = UpdateMode.Continuous;
                keyboard.UpdateFrequency = 1f / 30f;
            }
            catch (CUEException ex)
            {
                Debug.WriteLine("CUE Exception! ErrorCode: " + Enum.GetName(typeof(CorsairError), ex.Error));
            }
            catch (WrapperException ex)
            {
                Debug.WriteLine("Wrapper Exception! Message:" + ex.Message);
            }





            //Console.WriteLine("rainbow-test");

            // Create an simple horizontal rainbow containing two times the full spectrum
            //RainbowGradient rainbowGradient = new RainbowGradient(0, 720);

            //// Add the rainbow to the keyboard and perform an initial update
            //keyboard.Brush = new LinearGradientBrush(rainbowGradient);
            //keyboard.Update();

            //// Let the rainbow move around for 10 secs
            //for (int i = 0; i < 10; i++)
            //{
            //    rainbowGradient.StartHue += 10f;
            //    rainbowGradient.EndHue += 10f;
            //    keyboard.Update();
            //    Thread.Sleep(10);
            //}

            //GradientStop[] gradientStops =
            //{
            //    new GradientStop(0f, Color.Blue),
            //    new GradientStop(1f, Color.Red)
            //};
            //LinearGradient blueToRedGradient = new LinearGradient(gradientStops);
            //keyboard.Brush = new SolidColorBrush(Color.Transparent);
            //keyboard.Update();

            //IBrush brush = new SolidColorBrush(Color.White);
            //keyboard.Brush = brush;
            //keyboard.Update();



        //    keyboard.Brush = new myBrush(new PointF(1f, 1f), new PointF(1f, 1f), blueToRedGradient);

            //  keyboard.Update();


            //keyboard.Brush = new SolidColorBrush(Color.Transparent);
            //keyboard.Update();


            //IBrush brush = new SolidColorBrush(Color.White);
            //keyboard.Brush = brush;
            //keyboard.Update();

            // Start worker thread that pulls HDD activity
            hddInfoWorkerThread = new Thread(new ThreadStart(HddActivityThread));
            hddInfoWorkerThread.Start();
        }
        #endregion

        #region Context Menu Event Handlers
        /// <summary>
        /// Close the application on click of 'quit' button on context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void quitMenuItem_Click(object sender, EventArgs e)
        {
            hddInfoWorkerThread.Abort();
            hddNotifyIcon.Dispose();
            this.Close();
        }
        #endregion

        #region Hard drive activity threads
        /// <summary>
        /// This is the thread that pulls the HDD for activity and updates the notification icon
        /// </summary>
        public void HddActivityThread()
        {
            ManagementClass driveDataClass = new ManagementClass("Win32_PerfFormattedData_PerfDisk_PhysicalDisk");
            int col = 5;
            int row = 21;


            GradientStop[] gradientStops =
            {
                new GradientStop(0f, Color.Blue),
                new GradientStop(1f, Color.Red)
            };
            LinearGradient blueToRedGradient = new LinearGradient(gradientStops);

            myBrush brush = new myBrush(new PointF(1f, 1f), new PointF(1f, 1f), blueToRedGradient);

            try
            {
                // Main loop where all the magic happens
                while (true)
                {

                    col+=15;

                    if (col > 350)
                    {
                        col = 0;
                        row+=18;

                        if (row > 164)
                        {
                            row = 31;
                        }
                    }

                   // Console.WriteLine("{0}", col);
                  
                    brush.EndPoint = new PointF(col, row);
                    keyboard.Brush = brush;
                   // keyboard.Update();


                    //// Connect to the drive performance instance 
                    //ManagementObjectCollection driveDataClassCollection = driveDataClass.GetInstances();
                    //foreach( ManagementObject obj in driveDataClassCollection)
                    //{
                    //    // Only process the _Total instance and ignore all the indevidual instances
                    //    if( obj["Name"].ToString() == "_Total")
                    //    {
                    //        if( Convert.ToUInt64(obj["DiskBytesPersec"]) > 0 )
                    //        {
                    //            // Show busy icon
                    //            //hddNotifyIcon.Icon = busyIcon;
                    //            keyboard[CorsairKeyboardKeyId.PauseBreak].Led.Color = Color.Red;
                    //         //   keyboard.Update();
                    //        }
                    //        else
                    //        {
                    //            // Show idle icon
                    //            //hddNotifyIcon.Icon = idleIcon;
                    //            keyboard[CorsairKeyboardKeyId.PauseBreak].Led.Color = Color.Green;
                    //         //   keyboard.Update();
                    //        }
                    //    }
                    //}

                    // Sleep for 10th of millisecond 
                   Thread.Sleep(20);
                }
            } catch( ThreadAbortException tbe )
            {
                driveDataClass.Dispose();
                // Thead was aborted
            }
        }
        #endregion
    }
}
