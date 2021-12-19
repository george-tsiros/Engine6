namespace Gl;

public static partial class User {
    public enum RawInputDeviceCommand:uint {
        PreparsedData = 0x20000005,
        DeviceName = 0x20000007,
        DeviceInfo = 0x2000000b,
    }
}
