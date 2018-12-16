using System;
using System.Collections.Generic;

namespace EquationParser.Equations.Equations {
    public class Constant : IEquation {

        public readonly float Value;

        public Constant(float value) {
            Value = value;
        }

        public float Evaluate(float[] variables, List<float> equationVariables, Action<string> errorCallback) {
            return Value;
        }
    }
}
