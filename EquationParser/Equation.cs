using EquationParser.Equations.Equations;
using EquationParser.Exceptions;
using System.Collections.Generic;
using System.Text;

namespace EquationParser {
    public sealed class Equation {
        public static Equation Parse(string es) {
            List<EquationParsingException> exc;
            return Parse(es, out exc);
        }

        public static Equation Parse(string es, out List<EquationParsingException> exceptions) {
            // string clear white spaces
            StringBuilder sB = new StringBuilder();
            foreach (char c in es) {
                if (!char.IsWhiteSpace(c))
                    sB.Append(c);
            }
            es = sB.ToString();

            List<EquationParsingException> exc = new List<EquationParsingException>();

            Equation eq = new Equation(Equations.EquationParser.ParseEquation(es, e => exc.Add(new EquationParsingException(e))));

            exceptions = exc;

            return eq;
        }

        private IEquation equation;

        internal Equation(IEquation equation) {
            this.equation = equation;
        }

        public float Evaluate(params float[] variables) {
            return this.equation.Evaluate(variables, new List<float>(), e => { });
        }

        public float Evaluate(out List<EquationEvaluationException> errors, params float[] variables) {
            List<EquationEvaluationException> es = new List<EquationEvaluationException>();

            float result = this.equation.Evaluate(variables, new List<float>(), e => es.Add(new EquationEvaluationException(e)));

            errors = es;

            return result;
        }
    }
}
