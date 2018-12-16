using EquationParser.BooleanAlgebra.Equations;

namespace EquationParser.BooleanAlgebra {
    public enum SegmentType { Variable, Constant, Function, Operator, Equation }

    internal class SegmentData {

        internal readonly int Index;
        internal readonly string Data;
        internal readonly SegmentType Type;
        internal ILogic Equation;

        internal SegmentData(int index, string data, SegmentType type) {
            Index = index;
            Data = data;
            Type = type;
        }

        public override string ToString() {
            return $"[{Index}:{Type}]{Data}";
        }

    }
}
