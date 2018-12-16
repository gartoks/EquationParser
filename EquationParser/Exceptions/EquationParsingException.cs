using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationParser.Exceptions {
    public class EquationParsingException : Exception {

        public EquationParsingException()
            : base() { }

        public EquationParsingException(string msg)
            : base(msg) { }

    }
}
