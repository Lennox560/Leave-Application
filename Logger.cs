using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employee_Leave_Booking_System
{
    public static class Logger
    {
        public static void LogError(Exception ex, string message)
        {
            using (StreamWriter writer = new StreamWriter("logFile", true))
            {
                writer.WriteLine("Message :" + message);
                writer.WriteLine("Exception : " + ex.StackTrace);
            }
        }
    }
}
