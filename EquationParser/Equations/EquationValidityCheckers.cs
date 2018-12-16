using System;

namespace EquationParser.Equations {
    internal static class EquationValidityCheckers {
        internal static Func<float, Action<string>, bool> validity_checker_p1_ln = (v0, eCb) => { if (v0 <= 0) { eCb("Natural logarithm is not defined for values <= 0."); return true; } return false; };
        internal static Func<float, Action<string>, bool> validity_checker_p1_sqrt = (v0, eCb) => { if (v0 < 0) { eCb("Square root is not defined for values < 0."); return true; } return false; };
        internal static Func<float, float, Action<string>, bool> validity_checker_p2_nrt = (v0, v1, eCb) => { if (v0 < 0) { eCb("N-th root is not defined for values < 0."); return true; } return false; };
        internal static Func<float, float, Action<string>, bool> validity_checker_p2_difference = (v0, v1, eCb) => { if (v1 == 0) { eCb("Cannot divide by 0."); return true; } return false; };
        internal static Func<float, float, Action<string>, bool> validity_checker_p2_atan2 = (v0, v1, eCb) => { if (v1 == 0) { eCb("ATan2 : Cannot divide by 0."); return true; } return false; };
        internal static Func<float, float, Action<string>, bool> validity_checker_p2_log = (v0, v1, eCb) => { if (v0 <= 0) { eCb("Logarithm is not defined for values <= 0."); return true; } return false; };
    }
}
