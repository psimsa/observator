# Cross-Assembly Interception in Observator

## Problem Statement

When using Observator's source generator to instrument methods with [ObservatorTrace], interception works as expected for methods defined and called within the same assembly. However, when an attributed method is defined in one assembly (e.g., a library) and called from another (e.g., an app), interception does **not** work at the call site in the referencing assembly.

## Root Cause

- The .NET 9 interception mechanism requires the `[InterceptsLocation]` attribute and the generated interceptor method to be present in the assembly where the call site exists.
- The generator only emits interceptors for methods where it can find the method declaration (i.e., in the defining assembly), not for call sites in referencing assemblies.
- C# does **not** allow a partial class to be continued across assemblies. Synthesizing a partial class for an external type in the referencing assembly is ignored by the compiler and does not work.

## Options Already Tried

1. **Emit interceptors in referencing assembly:**
   - Attempted to synthesize a partial class for the external type and emit the interceptor in the referencing assembly.
   - Result: The compiler ignores the synthesized partial class; no code is emitted.

2. **Emit interceptors for all call sites:**
   - Attempted to generate interceptors for every call site, using the method symbol for signature and call.
   - Result: Fails for external types due to the above limitation.

## Other Possibilities

- **Emit interceptors only in the defining assembly:**
  - Works for internal calls, but not for external call sites.
  - Referencing assemblies cannot participate in interception for external types.

- **Proxy/Wrapper Pattern:**
  - Generate a proxy or wrapper class in the referencing assembly that delegates to the external method and applies observability logic.
  - Requires consumers to use the proxy instead of the original type.

- **NuGet/Analyzer Distribution:**
  - Ship the generated interceptors as part of the library package, so referencing assemblies have access to the generated code.
  - Still does not solve the call site interception problem for external types.

- **Emit Diagnostic:**
  - The generator can emit a diagnostic or warning when it detects a cross-assembly call site that cannot be intercepted, explaining the limitation.

## Conclusion

Due to C# and Roslyn limitations, true cross-assembly interception via source generators is not currently possible. The generator cannot synthesize partial classes for external types in referencing assemblies. The only robust solutions are to:
- Use proxies/wrappers for cross-assembly scenarios.
- Emit diagnostics to inform users of the limitation.

---

*This file documents the current state, attempted solutions, and possible workarounds for cross-assembly interception in Observator.*
