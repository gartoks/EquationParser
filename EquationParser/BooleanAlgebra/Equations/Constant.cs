using System;

namespace EquationParser.BooleanAlgebra.Equations {
    public class Constant : ILogic {

        public readonly bool Value;

        public Constant(bool value) {
            Value = value;
        }

        public bool Evaluate(bool[] variables, Action<string> errorCallback) {
            return Value;
        }
    }
}
