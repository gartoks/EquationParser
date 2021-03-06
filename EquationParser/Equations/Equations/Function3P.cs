﻿using System;
using System.Collections.Generic;

namespace EquationParser.Equations.Equations {
    public class Function3P : IEquation {

        internal readonly Func<float, float, float, float> function;
        internal readonly IEquation x0;
        internal readonly IEquation x1;
        internal readonly IEquation x2;
        internal readonly Func<float, float, float, Action<string>, bool> validityChecker;

        internal Function3P(Func<float, float, float, float> function, IEquation x0, IEquation x1, IEquation x2, Func<float, float, float, Action<string>, bool> validityChecker = null) {
            this.function = function;
            this.x0 = x0;
            this.x1 = x1;
            this.validityChecker = validityChecker;
        }

        public float Evaluate(float[] variables, List<float> equationVariables, Action<string> errorCallback) {
            float v0 = x0.Evaluate(variables, equationVariables, errorCallback);
            float v1 = x1.Evaluate(variables, equationVariables, errorCallback);
            float v2 = x2.Evaluate(variables, equationVariables, errorCallback);

            if (this.validityChecker != null && this.validityChecker(v0, v1, v2, errorCallback))
                return default(float);

            return this.function(v0, v1, v2);
        }
    }
}
