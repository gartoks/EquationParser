using System;
using System.Collections.Generic;

namespace EquationParser.Equations.Equations {
    public class Variable : IEquation {

        public readonly int Index;

        public Variable(int index) {
            Index = index;
        }

        public float Evaluate(float[] variables, List<float> equationVariables, Action<string> errorCallback) {
            if (Index >= variables.Length) {
                errorCallback($"Variable {Index} not found");
                return default(float);
            }

            return variables[Index];
        }
    }
}
