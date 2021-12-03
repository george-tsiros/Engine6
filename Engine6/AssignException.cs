using System;
using System.Runtime.Serialization;

namespace Engine;

[Serializable]
internal class AssignException:Exception {
    public AssignException () { }

    public AssignException (string message) : base(message) { }

    public AssignException (string message, Exception innerException) : base(message, innerException) { }

    protected AssignException (SerializationInfo info, StreamingContext context) : base(info, context) { }
}
