using System;

namespace EquationParser.BooleanAlgebra.Equations {
    internal interface ILogic {
        bool Evaluate(bool[] variables, Action<string> errorCallback);
    }
}
