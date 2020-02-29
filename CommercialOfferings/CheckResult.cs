using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommercialOfferings
{
    public class CheckList
    {
        public bool CheckSuccess
        {
            get { return _check; }
        }
        private bool _check = true;
        public List<string> Messages = new List<string>();

        public void Check(bool check, string message)
        {
            if (!check)
            {
                _check = false;
                Messages.Add(message);
            }
        }
        public void Check(CheckList checkList, string message)
        {
            if (!checkList.CheckSuccess)
            {
                _check = false;
                Messages.Add(message);
                Messages.AddRange(checkList.Messages);
            }
        }
    }
}
