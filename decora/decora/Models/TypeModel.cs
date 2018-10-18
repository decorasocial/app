using System;
using System.Collections.Generic;
using System.Text;

namespace decora.Models
{
    public class TypeModel
    {
        public int idType { get; set; }
        public string title { get; set; }

        public override string ToString()
        {
            return this.title;
        }
    }
}
