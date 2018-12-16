using EquationParser.BooleanAlgebra.Equations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EquationParser.BooleanAlgebra {
    internal static class LogicParser {
        internal static ILogic ParseEquation(string es, Action<string> errorCallback) {
            // remove outer ( )
            bool invalidParPlacement;
            es = RemoveOuterParentheses(es, out invalidParPlacement);
            if (invalidParPlacement) {
                errorCallback("Cannot parse equation, invalid parentheses placement.");
                return null;
            }

            List<SegmentData> segments = new List<SegmentData>();
            string segment = "";
            HashSet<SegmentType> potentialSegmentTypes = new HashSet<SegmentType>();
            potentialSegmentTypes.Add(SegmentType.Variable);
            potentialSegmentTypes.Add(SegmentType.Constant);
            potentialSegmentTypes.Add(SegmentType.Function);
            potentialSegmentTypes.Add(SegmentType.Operator);
            potentialSegmentTypes.Add(SegmentType.Equation);
            for (int j = 0; j <= es.Length; j++) {
                char c = j < es.Length ? es[j] : '\0';
                int segmentStartIndex = j - segment.Length;

                SegmentData finishedSegment = null;
                if (finishedSegment == null && potentialSegmentTypes.Contains(SegmentType.Variable) && IsVariableFinished(segment, c))
                    finishedSegment = new SegmentData(segmentStartIndex, segment, SegmentType.Variable);
                else if (finishedSegment == null && potentialSegmentTypes.Contains(SegmentType.Constant) && IsConstantFinished(segment, c))
                    finishedSegment = new SegmentData(segmentStartIndex, segment, SegmentType.Constant);
                else if (finishedSegment == null && potentialSegmentTypes.Contains(SegmentType.Function) && IsFunctionFinished(segment, c))
                    finishedSegment = new SegmentData(segmentStartIndex, segment, SegmentType.Function);
                else if (finishedSegment == null && potentialSegmentTypes.Contains(SegmentType.Operator) && IsOperatorFinished(segment, c))
                    finishedSegment = new SegmentData(segmentStartIndex, segment, SegmentType.Operator);
                else if (finishedSegment == null && potentialSegmentTypes.Contains(SegmentType.Equation) && IsEquationFinished(segment, c))
                    finishedSegment = new SegmentData(segmentStartIndex, segment, SegmentType.Equation);

                if (finishedSegment != null) {
                    segments.Add(finishedSegment);

                    potentialSegmentTypes.Clear();
                    potentialSegmentTypes.Add(SegmentType.Variable);
                    potentialSegmentTypes.Add(SegmentType.Constant);
                    potentialSegmentTypes.Add(SegmentType.Function);
                    potentialSegmentTypes.Add(SegmentType.Operator);
                    potentialSegmentTypes.Add(SegmentType.Equation);

                    segment = "";
                    finishedSegment = null;
                }

                segment += c;

                // check possible segment types
                if (potentialSegmentTypes.Contains(SegmentType.Variable) && !IsStartOfVariable(segment))
                    potentialSegmentTypes.Remove(SegmentType.Variable);
                if (potentialSegmentTypes.Contains(SegmentType.Constant) && !IsStartOfConstant(segment))
                    potentialSegmentTypes.Remove(SegmentType.Constant);
                if (potentialSegmentTypes.Contains(SegmentType.Function) && !IsStartOfFunction(segment))
                    potentialSegmentTypes.Remove(SegmentType.Function);
                if (potentialSegmentTypes.Contains(SegmentType.Operator) && !IsStartOfOperator(segment))
                    potentialSegmentTypes.Remove(SegmentType.Operator);
                if (potentialSegmentTypes.Contains(SegmentType.Equation) && !IsStartOfEquation(segment))
                    potentialSegmentTypes.Remove(SegmentType.Equation);
            }

            // pre-process segments
            int k = 0;
            while (k < segments.Count) {
                int i = k;
                k++;

                SegmentData sD = segments[i];

                if (sD.Type != SegmentType.Function)
                    continue;

                if (i == segments.Count - 1) {
                    errorCallback($"Function '{sD.Data}' has no body.");
                    return null;
                }

                SegmentData functionBodySegment = segments[i + 1];

                if (functionBodySegment.Type != SegmentType.Equation) {
                    errorCallback($"Invalid function body '{functionBodySegment.Data}' of function '{sD.Data}' at {functionBodySegment.Index}.");
                }

                segments.RemoveAt(i + 1);

                sD = new SegmentData(sD.Index, sD.Data + functionBodySegment.Data, SegmentType.Function);
                segments[i] = sD;
            }

            k = 0;
            while (k < segments.Count) {
                int i = k;
                k++;

                SegmentData sD = segments[i];

                if (sD.Type != SegmentType.Operator)
                    continue;

                if (!LogicFunctions.IsLeadingOperator(sD.Data))
                    continue;

                if (i > 0 && LogicFunctions.IsContainedOperator(sD.Data) && IsValidOperatorParameterType(segments[i - 1].Type))
                    continue;

                if (i == segments.Count - 1) {
                    errorCallback($"Operator '{sD.Data}' has no body.");
                    return null;
                }

                SegmentData operatorBodySegment = segments[i + 1];

                if (!IsValidOperatorParameterType(operatorBodySegment.Type)) {
                    errorCallback($"Invalid operator '{sD.Data}' placement at {sD.Index}.");
                    return null;
                }

                segments.RemoveAt(i + 1);

                sD = new SegmentData(sD.Index, $"{LogicFunctions.GetLeadingOperatorFunctionName(sD.Data)}({operatorBodySegment.Data})", SegmentType.Function);
                segments[i] = sD;
            }

            // process segments
            if (segments.Count == 1)
                return ParseSingleEquation(segments[0], errorCallback);

            while (segments.Count > 1) {
                SegmentData highestOperator = FindHighestPriorityOperator(segments);
                int segmentIndex = segments.FindIndex(sD => sD.Index == highestOperator.Index);

                if (segmentIndex == 0 || segmentIndex == segments.Count - 1) {
                    errorCallback($"Invalid operator placement of '{highestOperator.Data}' at position {highestOperator.Index}. An operator cannot lead or trail the equation.");
                    return null;
                }

                SegmentData precSegment = segments[segmentIndex - 1];
                SegmentData succSegment = segments[segmentIndex + 1];

                if (!IsValidOperatorParameterType(precSegment.Type)) {
                    errorCallback($"Invalid first operator argument '{precSegment.Data}' at {precSegment.Index}.");
                    return null;
                }

                if (!IsValidOperatorParameterType(succSegment.Type)) {
                    errorCallback($"Invalid second operator argument '{succSegment.Data}' at {succSegment.Index}.");
                    return null;
                }

                ILogic precEq = precSegment.Equation != null ? precSegment.Equation : ParseEquation(precSegment.Data, errorCallback);
                ILogic succEq = succSegment.Equation != null ? succSegment.Equation : ParseEquation(succSegment.Data, errorCallback);

                ILogic eq = CreateContainedOperatorEquation(highestOperator.Data, precEq, succEq);

                if (eq == null) {
                    errorCallback($"Invalid operator '{highestOperator.Data}' at {highestOperator.Index}.");
                    return null;
                }

                SegmentData mergedSegment = new SegmentData(precSegment.Index, precSegment.Data + highestOperator.Data + succSegment.Data, SegmentType.Equation);
                mergedSegment.Equation = eq;

                segments[segmentIndex - 1] = mergedSegment;
                segments.RemoveAt(segmentIndex + 1);
                segments.RemoveAt(segmentIndex);
            }

            return segments.Single().Equation;
        }

        private static ILogic ParseSingleEquation(SegmentData sD, Action<string> errorCallback) {

            string s = sD.Data;

            switch (sD.Type) {
                case SegmentType.Variable:
                    return new Variable(int.Parse(s.Substring(1, s.Length - 2)));
                case SegmentType.Constant:
                    return CreateFunction(s, new ILogic[0]);
                case SegmentType.Function: {
                        int functionBodyStartIndex = s.IndexOf('(');
                        int functionBodyEndIndex = s.LastIndexOf(')');

                        if (functionBodyStartIndex == -1) {
                            errorCallback($"Function call '{s}' at {sD.Index} has no body.");
                            return null;
                        }

                        string functionName = s.Substring(0, functionBodyStartIndex);
                        string functionBody = s.Substring(functionBodyStartIndex, s.Length - functionBodyStartIndex);

                        if (functionBodyEndIndex == -1) {
                            errorCallback($"Function body '{functionBody}' of function '{functionName}' at {sD.Index} has no end to its body.");
                            return null;
                        }

                        if (functionBodyEndIndex != s.Length - 1) {
                            errorCallback($"Function body '{functionBody}' of function '{functionName}' at {sD.Index} has an invalid body.");
                            return null;
                        }

                        bool invalidParPlacement;
                        functionBody = RemoveOuterParentheses(functionBody, out invalidParPlacement);

                        if (invalidParPlacement) {
                            errorCallback($"Invalid parenthesis placement in function body '{functionBody}' of function '{functionName}' at {sD.Index}.");
                            return null;
                        }

                        int functionParameterCount = LogicFunctions.GetFunctionParameterCount(functionName);

                        string[] parametersS = SplitParameters(functionBody);

                        if (functionParameterCount != parametersS.Length) {
                            errorCallback($"Cannot parse function '{s}', invalid number of parameters ({parametersS.Length} - expected {functionParameterCount}.");
                            return null;
                        }

                        ILogic[] parameterEquations = new ILogic[functionParameterCount];
                        for (int fpc = 0; fpc < functionParameterCount; fpc++) {
                            parameterEquations[fpc] = ParseEquation(parametersS[fpc], errorCallback);
                        }

                        return CreateFunction(functionName, parameterEquations);
                    }
            }

            errorCallback($"Cannot parse equation '{s}' at {sD.Index}.");
            return null;
        }

        private static string RemoveOuterParentheses(string s, out bool invalidParPlacement) {
            invalidParPlacement = false;

            int trailingPar = 0;
            for (int i = s.Length - 1; i >= 0; i--) {
                if (s[i] == ')')
                    trailingPar++;
                else
                    break;
            }

            int par = 0;
            int leadingPar = 0;
            bool checkingLeading = true;
            int minParInside = 0;
            for (int i = 0; i < s.Length; i++) {
                char c = s[i];

                if (c == ')') {
                    par--;

                    if (i < s.Length - trailingPar)
                        minParInside = Math.Min(minParInside, par);
                }

                if (c == '(') {
                    par++;
                    if (checkingLeading)
                        leadingPar++;
                } else if (checkingLeading) {
                    checkingLeading = false;
                    minParInside = leadingPar;
                }

                if (par < 0)
                    invalidParPlacement = true;
            }

            if (par != 0)
                invalidParPlacement = true;

            return s.Substring(minParInside, s.Length - 2 * minParInside);
        }

        private static ILogic CreateContainedOperatorEquation(string op, ILogic precEq, ILogic succEq) {
            switch (op) {
                case "^":
                    return new Function2P(LogicFunctions.and, precEq, succEq);
                case "v":
                    return new Function2P(LogicFunctions.or, precEq, succEq);
                case "x":
                    return new Function2P(LogicFunctions.xor, precEq, succEq);
                case "<->":
                    return new Function2P(LogicFunctions.biconditional, precEq, succEq);
                case "->":
                    return new Function2P(LogicFunctions.implication, precEq, succEq);
            }

            return null;
        }

        private static ILogic CreateFunction(string functionName, IEnumerable<ILogic> parameters) {
            FieldInfo functionInfo = LogicFunctions.GetFunctionInfo(functionName);

            if (functionInfo == null)
                return null;

            int parameterCount = parameters.Count();
            if (LogicFunctions.GetFunctionParameterCount(functionName) != parameterCount)
                return null;

            switch (parameterCount) {
                case 0:
                    return new Constant(((Func<bool>)functionInfo.GetValue(null)).Invoke());
                case 1:
                    return new Function1P((Func<bool, bool>)functionInfo.GetValue(null), parameters.ElementAt(0));
                case 2:
                    return new Function2P((Func<bool, bool, bool>)functionInfo.GetValue(null), parameters.ElementAt(0), parameters.ElementAt(1));
                case 3:
                    return new Function3P((Func<bool, bool, bool, bool>)functionInfo.GetValue(null), parameters.ElementAt(0), parameters.ElementAt(1), parameters.ElementAt(2));
            }

            return null;
        }

        private static string[] SplitParameters(string s) {
            if (s.Length == 0)
                return new string[0];

            List<int> commaIndices = new List<int>();
            int par = 0;
            for (int i = 0; i < s.Length; i++) {
                char c = s[i];

                if (c == '(')
                    par++;
                else if (c == ')')
                    par--;
                else if (par == 0 && c == ',')
                    commaIndices.Add(i);
            }

            string[] parameters = new string[commaIndices.Count + 1];
            if (parameters.Length == 1)
                parameters[0] = s;
            else {
                parameters[0] = s.Substring(0, commaIndices[0]);
                parameters[parameters.Length - 1] = s.Substring(commaIndices[commaIndices.Count - 1] + 1, s.Length - (commaIndices[commaIndices.Count - 1] + 1));

                for (int i = 0; i < commaIndices.Count - 1; i++) {
                    int startIdx = commaIndices[i] + 1;
                    parameters[i + 1] = s.Substring(startIdx, commaIndices[i + 1] - startIdx);
                }
            }

            return parameters;
        }

        #region Equation
        internal static bool IsEquationFinished(string s, char nextC) {
            return IsEquation(s);
        }

        internal static bool IsEquation(string s) {
            if (s.Length == 0)
                return false;

            if (!IsStartOfEquation(s))
                return false;

            int parStack = 0;
            for (int i = 0; i < s.Length; i++) {
                char c = s[i];

                if (c == '(')
                    parStack++;
                else if (c == ')')
                    parStack--;
            }

            return parStack == 0;
        }

        internal static bool IsStartOfEquation(string s) {
            if (s.Length == 0)
                return true;

            return s.StartsWith("(");
        }
        #endregion

        #region Operator
        private static SegmentData FindHighestPriorityOperator(IEnumerable<SegmentData> segments) {
            int highestPrio = -1;
            SegmentData highestPrioOperator = null;
            foreach (SegmentData sD in segments) {
                if (sD.Type != SegmentType.Operator)
                    continue;

                int prio = LogicFunctions.GetOperatorPriority(sD.Data);
                if (highestPrioOperator == null || highestPrio < prio) {
                    highestPrioOperator = sD;
                    highestPrio = prio;
                }
            }

            return highestPrioOperator;
        }

        internal static bool IsValidOperatorParameterType(SegmentType type) {
            return type == SegmentType.Constant || type == SegmentType.Equation || type == SegmentType.Function || type == SegmentType.Variable;
        }

        internal static bool IsOperatorFinished(string s, char nextC) {
            return IsOperator(s) && !IsStartOfOperator(s + nextC);
        }

        internal static bool IsOperator(string s) {
            if (s.Length == 0)
                return false;

            return LogicFunctions.Operators.Contains(s);
        }

        internal static bool IsStartOfOperator(string s) {
            if (s.Length == 0)
                return true;

            foreach (string op in LogicFunctions.Operators) {
                if (op.StartsWith(s))
                    return true;
            }

            return false;
        }
        #endregion

        #region Function
        internal static bool IsFunctionFinished(string s, char nextC) {
            return nextC == '(' && IsFunction(s);
        }

        internal static bool IsFunction(string s) {
            if (s.Length == 0)
                return false;

            IEnumerable<FieldInfo> functionInfos = LogicFunctions.FunctionInfos.Where(f => f.FieldType.GetGenericArguments().Count() > 1);
            foreach (FieldInfo fI in functionInfos) {
                if (s.Equals(fI.Name))
                    return true;
            }

            return false;
        }

        internal static bool IsStartOfFunction(string s) {
            if (s.Length == 0)
                return true;

            IEnumerable<FieldInfo> functionInfos = LogicFunctions.FunctionInfos.Where(f => f.FieldType.GetGenericArguments().Count() > 1);
            foreach (FieldInfo fI in functionInfos) {
                if (fI.Name.StartsWith(s))
                    return true;
            }

            return false;
        }
        #endregion

        #region Constant
        internal static bool IsConstantFinished(string s, char nextC) {
            return IsConstant(s) && !IsStartOfConstant(s + nextC);
        }

        internal static bool IsConstant(string s) {
            return LogicFunctions.GetFunctions(0).Contains(s);
        }

        internal static bool IsStartOfConstant(string s) {
            foreach (string constant in LogicFunctions.GetFunctions(0)) {
                if (constant.StartsWith(s))
                    return true;
            }

            return false;
        }
        #endregion

        #region Variable
        internal static bool IsVariableFinished(string s, char nextC) {
            return IsVariable(s);
        }

        internal static bool IsVariable(string s) {
            return s.StartsWith("{") && s.EndsWith("}") && IsInteger(s.Substring(1, s.Length - 2));
        }

        internal static bool IsStartOfVariable(string s) {
            return s.Length > 0 && s[0] == '{' && (s.Length == 1 || IsVariable(s) || IsInteger(s.Substring(1, s.Length - 1)));

        }
        #endregion

        #region Integer
        internal static bool IsIntegerFinished(string s, char nextC) {
            return !IsPartOfInteger(nextC) && IsInteger(s);
        }

        internal static bool IsInteger(string s) {
            if (s.Length == 0)
                return false;

            for (int i = 0; i < s.Length; i++) {
                if (!IsPartOfInteger(s[i]))
                    return false;
            }

            return true;
        }

        internal static bool IsStartOfInteger(string s) {
            return s.Length > 0 && IsInteger(s);
        }

        internal static bool IsPartOfInteger(char c) {
            return c == '0' || c == '1' || c == '2' || c == '3' || c == '4' || c == '5' || c == '6' || c == '7' || c == '8' || c == '9';
        }
        #endregion
    }
}
