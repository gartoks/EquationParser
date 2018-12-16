using System;

namespace EquationParser.BooleanAlgebra.Equations {
    public class Function1P : ILogic {

        internal readonly Func<bool, bool> function;
        internal readonly ILogic x0;

        internal Function1P(Func<bool, bool> function, ILogic x0) {
            this.function = function;
            this.x0 = x0;
        }

        public bool Evaluate(bool[] variables, Action<string> errorCallback) {
            bool v0 = x0.Evaluate(variables, errorCallback);
            return this.function(v0);
        }
    }
}
