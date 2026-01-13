using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.Permission.EventModel
{
    public static  class MessengerEvent
    {
        public static event Action? SessionExpired;
        public static void SendSessionExpired()
        {
            SessionExpired?.Invoke();
        }
    }
}
