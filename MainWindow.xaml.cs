using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NETMediaPresenter
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MONITORINFOEX
    {
        public int Size;
        public Rect Monitor;
        public Rect WorkArea;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
    }

    public class NativeMethods
    {
        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);
    }

    public partial class MainWindow : Window
    {
        public class Screen
        {
            public string DeviceName;
            public Rect WorkingArea;
        }

        public static List<Screen> AllScreens = new List<Screen>();
        public static Rect SelectedScreen;
        public static Window2 win2 = new Window2();

        static bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
        {
            MONITORINFOEX mi = new MONITORINFOEX();
            mi.Size = Marshal.SizeOf(typeof(MONITORINFOEX));
            NativeMethods.GetMonitorInfo(hMonitor, ref mi);
            Screen _screen = new Screen();
            _screen.DeviceName = mi.DeviceName;
            _screen.WorkingArea = mi.WorkArea;
            AllScreens.Add(_screen);
            return true;
        }

        public MainWindow()
        {
            Closed += MainWindow_Closed;
            InitializeComponent();

            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnumProc, IntPtr.Zero);

            foreach (Screen _screen in AllScreens)
            {
                ScreenList.Items.Add(_screen.DeviceName);
            }
            ScreenList.Items.Add("NONE");
            ScreenList.SelectedIndex = ScreenList.Items.Count - 1;

            ScreenList.SelectionChanged += new SelectionChangedEventHandler(ScreenList_SelectionChanged);
            MediaList.SelectionChanged += new SelectionChangedEventHandler(MediaList_SelectionChanged);
            MediaList.DragOver += new DragEventHandler(MediaList_DragOver);
            MediaList.Drop += new DragEventHandler(MediaList_Drop);
            MediaList.KeyDown += new KeyEventHandler(MediaList_KeyDown);
        }

        private void ScreenList_SelectionChanged(object sender, EventArgs e)
        {
            if (ScreenList.SelectedItem as string == "NONE") {
                win2.MediaPlayer.Stop();
                win2.Hide();
            } else {
                SelectedScreen = AllScreens[ScreenList.SelectedIndex].WorkingArea;
                win2.WindowState = WindowState.Normal;
                win2.Left = SelectedScreen.left;
                win2.Top = SelectedScreen.top;
                win2.Show();
                win2.WindowState = WindowState.Maximized;
                win2.MediaPlayer.Play();
                this.Activate();
            }
        }

        private void MediaList_DragOver(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        private void MediaList_Drop(object sender, DragEventArgs e)
        {
            string[] FileList = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string File in FileList)
                MediaList.Items.Add(File);
            StatusBar.Text = "List contains " + MediaList.Items.Count + " items";
        }
 
        private void MediaList_SelectionChanged(object sender, EventArgs e)
        {
            win2.MediaPlayer.Source = new Uri(MediaList.SelectedItem.ToString(), UriKind.Absolute);
            if(ScreenList.SelectedItem as string != "NONE")
                win2.MediaPlayer.Play();
        }

        private void MediaList_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Delete)
                MediaList.Items.Remove(MediaList.SelectedItem);
            StatusBar.Text = "List contains " + MediaList.Items.Count + " items";
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            win2.MediaPlayer.Stop();
            win2.Close();
        }
    }
}