using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt_BD
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int Stock { get; set; }

        public Book(int id, string title, string author, int stock)
        {
            Id = id;
            Title = title;
            Author = author;
            Stock = stock;
        }
    }

}
