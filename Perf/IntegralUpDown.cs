namespace Perf {
    using System.Windows.Forms;

    class IntegralUpDown:NumericUpDown {
        new public int Value {
            get => (int)base.Value;
            set => base.Value = value;
        }
    }
}
