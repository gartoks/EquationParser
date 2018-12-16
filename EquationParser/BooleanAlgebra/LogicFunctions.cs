using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EquationParser.BooleanAlgebra {
    internal static class LogicFunctions {

        // Parameters: 0
        internal static Func<bool> TRUE = () => true;
        internal static Func<bool> FALSE = () => false;
        internal static Func<bool> T = () => true;
        internal static Func<bool> F = () => false;

        // Parameters: 1

        internal static Func<bool, bool> not = x0 =>!x0;

        // Parameters: 2
        internal static Func<bool, bool, bool> and = (x0, x1) => x0 && x1;
        internal static Func<bool, bool, bool> or = (x0, x1) => x0 || x1;
        internal static Func<bool, bool, bool> xor = (x0, x1) => x0 ^ x1;
        internal static Func<bool, bool, bool> implication = (x0, x1) => or(not(x0), x1);
        internal static Func<bool, bool, bool> biconditional = (x0, x1) => not(xor(x0, x1));

        // Parameters: 3

        private static FieldInfo[] functionInfos;
        internal static IEnumerable<FieldInfo> FunctionInfos {
            get {
                if (functionInfos == null)
                    functionInfos = typeof(LogicFunctions).GetFields(BindingFlags.NonPublic | BindingFlags.Static);

                return functionInfos;
            }
        }

        private static string[] operators = new string[] { "^", "v", "-", "x", "->", "<->" };
        internal static IEnumerable<string> Operators => operators;
        
        internal static bool IsContainedOperator(string name) {
            return name == "^" || name == "v" || name == "x" || name == "->" || name == "<->";
        }

        internal static bool IsLeadingOperator(string name) {
            return name == "-";
        }

        internal static string GetLeadingOperatorFunctionName(string op) {
            switch (op) {
                case "-":
                    return "not";
            }

            return null;
        }

        internal static int GetOperatorPriority(string op) {
            switch (op) {
                case "-":
                    return 3;
                case "->":
                    return 2;
                case "<->":
                    return 2;
                case "v":
                    return 1;
                case "^":
                    return 1;
                case "x":
                    return 1;
            }

            return -1;
        }

        internal static FieldInfo GetFunctionInfo(string name) {
            IEnumerable<FieldInfo> fI = FunctionInfos.Where(f => f.Name.ToLower().Equals(name.ToLower()));
            if (fI.Count() != 1)
                return null;

            return fI.Single();
        }

        internal static IEnumerable<FieldInfo> GetFunctionInfos(int parameters) {
            return FunctionInfos.Where(f => f.FieldType.GetGenericArguments().Length - 1 == parameters);
        }

        internal static IEnumerable<string> GetFunctions(int parameters) {
            return GetFunctionInfos(parameters).Select(f => f.Name.ToLower());
        }

        internal static int GetFunctionParameterCount(string name) {
            FieldInfo fI = GetFunctionInfo(name);

            if (fI == null)
                return -1;

            return fI.FieldType.GetGenericArguments().Length - 1;
        }


    }
}
