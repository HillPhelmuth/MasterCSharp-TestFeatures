using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSyntaxModule
{
    public class CSharpDiagramState
    {
        public event Action<string> OnSendRawCode;
        public void SendCode(string code) => SendRawCode(code);
        private void SendRawCode(string code)
        {
            OnSendRawCode?.Invoke(code);
        }
    }
}
