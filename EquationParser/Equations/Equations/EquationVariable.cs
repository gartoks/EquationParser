using System;
using System.Collections.Generic;

namespace EquationParser.Equations.Equations {
    public class EquationVariable : IEquation {

        public readonly int Index;

        public EquationVariable(int index) {
            Index = index;
        }

        public float Evaluate(float[] variables, List<float> equationVariables, Action<string> errorCallback) {
            if (Index >= equationVariables.Count) {
                errorCallback($"Equation variable {Index} not found");
                return default(float);
            }

            return equationVariables[Index];
        }
    }
}
