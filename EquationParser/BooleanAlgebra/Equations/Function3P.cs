using System;

namespace EquationParser.BooleanAlgebra.Equations {
    public class Function3P : ILogic {

        internal readonly Func<bool, bool, bool, bool> function;
        internal readonly ILogic x0;
        internal readonly ILogic x1;
        internal readonly ILogic x2;

        internal Function3P(Func<bool, bool, bool, bool> function, ILogic x0, ILogic x1, ILogic x2) {
            this.function = function;
            this.x0 = x0;
            this.x1 = x1;
        }

        public bool Evaluate(bool[] variables, Action<string> errorCallback) {
            bool v0 = x0.Evaluate(variables, errorCallback);
            bool v1 = x1.Evaluate(variables, errorCallback);
            bool v2 = x2.Evaluate(variables, errorCallback);
            return this.function(v0, v1, v2);
        }
    }
}
