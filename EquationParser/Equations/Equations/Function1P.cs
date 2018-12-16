using System;
using System.Collections.Generic;

namespace EquationParser.Equations.Equations {
    public class Function1P : IEquation {

        internal readonly Func<float, float> function;
        internal readonly IEquation x0;
        internal readonly Func<float, Action<string>, bool> validityChecker;

        internal Function1P(Func<float, float> function, IEquation x0, Func<float, Action<string>, bool> validityChecker = null) {
            this.function = function;
            this.x0 = x0;
            this.validityChecker = validityChecker;
        }

        public float Evaluate(float[] variables, List<float> equationVariables, Action<string> errorCallback) {
            float v0 = x0.Evaluate(variables, equationVariables, errorCallback);

            if (this.validityChecker != null && this.validityChecker(v0, errorCallback))
                return default(float);

            return this.function(v0);
        }
    }
}
