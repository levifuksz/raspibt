using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasPiBtControl.Services
{
    public interface IProgressDialogService
    {
        void Show(string message);
        void Hide();
    }
}
