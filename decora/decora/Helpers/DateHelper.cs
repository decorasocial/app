using System;
using System.Collections.Generic;
using System.Text;

namespace decora.Helpers
{
    public class DateHelper
    {
        public static string convertStringDate(string oldDate, string format = "dd/mm/yyyy")
        {
            DateTime dt = DateTime.Parse(oldDate);

            if (format == "ago")
            {
                return "ago";
            }
            else
            {
                return dt.ToString(format);
            }

        }
    }
}
