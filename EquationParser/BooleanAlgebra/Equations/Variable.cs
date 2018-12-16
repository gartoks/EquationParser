using System;

namespace EquationParser.BooleanAlgebra.Equations {
    public class Variable : ILogic {

        public readonly int Index;

        public Variable(int index) {
            Index = index;
        }

        public bool Evaluate(bool[] variables, Action<string> errorCallback) {
            if (Index >= variables.Length) {
                errorCallback("Variable not found");
                return default(bool);
            }

            return variables[Index];
        }
    }
}
