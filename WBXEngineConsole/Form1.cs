using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WBXEngineConsole
{
    public partial class Form1 : Form
    {
        private bool _isRunning = false;
        WBXEngine.Manager.Assistant _assisentent = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ToggleButtons(false);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                ButtonsEnabled = false;
                if (_assisentent == null) _assisentent = new WBXEngine.Manager.Assistant();
                _assisentent.Start();

                
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            ToggleButtons(true);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            stop();
        }


        private void stop()
        {
            if(_isRunning)
            {
                ButtonsEnabled = false;
                if (_assisentent != null) _assisentent.Stop();
                ToggleButtons(false);
            }
        }

        private void ToggleButtons(bool running)
        {
            _isRunning = running;
            btnStart.Enabled = !_isRunning;
            btnStop.Enabled = _isRunning;
        }

        private bool ButtonsEnabled
        {
            set
            {
                btnStart.Enabled = value;
                btnStart.Enabled = value;

                Application.DoEvents();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stop();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            //using (WBX.MARS.Product.Item item = new WBX.MARS.Product.Item("010509", WBX.MARS.Product.Item.Columns.ItemSkuAlt))
            //{
            //    if (item.Exists) item.Quantity.Available = 5; item.Save();
            //}
        }
    }
}
