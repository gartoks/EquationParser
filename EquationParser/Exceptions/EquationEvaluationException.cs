using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationParser.Exceptions {
    public class EquationEvaluationException : Exception {

        public EquationEvaluationException()
            : base() { }

        public EquationEvaluationException(string msg)
            : base(msg) { }


    }
}
