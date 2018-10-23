using System;
using System.Collections.Generic;

namespace Client
{
    public class Programs
    {
        private List<Process> programs;
        private long lastUpdate;
        private long beginTime;
        private Process processWithFocus;
        private bool notYetReceivedProgramWithFocus;    //serve per gestire caso in cui si riceva prima pacchetto con app focus e poi lista programmi
        private string nameFocus;
        private int handleFocus;

        private Programs()
        {
            programs = new List<Process>();
            beginTime = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
            lastUpdate = beginTime;
            notYetReceivedProgramWithFocus = false;
        }

        public bool addProgram(Process p)
        {
            //updateTime();
            if (!programs.Contains(p))
            {
                if(notYetReceivedProgramWithFocus && p.Name == nameFocus && p.Handle==handleFocus)
                    swapFocus(p);
                programs.Add(p);
                return true;
            }
            else
            {
                Process proc = programs.Find(item => item == p);
                if (notYetReceivedProgramWithFocus && proc.Name == nameFocus && p.Handle == handleFocus)
                    swapFocus(proc);
                proc.InUse = true;
                return false;
            }
            
        }

        public void swapFocus(Process p)
        {            
            if(processWithFocus!=null)
                processWithFocus.InFocus = false;
            p.InFocus = true;
            processWithFocus = p;
            notYetReceivedProgramWithFocus = false;
            updateTime();       //se problemi commento
        }

        public void changeFocus(string name, int handle)
        {           
            Process proc = programs.Find(item => ((item.Name == name) && (item.Handle == handle)));
            if (proc == null)
            {
                if (processWithFocus != null)
                {
                    processWithFocus.InFocus = false;
                    processWithFocus = null;
                }
                notYetReceivedProgramWithFocus = true;
                nameFocus = name;
                handleFocus = handle;
            }
            else
                swapFocus(proc);
        }

        public bool removeProgram(Process p)
        {
            //updateTime();
            if (programs.Contains(p))
            {
                Process proc = programs.Find(item => item == p);
                if (!proc.InUse)
                    return false;   //programma è già fra quelli non in uso, allora va aggiunto a lista e non tolto
              
                if (proc == processWithFocus)
                {
                    processWithFocus = null;
                    proc.InFocus = false;
                }
                proc.InUse = false;
                return true;
            }
            else
                return false;   //non esiste quel programma
        }

        public void updateTime()
        {
            long date = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;  //prima avevo +1
            long totalTime = (date - beginTime);
            
            foreach (var item in programs)
            {
                if (item.InFocus)
                {
                    item.TimeInFocus += (date - lastUpdate);
                    lastUpdate = date;
                }
                item.PercentageTimeInFocus = (float)item.TimeInFocus / totalTime * 100;
            }
        }

        public List<Process> getList()
        {
            return programs;
        }

        public List<Process> getListInUse()
        {
            return programs.FindAll(item => item.InUse == true);
        }

        static public Programs creaListPrograms()
        {
            return new Programs();
        }
    }
}
