namespace Gl;

using System;
using System.Collections;
using System.Collections.Generic;

public class CircularBuffer<T>:IEnumerable<T> {
    readonly T[] Buffer;
    private int Index;
    private bool Full;
    public CircularBuffer (int depth) {
        if (depth < 2)
            throw new ArgumentOutOfRangeException(nameof(depth), "must be at least 2");
        Buffer = new T[depth];
    }

    public int Count => Full ? Buffer.Length : Index;
    public void Add (T t) {
        Buffer[Index] = t;
        if (++Index == Buffer.Length) {
            Index = 0;
            Full = true;
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator () =>
        new ArEnum<T>(Buffer, Count, Index - 1);

    IEnumerator IEnumerable.GetEnumerator () =>
        new ArEnum<T>(Buffer, Count, Index - 1);


    public T this[int i] {
        get {
            var count = Count;
            if (i < 0 || count <= i)
                throw new ArgumentOutOfRangeException(nameof(i));
            // move i+1 positions back in the array
            var index = Index - i - 1;
            if (index < 0)
                index += Buffer.Length;
            return Buffer[index];
        }
    }
}
