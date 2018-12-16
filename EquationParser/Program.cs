
using EquationParser.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EquationParser.Equations {
    class Program {
        static void Main(string[] args) {

            Test_Equation_Parsing();

        }

        private static void Test_Logic_Parsing() {
            string s = "true^false";

            //List<EquationParsingException> exc;
            Logic l = Logic.Parse(s);

            //foreach (EquationParsingException ec in exc) {
            //    Debug.WriteLine("ParsingException: " + ec.Message);
            //}

            //List<EquationEvaluationException> evExc = null;
            Debug.WriteLine(l.Evaluate(false, false));
            Debug.WriteLine(l.Evaluate(false, true));
            Debug.WriteLine(l.Evaluate(true, false));
            Debug.WriteLine(l.Evaluate(true, true));

            //foreach (EquationEvaluationException ec in evExc) {
            //    Debug.WriteLine("EvaluationException: " + ec.Message);
            //}
        }

        private static void Test_Equation_Parsing() {
            string s = "1.0 / (1.0 + exp(-1 * {0}))";
            //string s = "maxL(0, 10, 1, $0$)";

            List<EquationParsingException> exc;
            Equation e = Equation.Parse(s, out exc);

            foreach (EquationParsingException ec in exc) {
                Debug.WriteLine("ParsingException: " + ec.Message);
            }

            List<EquationEvaluationException> evExc = null;
            Debug.WriteLine(e.Evaluate(out evExc, 2));

            foreach (EquationEvaluationException ec in evExc) {
                Debug.WriteLine("EvaluationException: " + ec.Message);
            }
        }

        private static void Test_RemoveWhitespaces() {
            string es = "3 * (7 + 3 * (2 - 1)) / 2 + 1 * (4 + 8)";

            StringBuilder sB = new StringBuilder();
            for (int i = 0; i < es.Length; i++) {
                if (!Char.IsWhiteSpace(es[i]))
                    sB.Append(es[i]);
            }
            Debug.WriteLine(sB.ToString());
        }

    }
}
