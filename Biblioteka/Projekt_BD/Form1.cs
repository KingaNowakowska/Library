using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Projekt_BD
{
    public partial class Form1 : Form
    {
        private readonly Dictionary<RadioButton, Button> radioButtonButtonPairs;
        private LoginForm loginForm;
        private DBConnect db;
        public Form1(string username, string password)
        {

            InitializeComponent();

            db = new DBConnect("localhost", "biblioteka", username, password);
            // Initialize collection and pair radio buttons with buttons
            radioButtonButtonPairs = new Dictionary<RadioButton, Button>
            {
                { radioButton1, button1 },
                { radioButton2, button2 },
                { radioButton3, button3 },
                { radioButton4, button4 },
                { radioButton6, button6 },
                { radioButton7, button7 },
                { radioButton8, button8 }
            };

            // Add CheckedChanged event handler for each radio button
            foreach (var pair in radioButtonButtonPairs)
            {
                pair.Key.CheckedChanged += RadioButton_CheckedChanged;
            }
        }

        private void OpenLoginForm()
        {
            loginForm = new LoginForm();
            DialogResult result = loginForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                string username = loginForm.Username;
                MessageBox.Show($"Logged in as: {username}");
            }
        }

        private void buttonOpenLoginForm_Click(object sender, EventArgs e)
        {
            OpenLoginForm();
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null && radioButton.Checked)
            {
                // Reset all buttons' Enabled state
                foreach (var button in radioButtonButtonPairs.Values)
                {
                    button.Enabled = false;
                }

                // Enable button associated with selected radio button
                if (radioButtonButtonPairs.TryGetValue(radioButton, out Button associatedButton))
                {
                    associatedButton.Enabled = true;
                }
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                // Perform action based on which button was clicked
                if (button == button1)
                {
                    string title = textBox3.Text.Trim();
                    if (long.TryParse(textBox1.Text, out long pesel) && !string.IsNullOrEmpty(title))
                    {
                        db.BorrowBook(pesel, title);
                    }
                    else
                    {
                        MessageBox.Show("Error! Check input data.", "Message");
                    }
                    ClearTextBoxes(textBox1, textBox3);
                }
                else if (button == button2)
                {
                    if (int.TryParse(textBox5.Text, out int id) && !string.IsNullOrEmpty(textBox7.Text))
                    {
                        db.ReturnBook(id, textBox7.Text);
                    }
                    else
                    {
                        MessageBox.Show("Error! Check input data.", "Message");
                    }
                    ClearTextBoxes(textBox5, textBox7);
                }
                else if (button == button3)
                {
                    if (int.TryParse(textBox11.Text, out int pesel) && !string.IsNullOrEmpty(textBox9.Text) && !string.IsNullOrEmpty(textBox10.Text))
                    {
                        db.Insert(textBox9.Text, textBox10.Text, pesel);
                    }
                    else
                    {
                        MessageBox.Show("Error! Check input data.", "Message");
                    }
                    ClearTextBoxes(textBox9, textBox10, textBox11);
                }
                else if (button == button4)
                {
                    if (int.TryParse(textBox14.Text, out int pesel))
                    {
                        db.Delete(pesel);
                    }
                    else
                    {
                        MessageBox.Show("Error! Check input data.", "Message");
                        textBox14.Text = "";
                    }
                }
                else if (button == button6)
                {
                    List<Task> tasks = db.Select();
                    listBox1.Items.Clear(); // Clear before add new elements

                    foreach (Task task in tasks)
                    {
                        string itemText = $"ID: {task.Id}, Name: {task.Name}, Surname: {task.Surname}, PESEL: {task.Pesel}, Count of books: {task.CountOfBooks}, Titles: {task.Title}";
                        listBox1.Items.Add(itemText);
                    }

                }
                else if (button == button7)
                {
                    if (int.TryParse(textBox18.Text, out int newPesel) && int.TryParse(textBox19.Text, out int oldPesel))
                    {
                        db.Update(textBox16.Text, textBox17.Text, newPesel, oldPesel);
                    }
                    else
                    {
                        MessageBox.Show("Error! Check input data.", "Message");
                        ClearTextBoxes(textBox16, textBox17, textBox18, textBox19);
                    }
                }
                else if (button == button8)
                {

                    int count = db.Count();
                    label22.Text = count.ToString();
                }
            }
        }

        // Form1
        private void Form1_Load(object sender, EventArgs e)
        {
            
        }


        private void ClearTextBoxes(params TextBox[] textBoxes)
        {
            foreach (TextBox textBox in textBoxes)
            {
                textBox.Text = "";
            }
        }
    }
}

