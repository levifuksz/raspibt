using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasPiBtControl.Model
{
    public class BtDeviceInfo
    {
        public const string RequiredServiceID = "7be1fcb3-5776-42fb-91fd-2ee7b5bbb86d";

        public string Name { get; set; }
        public string Address { get; set; }
        public bool ServicesDiscovered { get; set; }
        public bool HasRequiredServiceID { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
