using System;
using System.Collections.Generic;
using System.Text;

namespace decora.Models
{
    public class BookModel
    {
        public int idBook { get; set; }
        public string title { get; set; }

        public override string ToString()
        {
            return this.title;
        }
    }
}
