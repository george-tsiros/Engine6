namespace Gl;

using System;
using System.Collections;
using System.Collections.Generic;

class ArEnum<T>:IEnumerator<T> {
    public T Current {
        get {
            if (Index < 0 || Count <= Index)
                throw new InvalidOperationException();
            var offset = StartAt - Index;
            var i = offset < 0
                ? Elements.Length - offset
                : offset;
            return Elements[offset < 0 ? offset + Elements.Length : offset];
        }
    }

    object IEnumerator.Current => Current;

    public void Dispose () { }
    public bool MoveNext () {
        return ++Index < Count;
    }

    public void Reset () {
        Index = -1;
    }
    int Index = -1;
    readonly T[] Elements;
    readonly int Count;
    readonly int StartAt;
    internal ArEnum (T[] elements, int count, int startAt) {
        (Elements, Count, StartAt) = (elements, count, startAt);
    }
}
