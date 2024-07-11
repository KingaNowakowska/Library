using System;
using System.Windows.Forms;

namespace Projekt_BD
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool isLoggedIn = false;
            string username = string.Empty;
            string password = string.Empty;

            while (!isLoggedIn)
            {
                using (LoginForm loginForm = new LoginForm())
                {
                    DialogResult result = loginForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        username = loginForm.Username;
                        password = loginForm.Password;

                        DBConnect db = new DBConnect("localhost", "biblioteka", username, password);

                        // Sprawdzanie poprawności logowania
                        if (db.CheckLogin(username, password))
                        {
                            isLoggedIn = true;
                            Application.Run(new Form1(username, password));
                        }
                        else
                        {
                            MessageBox.Show("Invalid username or password. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        // Cancelled log in
                        return;
                    }
                }
            }
        }
    }
}


