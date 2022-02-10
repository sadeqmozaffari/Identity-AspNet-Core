using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Common
{
   public class FixText
    {
        public static string fixText(string text)
        {
            return text.Trim().ToLower();
        }
    }
}
