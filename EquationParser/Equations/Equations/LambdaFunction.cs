using System;
using System.Collections.Generic;

namespace EquationParser.Equations.Equations {
    public class LambdaFunction : IEquation {

        internal readonly Func<float, float, float> function;
        internal readonly IEquation s;
        internal readonly IEquation d;
        internal readonly IEquation i;
        internal readonly IEquation eq;

        internal LambdaFunction(Func<float, float, float> function, IEquation s, IEquation d, IEquation i, IEquation eq) {
            this.function = function;
            this.s = s;
            this.d = d;
            this.i = i;
            this.eq = eq;
        }

        public float Evaluate(float[] variables, List<float> equationVariables, Action<string> errorCallback) {
            float s = this.s.Evaluate(variables, equationVariables, errorCallback);
            float d = this.d.Evaluate(variables, equationVariables, errorCallback);
            float i = this.i.Evaluate(variables, equationVariables, errorCallback);

            int varCounterIdx = equationVariables.Count;
            equationVariables.Add(0);

            float result = 0;
            float it = s;
            while (it < d) {
                equationVariables[varCounterIdx] = it;
                result = this.function(result, this.eq.Evaluate(variables, equationVariables, errorCallback));
                it += i;
            }

            equationVariables.RemoveAt(varCounterIdx);

            return result;
        }
    }
}
