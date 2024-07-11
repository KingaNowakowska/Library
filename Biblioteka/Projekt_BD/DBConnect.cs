using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Projekt_BD
{
    public class DBConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;
        private string connectionString;
        private List<Task> tasksList = new List<Task>();
        private List<Book> booksList = new List<Book>();



        public DBConnect(string server, string database, string uid, string password)
        {
            this.server = server;
            this.database = database;
            this.uid = uid;
            this.password = password;
            Initialize(server, database, uid, password);

        }

        private void Initialize(string server, string database, string uid, string password)
        {
            connectionString = $"SERVER={server};DATABASE={database};UID={uid};PASSWORD={password};";
            connection = new MySqlConnection(connectionString);
        }



        public void SetLoginCredentials(string newUid, string newPassword)
        {
            uid = newUid;
            password = newPassword;

            // Update connection string with new credentials
            connection.ConnectionString = $"SERVER={server}; DATABASE={database}; UID={uid}; PASSWORD={password};";

        }



        // Check correct entire datas during log in
        public bool CheckLogin(string login, string password)
        {
            login = login.Trim();
            password = password.Trim();

            SetLoginCredentials(login, password); // Update credentials

            bool loginSuccessful = OpenConnection();

            if (loginSuccessful)
            {
                
                EnsureDatabaseExists();
            }

            return loginSuccessful;
        }

        private void EnsureDatabaseExists()
        {
            string tempConnectionString = $"SERVER={server};UID={uid};PASSWORD={password};";
            using (MySqlConnection tempConnection = new MySqlConnection(tempConnectionString))
            {
                try
                {
                    tempConnection.Open();
                    string query = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @databaseName;";
                    using (MySqlCommand cmd = new MySqlCommand(query, tempConnection))
                    {
                        cmd.Parameters.AddWithValue("@databaseName", database);
                        var result = cmd.ExecuteScalar();
                        if (result == null)
                        {
                            // Baza danych nie istnieje, więc ją tworzymy
                            string createDatabaseQuery = $"CREATE DATABASE {database};";
                            using (MySqlCommand createCmd = new MySqlCommand(createDatabaseQuery, tempConnection))
                            {
                                createCmd.ExecuteNonQuery();
                                MessageBox.Show($"Baza danych \"{database}\" nie istniała. Utworzono pomyślnie.");
                            }
                        }

                        // Połącz się z nowo utworzoną bazą danych i utwórz tabele, jeśli nie istnieją
                        string dbConnectionString = $"SERVER={server};DATABASE={database};UID={uid};PASSWORD={password};";
                        using (MySqlConnection dbConnection = new MySqlConnection(dbConnectionString))
                        {
                            dbConnection.Open();
                            string createTasksTableQuery = @"
                CREATE TABLE IF NOT EXISTS tasks (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    pesel BIGINT NOT NULL,
                    imie VARCHAR(50) NOT NULL,
                    nazwisko VARCHAR(50) NOT NULL,
                    tytul VARCHAR(100),
                    ilosc_ksiazek INT DEFAULT 0
                );";
                            string createBooksTableQuery = @"
                CREATE TABLE IF NOT EXISTS books (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    tytul VARCHAR(100) NOT NULL,
                    autor VARCHAR(100),
                    stan INT DEFAULT 0
                );";
                            using (MySqlCommand createTasksCmd = new MySqlCommand(createTasksTableQuery, dbConnection))
                            {
                                createTasksCmd.ExecuteNonQuery();
                            }
                            using (MySqlCommand createBooksCmd = new MySqlCommand(createBooksTableQuery, dbConnection))
                            {
                                createBooksCmd.ExecuteNonQuery();
                            }

                            // Dodajemy książki do tabeli books
                            string insertBooksQuery = @"
                INSERT INTO books (tytul, autor, stan) VALUES
                ('Book 1', 'Author 1', 4),
                ('Book 2', 'Author 2', 2),
                ('Book 3', 'Author 3', 6)
                ON DUPLICATE KEY UPDATE stan = VALUES(stan);";
                            using (MySqlCommand insertBooksCmd = new MySqlCommand(insertBooksQuery, dbConnection))
                            {
                                insertBooksCmd.ExecuteNonQuery();
                            }

                            // Dodaj książki jako obiekty Book
                            booksList.Add(new Book(1, "Book 1", "Author 1", 4));
                            booksList.Add(new Book(2, "Book 2", "Author 2", 2));
                            booksList.Add(new Book(3, "Book 3", "Author 3", 6));
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Database Error: {ex.Message}");
                }
            }
        }


        private bool OpenConnection()
        {
            SetLoginCredentials(uid, password);
            try
            {
                EnsureDatabaseExists();
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Database Error: {ex.Message}");
                return false;
            }
        }

        
        public void Insert(string firstName, string lastName, long pesel)
        {
            SetLoginCredentials(uid, password); 

            string query = "INSERT INTO tasks (pesel, imie, nazwisko, tytul, ilosc_ksiazek) VALUES (@pesel, @imie, @nazwisko, '', 0);";

            if (OpenConnection())
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@pesel", pesel);
                        cmd.Parameters.AddWithValue("@imie", firstName);
                        cmd.Parameters.AddWithValue("@nazwisko", lastName);
                        cmd.ExecuteNonQuery();

                        // get the ID of the newly added user
                        int id = (int)cmd.LastInsertedId;

                        // Add the new user to the tasksList
                        Task newTask = new Task(id, pesel, firstName, lastName, "", 0);
                        tasksList.Add(newTask);

                        MessageBox.Show("User added successfully!");
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Database Error: {ex.Message}");
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        public void Update(string firstName, string lastName, long newPesel, long oldPesel)
        {
            SetLoginCredentials(uid, password); 

            string countQuery = "SELECT COUNT(*) FROM tasks WHERE pesel=@oldPesel;";
            string updateQuery = "UPDATE tasks SET imie=@imie, nazwisko=@nazwisko, pesel=@newPesel WHERE pesel=@oldPesel;";

            if (OpenConnection())
            {
                try
                {
                    MySqlCommand countCmd = new MySqlCommand(countQuery, connection);
                    countCmd.Parameters.AddWithValue("@oldPesel", oldPesel);
                    int count = Convert.ToInt32(countCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@imie", firstName);
                            updateCmd.Parameters.AddWithValue("@nazwisko", lastName);
                            updateCmd.Parameters.AddWithValue("@newPesel", newPesel);
                            updateCmd.Parameters.AddWithValue("@oldPesel", oldPesel);
                            updateCmd.ExecuteNonQuery();

                            Task updatedTask = tasksList.Find(t => t.Pesel == oldPesel);
                            if (updatedTask != null)
                            {
                                updatedTask.Pesel = newPesel;
                                updatedTask.Name = firstName;
                                updatedTask.Surname = lastName;
                            }

                            MessageBox.Show("User updated successfully!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("User does not exist!");
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Database Error: {ex.Message}");
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        public void Delete(long pesel)
        {
            SetLoginCredentials(uid, password); 

            string countQuery = "SELECT COUNT(*) FROM tasks WHERE pesel=@pesel;";
            string deleteQuery = "DELETE FROM tasks WHERE pesel=@pesel;";

            if (OpenConnection())
            {
                try
                {
                    MySqlCommand countCmd = new MySqlCommand(countQuery, connection);
                    countCmd.Parameters.AddWithValue("@pesel", pesel);
                    int count = Convert.ToInt32(countCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        using (MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection))
                        {
                            deleteCmd.Parameters.AddWithValue("@pesel", pesel);
                            deleteCmd.ExecuteNonQuery();

                            // Remove task from list
                            Task taskToRemove = tasksList.Find(t => t.Pesel == pesel);
                            if (taskToRemove != null)
                            {
                                tasksList.Remove(taskToRemove);
                            }

                            MessageBox.Show("User deleted successfully!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("User does not exist!");
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Database Error: {ex.Message}");
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        
        public void BorrowBook(long pesel, string title)
        {
            SetLoginCredentials(uid, password); 

            string countQuery = "SELECT COUNT(*) FROM tasks WHERE pesel=@pesel;";
            string borrowQuery = "UPDATE tasks SET tytul=IF(tytul='', @title, CONCAT(tytul, ', ', @title)), ilosc_ksiazek=ilosc_ksiazek+1 WHERE pesel=@pesel;";
            string decreaseStockQuery = "UPDATE books SET stan=stan-1 WHERE tytul=@title;";

            // A query checking whether the book exists and is available
            string checkBookExistsQuery = "SELECT stan FROM books WHERE tytul=@title;";

            if (OpenConnection())
            {
                MySqlTransaction transaction = null;

                try
                {
                    transaction = connection.BeginTransaction();

                    // Check if the user exists
                    MySqlCommand countCmd = new MySqlCommand(countQuery, connection, transaction);
                    countCmd.Parameters.AddWithValue("@pesel", pesel);
                    int count = Convert.ToInt32(countCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        // Checking whether the book exists and whether its status is > 0
                        MySqlCommand checkBookCmd = new MySqlCommand(checkBookExistsQuery, connection, transaction);
                        checkBookCmd.Parameters.AddWithValue("@title", title);
                        int stock = Convert.ToInt32(checkBookCmd.ExecuteScalar());

                        if (stock > 0)
                        {
                            // Borrowing a book
                            using (MySqlCommand borrowCmd = new MySqlCommand(borrowQuery, connection, transaction))
                            {
                                borrowCmd.Parameters.AddWithValue("@title", title);
                                borrowCmd.Parameters.AddWithValue("@pesel", pesel);
                                borrowCmd.ExecuteNonQuery();

                                using (MySqlCommand decreaseStockCmd = new MySqlCommand(decreaseStockQuery, connection, transaction))
                                {
                                    decreaseStockCmd.Parameters.AddWithValue("@title", title);
                                    decreaseStockCmd.ExecuteNonQuery();

                                    // Updating the list of books in the task
                                    Task taskToUpdate = tasksList.Find(t => t.Pesel == pesel);
                                    if (taskToUpdate != null)
                                    {
                                        taskToUpdate.Title = string.IsNullOrEmpty(taskToUpdate.Title) ? title : $"{taskToUpdate.Title}, {title}";
                                        taskToUpdate.CountOfBooks++;
                                    }

                                    // Book status update in books List
                                    Book bookToUpdate = booksList.Find(b => b.Title == title);
                                    if (bookToUpdate != null)
                                    {
                                        bookToUpdate.Stock--;
                                    }

                                    transaction.Commit();
                                    MessageBox.Show("Book borrowed successfully!");
                                }
                            }
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("We currently do not have this book in stock.", "Book Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        transaction.Rollback();
                        MessageBox.Show("User does not exist!", "User Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (MySqlException ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show($"Database Error: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        public void ReturnBook(long pesel, string title)
        {
            SetLoginCredentials(uid, password); 

            string countQuery = "SELECT COUNT(*) FROM tasks WHERE pesel=@pesel;";
            string returnQuery = "UPDATE tasks SET tytul=@newTitle, ilosc_ksiazek=ilosc_ksiazek-1 WHERE pesel=@pesel;";
            string increaseStockQuery = "UPDATE books SET stan=stan+1 WHERE tytul=@title;";

            if (OpenConnection())
            {
                MySqlTransaction transaction = null;

                try
                {
                    transaction = connection.BeginTransaction();

                    MySqlCommand countCmd = new MySqlCommand(countQuery, connection, transaction);
                    countCmd.Parameters.AddWithValue("@pesel", pesel);
                    int count = Convert.ToInt32(countCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        using (MySqlCommand returnCmd = new MySqlCommand(returnQuery, connection, transaction))
                        {
                            string newTitle = RemoveBookFromTask(title, pesel);
                            returnCmd.Parameters.AddWithValue("@newTitle", newTitle);
                            returnCmd.Parameters.AddWithValue("@pesel", pesel);
                            returnCmd.ExecuteNonQuery();

                            using (MySqlCommand increaseStockCmd = new MySqlCommand(increaseStockQuery, connection, transaction))
                            {
                                increaseStockCmd.Parameters.AddWithValue("@title", title);
                                increaseStockCmd.ExecuteNonQuery();

                                // Updating the list of books in the task
                                Task taskToUpdate = tasksList.Find(t => t.Pesel == pesel);
                                if (taskToUpdate != null)
                                {
                                    taskToUpdate.Title = newTitle;
                                    taskToUpdate.CountOfBooks--;
                                }

                                // Book status update in books List
                                Book bookToUpdate = booksList.Find(b => b.Title == title);
                                if (bookToUpdate != null)
                                {
                                    bookToUpdate.Stock++;
                                }

                                transaction.Commit();
                                MessageBox.Show("Book returned successfully!");
                            }
                        }
                    }
                    else
                    {
                        transaction.Rollback();
                        MessageBox.Show("User does not exist!", "User Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (MySqlException ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show($"Database Error: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        public List<Task> Select()
        {
            tasksList.Clear(); 

            SetLoginCredentials(uid, password); 

            string query = "SELECT * FROM tasks;";

            if (OpenConnection())
            {
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        MySqlDataReader dataReader = cmd.ExecuteReader();

                        while (dataReader.Read())
                        {
                            int id = Convert.ToInt32(dataReader["id"]);
                            long pesel = Convert.ToInt64(dataReader["pesel"]);
                            string firstName = dataReader["imie"].ToString();
                            string lastName = dataReader["nazwisko"].ToString();
                            string title = dataReader["tytul"].ToString();
                            int countOfBooks = Convert.ToInt32(dataReader["ilosc_ksiazek"]);

                            Task task = new Task(id, pesel, firstName, lastName, title, countOfBooks);
                            tasksList.Add(task);
                        }

                        dataReader.Close();
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Database Error: {ex.Message}");
                }
                finally
                {
                    CloseConnection();
                }
            }

            return tasksList;
        }

        // Helper method to remove the book from the task's title list
        private string RemoveBookFromTask(string title, long pesel)
        {
            Task task = tasksList.Find(t => t.Pesel == pesel);
            if (task != null && !string.IsNullOrEmpty(task.Title))
            {
                string[] titles = task.Title.Split(new string[] { ", " }, StringSplitOptions.None);
                List<string> titleList = titles.ToList();
                titleList.Remove(title);
                return string.Join(", ", titleList);
            }
            return string.Empty;
        }

        public int Count()
        {
            string query = "SELECT COUNT(*) FROM tasks;";

            if (OpenConnection())
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Database Error: {ex.Message}");
                    return -1;
                }
                finally
                {
                    CloseConnection();
                }
            }

            return -1;
        }
    }
}






