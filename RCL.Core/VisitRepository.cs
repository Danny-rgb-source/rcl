using System.Collections.Generic;

namespace RCL.Core
{
    public class VisitRepository
    {
        private Dictionary<string,int> visitLog = new Dictionary<string,int>();

        public void LogVisit(string customerName)
        {
            if (!visitLog.ContainsKey(customerName))
                visitLog[customerName] = 0;
            visitLog[customerName]++;
        }

        public int GetVisits(string customerName)
        {
            return visitLog.ContainsKey(customerName) ? visitLog[customerName] : 0;
        }

        public Dictionary<string,int> GetAllVisits()
        {
            return visitLog;
        }
    }
}

