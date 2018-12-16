using EquationParser.BooleanAlgebra;
using EquationParser.BooleanAlgebra.Equations;
using EquationParser.Equations.Equations;
using EquationParser.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquationParser {
    public sealed class Logic {
        public static Logic Parse(string es) {
            List<EquationParsingException> exc;
            return Parse(es, out exc);
        }

        public static Logic Parse(string es, out List<EquationParsingException> exceptions) {
            // string clear white spaces
            StringBuilder sB = new StringBuilder();
            for (int i = 0; i < es.Length; i++) {
                if (!Char.IsWhiteSpace(es[i]))
                    sB.Append(es[i]);
            }
            es = sB.ToString();

            List<EquationParsingException> exc = new List<EquationParsingException>();

            Logic eq = new Logic(LogicParser.ParseEquation(es, e => exc.Add(new EquationParsingException(e))));

            exceptions = exc;

            return eq;
        }

        private ILogic equation;

        internal Logic(ILogic equation) {
            this.equation = equation;
        }

        public bool Evaluate(params bool[] variables) {
            return this.equation.Evaluate(variables, e => { });
        }

        public bool Evaluate(out List<EquationEvaluationException> errors, params bool[] variables) {
            List<EquationEvaluationException> es = new List<EquationEvaluationException>();

            bool result = this.equation.Evaluate(variables, e => es.Add(new EquationEvaluationException(e)));

            errors = es;

            return result;
        }
    }
}
