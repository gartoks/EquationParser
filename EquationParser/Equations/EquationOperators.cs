using EquationParser.Equations.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EquationParser.Equations {
    internal static class EquationOperators {

        // Parameters: 0

        // Parameters: 1
        internal static Func<float, float> negate = x0 => -x0;
        internal static Func<float, float> sqrt = x0 => (float)Math.Sqrt(x0);

        // Parameters: 2
        internal static Func<float, float, float> sum = (x0, x1) => x0 + x1;
        internal static Func<float, float, float> difference = (x0, x1) => x0 - x1;
        internal static Func<float, float, float> multiply = (x0, x1) => x0 * x1;
        internal static Func<float, float, float> divide = (x0, x1) => x0 / x1;
        internal static Func<float, float, float> pow = (x0, x1) => (float)Math.Pow(x0, x1);
        internal static Func<float, float, float> mod = (x0, x1) => x0 % x1;

        // Parameters: 3

        private static FieldInfo[] operatorInfos;
        internal static IEnumerable<FieldInfo> OperatorInfos {
            get {
                if (operatorInfos == null)
                    operatorInfos = typeof(EquationOperators).GetFields(BindingFlags.NonPublic | BindingFlags.Static);

                return operatorInfos;
            }
        }

        private static string[] operators = new string[] { "v", "^", "/", "%", "*", "-", "+" };
        internal static IEnumerable<string> Operators => operators;
        
        internal static bool IsContainedOperator(string name) {
            return name == "^" || name == "/" || name == "%" || name == "*" || name == "-" || name == "+";
        }

        internal static bool IsLeadingOperator(string name) {
            return name == "v" || name == "-";
        }

        internal static string GetOperatorIdentifier(string op) {
            switch (op) {
                case "v":
                    return "sqrt";
                case "^":
                    return "pow";
                case "/":
                    return "divide";
                case "%":
                    return "mod";
                case "*":
                    return "multiply";
                case "-":
                    return "difference";
                case "+":
                    return "sum";
            }

            return null;
        }

        internal static int GetOperatorPriority(string op) {
            switch (op) {
                case "v":
                    return 7;
                case "^":
                    return 6;
                case "/":
                    return 5;
                case "%":
                    return 4;
                case "*":
                    return 3;
                case "-":
                    return 2;
                case "+":
                    return 1;
            }

            return -1;
        }

        internal static FieldInfo GetOperatorInfo(string name) {
            IEnumerable<FieldInfo> fI = OperatorInfos.Where(f => f.Name.Equals(name));
            if (fI.Count() != 1)
                return null;

            return fI.Single();
        }

        internal static IEnumerable<FieldInfo> GetOperatorInfos(int parameters) {
            return OperatorInfos.Where(f => f.FieldType.GetGenericArguments().Length - 1 == parameters);
        }

        internal static IEnumerable<string> GetOperators(int parameters) {
            return GetOperatorInfos(parameters).Select(f => f.Name);
        }

        internal static int GetOperatorParameterCount(string name) {
            FieldInfo fI = GetOperatorInfo(name);

            if (fI == null)
                return -1;

            return fI.FieldType.GetGenericArguments().Length - 1;
        }

        internal static IEquation CreateOperator(string operatorName, IEnumerable<IEquation> parameters) {
            string operatorIdentifier = GetOperatorIdentifier(operatorName);
            FieldInfo operatorInfo = GetOperatorInfo(operatorIdentifier);

            if (operatorInfo == null)
                return null;

            int parameterCount = parameters.Count();
            if (GetOperatorParameterCount(operatorIdentifier) != parameterCount)
                return null;

            FieldInfo[] validityCheckerInfos = typeof(EquationValidityCheckers).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            IEnumerable<FieldInfo> validityCheckerInfoL = validityCheckerInfos.Where(f => f.Name.Equals($"validity_checker_p{parameterCount}_{operatorIdentifier.ToLower()}"));

            object validityChecker = null;
            if (validityCheckerInfoL.Count() == 1)
                validityChecker = validityCheckerInfoL.Single().GetValue(null);


            switch (parameterCount) {
                case 0:
                    return new Constant(((Func<float>)operatorInfo.GetValue(null)).Invoke());
                case 1:
                    return new Function1P((Func<float, float>)operatorInfo.GetValue(null), parameters.ElementAt(0), (Func<float, Action<string>, bool>)validityChecker);
                case 2:
                    return new Function2P((Func<float, float, float>)operatorInfo.GetValue(null), parameters.ElementAt(0), parameters.ElementAt(1), (Func<float, float, Action<string>, bool>)validityChecker);
                case 3:
                    return new Function3P((Func<float, float, float, float>)operatorInfo.GetValue(null), parameters.ElementAt(0), parameters.ElementAt(1), parameters.ElementAt(2), (Func<float, float, float, Action<string>, bool>)validityChecker);
            }

            return null;
        }

    }
}
