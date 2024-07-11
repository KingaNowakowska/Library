using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt_BD
{
    public class Task
    {
        public int Id { get; set; }
        public long Pesel { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Title { get; set; }
        public int CountOfBooks { get; set; }


        
        public Task(int id, long pesel, string name, string surname, string title, int countOfBooks)
        {
            Id = id;
            Pesel = pesel;
            Name = name;
            Surname = surname;
            Title = title;
            CountOfBooks = countOfBooks;
        }
    }

}
