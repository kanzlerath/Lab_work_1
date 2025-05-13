/*using System;


namespace Lab1_compile
{
    internal class ParseError : Exception
    {
        private int _idx;
        public int Idx
        {
            get
            {
                return _idx;
            }
        }
        private String incorrStr;

        public String IncorrStr
        {
            get
            {
                return incorrStr;
            }
        }

        public ParseError(String msg, String rem, int index) : base(msg)
        {
            incorrStr = rem;
            _idx = index;
        }
    }
}
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1_compile
{
    internal class ParseError
    {
        public string Message { get; }
        public string TokenValue { get; }
        public int Position { get; }

        public ParseError(string message, Token token)
        {
            Message = message;
            TokenValue = token?.Value ?? "EOF";
            Position = token?.Start ?? -1;
        }
    }
}
