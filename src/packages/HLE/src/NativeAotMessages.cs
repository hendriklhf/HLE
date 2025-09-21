namespace HLE;

internal static class NativeAotMessages
{
    public const string RequiresDynamicCode = "The native code for this instantiation might not be available at runtime.";
    public const string RequiresUnreferencedCode = "If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.";
}
