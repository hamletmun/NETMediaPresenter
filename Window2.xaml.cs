using System;
using System.Windows;
using System.Windows.Controls;

namespace NETMediaPresenter
{
    public partial class Window2 : Window
    {
        public Window2()
        {
            Closed += Window2_Closed;

            InitializeComponent();
        }

        private void Window2_Closed(object sender, EventArgs e)
        {
            MediaPlayer.Stop();
            this.Close();
        }
    }
}