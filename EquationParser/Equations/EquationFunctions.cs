using EquationParser.Equations.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EquationParser.Equations {
    internal static class EquationFunctions {

        // FUNCTIONS
        // Parameters: 0
        internal static Func<float> E = () => (float)Math.E;
        internal static Func<float> PI = () => (float)Math.PI;

        // Parameters: 1
        internal static Func<float, float> ln = x0 => (float)Math.Log(x0);
        internal static Func<float, float> sign = x0 => Math.Sign(x0);
        internal static Func<float, float> abs = x0 => Math.Abs(x0);
        internal static Func<float, float> ceil = x0 => (float)Math.Ceiling(x0);
        internal static Func<float, float> floor = x0 => (float)Math.Floor(x0);
        internal static Func<float, float> round = x0 => (float)Math.Round(x0);
        internal static Func<float, float> sin = x0 => (float)Math.Sin(x0);
        internal static Func<float, float> cos = x0 => (float)Math.Cos(x0);
        internal static Func<float, float> tan = x0 => (float)Math.Tan(x0);
        internal static Func<float, float> asin = x0 => (float)Math.Asin(x0);
        internal static Func<float, float> acos = x0 => (float)Math.Acos(x0);
        internal static Func<float, float> atan = x0 => (float)Math.Atan(x0);
        internal static Func<float, float> sinh = x0 => (float)Math.Sinh(x0);
        internal static Func<float, float> cosh = x0 => (float)Math.Cosh(x0);
        internal static Func<float, float> tanh = x0 => (float)Math.Tanh(x0);
        internal static Func<float, float> exp = x0 => (float)Math.Exp(x0);
        internal static Func<float, float> clamp01 = x0 => clamp(x0, 0, 1);

        // Parameters: 2
        internal static Func<float, float, float> log = (x0, x1) => (float)Math.Log(x0, x1);
        internal static Func<float, float, float> atan2 = (x0, x1) => (float)Math.Atan2(x0, x1);
        internal static Func<float, float, float> nrt = (x0, x1) => (float)Math.Pow(x0, 1.0 / x1);
        internal static Func<float, float, float> min = (x0, x1) => (float)Math.Min(x0, x1);
        internal static Func<float, float, float> max = (x0, x1) => (float)Math.Max(x0, x1);
        internal static Func<float, float, float> mod = (x0, x1) => x0 % x1;

        // Parameters: 3
        internal static Func<float, float, float, float> clamp = (x0, x1, x2) => x0 < x1 ? x1 : (x0 > x2 ? x2 : x0);

        // LAMBDA FUNCTIONS
        internal static Func<float, float, float> sumL = (r, x) => r + x;
        internal static Func<float, float, float> mulL = (r, x) => r * x;
        internal static Func<float, float, float> maxL = (r, x) => max(r, x);

        private static FieldInfo[] functionInfos;
        internal static IEnumerable<FieldInfo> FunctionInfos {
            get {
                if (functionInfos == null)
                    functionInfos = typeof(EquationFunctions).GetFields(BindingFlags.NonPublic | BindingFlags.Static);

                return functionInfos;
            }
        }

        internal static FieldInfo GetFunctionInfo(string name, int parameters) {
            IEnumerable<FieldInfo> fI = FunctionInfos.Where(f => f.Name.Equals(name) && f.FieldType.GetGenericArguments().Length - 1 == parameters && !IsLambdaFunction(name, parameters));
            if (fI.Count() != 1)
                return null;

            return fI.Single();
        }

        internal static IEnumerable<FieldInfo> GetFunctionInfos(int parameters) {
            return FunctionInfos.Where(f => f.FieldType.GetGenericArguments().Length - 1 == parameters);
        }

        internal static IEnumerable<string> GetFunctions(int parameters) {
            return GetFunctionInfos(parameters).Select(f => f.Name);
        }

        //internal static int GetFunctionParameterCount(string name) {
        //    if (IsLambdaFunction(name)) {
        //        return 4;
        //    } else {
        //        FieldInfo fI = GetFunctionInfo(name);

        //        if (fI == null)
        //            return -1;

        //        return fI.FieldType.GetGenericArguments().Length - 1;
        //    }
        //}

        private static string[] lambdaFunctions = { "sumL", "mulL", "maxL" };
        internal static bool IsLambdaFunction(string name, int parameters) => parameters == 4 && lambdaFunctions.Contains(name);

        internal static IEquation CreateFunction(string functionName, IEnumerable<IEquation> parameters) {
            int parameterCount = parameters.Count();
            if (IsLambdaFunction(functionName, parameterCount)) {
                IEnumerable<FieldInfo> fIs = FunctionInfos.Where(f => f.Name.Equals(functionName) && f.FieldType.GetGenericArguments().Length == 3);

                if (fIs.Count() != 1)
                    return null;

                FieldInfo fI = fIs.Single();

                return new LambdaFunction((Func<float, float, float>)fI.GetValue(null), parameters.ElementAt(0), parameters.ElementAt(1), parameters.ElementAt(2), parameters.ElementAt(3));
            } else {
                FieldInfo functionInfo = GetFunctionInfo(functionName, parameterCount);

                if (functionInfo == null)
                    return null;

                FieldInfo[] validityCheckerInfos = typeof(EquationValidityCheckers).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
                IEnumerable<FieldInfo> validityCheckerInfoL = validityCheckerInfos.Where(f => f.Name.Equals($"validity_checker_p{parameterCount}_{functionName.ToLower()}"));

                object validityChecker = null;
                if (validityCheckerInfoL.Count() == 1)
                    validityChecker = validityCheckerInfoL.Single().GetValue(null);

                switch (parameterCount) {
                    case 0:
                        return new Constant(((Func<float>)functionInfo.GetValue(null)).Invoke());
                    case 1:
                        return new Function1P((Func<float, float>)functionInfo.GetValue(null), parameters.ElementAt(0), (Func<float, Action<string>, bool>)validityChecker);
                    case 2:
                        return new Function2P((Func<float, float, float>)functionInfo.GetValue(null), parameters.ElementAt(0), parameters.ElementAt(1), (Func<float, float, Action<string>, bool>)validityChecker);
                    case 3:
                        return new Function3P((Func<float, float, float, float>)functionInfo.GetValue(null), parameters.ElementAt(0), parameters.ElementAt(1), parameters.ElementAt(2), (Func<float, float, float, Action<string>, bool>)validityChecker);
                }
            }

            return null;
        }


    }
}
