using System.Runtime.Intrinsics.X86;
using System.Text;

namespace HLE.Marshalling;

public static unsafe class CpuId
{
    public static string ManufacturerId { get; } = GetProcessorManufacturerId();

    private const int ManufacturerIdLength = 12;

    private static string GetProcessorManufacturerId()
    {
        (int _, int ebx, int ecx, int edx) = X86Base.CpuId(0, 0);
        int* manufacturerId = stackalloc int[3] { ebx, edx, ecx };
        return Encoding.ASCII.GetString((byte*)manufacturerId, ManufacturerIdLength);
    }
}
