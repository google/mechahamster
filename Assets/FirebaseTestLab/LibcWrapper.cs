#if UNITY_ANDROID

using System.Runtime.InteropServices;

namespace FirebaseTestLab {
  public class LibcWrapper {
    [DllImport("c")]
    public static extern int dup(int fd);

    [DllImport("c")]
    public static extern int write(int fd, string buf, int count);

    public static int WriteToFileDescriptor(int fileDescriptor, string message) {
      return write(fileDescriptor, message, message.Length);
    }
  }
}

#endif // UNITY_ANDROID