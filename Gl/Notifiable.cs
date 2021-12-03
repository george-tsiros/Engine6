namespace Shaders;

using System;

public struct Notifiable<T> where T : struct, IEquatable<T> {
    public static implicit operator T (Notifiable<T> self) => self.Value;
    private T v;
    public bool Changed { get; private set; }
    public T Value {
        get {
            Changed = false;
            return v;
        }
        set {
            if (Changed = !v.Equals(value))
                v = value;
        }
    }
}
