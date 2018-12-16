using System;
using System.Collections.Generic;

namespace EquationParser.Equations.Equations {
    internal interface IEquation {
        float Evaluate(float[] variables, List<float> equationVariables, Action<string> errorCallback);
    }
}
