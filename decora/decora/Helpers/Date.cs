using System;
using System.Collections.Generic;
using System.Text;

namespace decora.Helpers
{
    public class Date
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

        public static string sinceAgo(string date, int since)
        {
            int day = 1444;
            if (since > (2 * day))
                return date;

            if (since > day)
                return "Ontem.";

            if (since > 60)
            {
                float div = since % 60;
                string.Format("Há {0} horas.", Convert.ToString(Math.Floor(div)));
            }

            if (since > 1)
                return string.Format("Há {0} minutos.", since);

            return "Neste minuto.";
        }
    }
}
