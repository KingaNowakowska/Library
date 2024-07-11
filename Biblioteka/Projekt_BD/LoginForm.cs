using System;
using System.Windows.Forms;

namespace Projekt_BD
{
    public partial class LoginForm : Form
    {
        public string Username
        {
            get { return textBox1.Text.Trim(); }
            set { textBox1.Text = value; }
        }

        public string Password
        {
            get { return maskedTextBox1.Text.Trim(); }
            set { maskedTextBox1.Text = value; }
        }

        public LoginForm()
        {
            InitializeComponent();
            this.button1.Click += new System.EventHandler(this.btnLogin_Click);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();
            string password = maskedTextBox1.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

    }
}

