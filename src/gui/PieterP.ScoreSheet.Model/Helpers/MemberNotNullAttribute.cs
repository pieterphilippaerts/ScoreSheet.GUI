
// The MemberNotNullAttribute class is defined in the System.Diagnostics.CodeAnalysis namespace and is used to indicate that certain members
// of a class are guaranteed to be non-null after the method or property is called. This attribute is particularly useful for static analysis
// tools and code contracts.
// We need this compatibility definition for .NET Framework 4.8 because the MemberNotNullAttribute is not available in that version of the framework.

#if NET48
namespace System.Diagnostics.CodeAnalysis {
    [System.AttributeUsage(
        System.AttributeTargets.Method |
        System.AttributeTargets.Property,
        Inherited = false,
        AllowMultiple = true)]
    internal sealed class MemberNotNullAttribute : System.Attribute {
        public MemberNotNullAttribute(string member) {
            Members = new[] { member };
        }

        public MemberNotNullAttribute(params string[] members) {
            Members = members;
        }

        public string[] Members { get; }
    }
}
#endif