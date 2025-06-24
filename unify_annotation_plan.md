### Detailed Implementation Plan: Unifying Observator Trace Attributes

The goal is to unify the `ObservatorTraceAttribute` to be applicable to interfaces, classes, and methods, thereby deprecating `ObservatorInterfaceTraceAttribute`.

#### 1. Define a unified `ObservatorTraceAttribute`

*   **Action:** Move the existing `ObservatorTraceAttribute` definition from `src/Observator.Generator/InfrastructureGenerator.cs` to a new file `src/Observator.Abstractions/ObservatorTraceAttribute.cs`.
*   **Action:** Modify the `AttributeUsage` of the new `ObservatorTraceAttribute` to allow it to be applied to `Interface`, `Class`, and `Method`.
*   **Action:** Delete the existing `src/Observator.Abstractions/ObservatorInterfaceTraceAttribute.cs` file.
*   **Reasoning:** Centralizes the attribute definition in the `Abstractions` project, making it accessible to all projects that need to use it, and expands its applicability.

#### 2. Update `ObservatorConstants.cs`

*   **Action:** Remove all constants related to `ObservatorInterfaceTraceAttribute` (e.g., `ObservatorInterfaceTraceAttributeFullName`, `ObservatorInterfaceTraceAttributeName`, `ObservatorInterfaceTraceAttributeShortName`).
*   **Action:** Verify that the constants for `ObservatorTraceAttribute` (`ObservatorTraceAttributeFullName`, `ObservatorTraceAttributeName`, `ObservatorTraceShortName`) are correctly defined and point to the unified attribute's full name, name, and short name.
*   **Reasoning:** Cleans up unused constants and ensures consistency with the unified attribute.

#### 3. Update `MethodAnalyzer.cs`

*   **Action:** Modify the `AnalyzeMethodDeclaration` method to only check for `ObservatorConstants.ObservatorTraceAttributeFullName`, `ObservatorConstants.ObservatorTraceAttributeName`, or `ObservatorConstants.ObservatorTraceShortName` when identifying methods to intercept. Remove all checks for `ObservatorInterfaceTraceAttribute`.
*   **Action:** Adjust the logic for handling interface methods within `AnalyzeMethodDeclaration` to also rely solely on `ObservatorTraceAttribute`. Specifically, if a method is abstract or from an interface, it should only be considered for interception if `ObservatorTraceAttribute` is present.
*   **Action:** Modify the `AnalyzeInterfaceDeclaration` method to check for `ObservatorTraceAttribute` on both the interface symbol and its public methods. The method should be renamed to `AnalyzeTypeDeclaration` to reflect its broader applicability to classes as well.
*   **Reasoning:** Adapts the analysis logic to the unified attribute, simplifying the attribute checking process.

#### 4. Update `InterceptorGenerator.cs`

*   **Action:** Adjust the `interfaceMethods` syntax provider query to look for `ObservatorTraceAttribute` instead of `ObservatorInterfaceTraceAttribute`. This will involve updating the `predicate` and `transform` logic.
*   **Action:** Verify that the `externalAttributedMethods` logic correctly identifies methods with the unified `ObservatorTraceAttribute`. (Based on the current code, this section already checks for `ObservatorTraceAttributeFullName`, `ObservatorTraceAttributeName`, and `ObservatorTraceShortName`, so it might require minimal changes, if any, related to the new `AttributeUsage`.)
*   **Reasoning:** Ensures that the generator correctly identifies methods and interfaces for interception using the unified attribute.

#### 5. Update `DiagnosticDescriptors.cs`

*   **Action:** Review `DiagnosticDescriptors.cs` for any diagnostic messages that might be affected by the removal of `ObservatorInterfaceTraceAttribute` or the unification of attributes. Update or remove them as necessary.
*   **Reasoning:** Maintains the accuracy and relevance of diagnostic messages.

#### 6. Refactor `TestApp` and `TestLib` usages

*   **Action:** Identify all occurrences of `ObservatorInterfaceTraceAttribute` in `test/TestApp` and `test/TestLib` projects and replace them with `ObservatorTraceAttribute`.
*   **Action:** Specifically, examine `test/TestApp/IMyService.cs` and `test/TestApp/MyService.cs` as likely candidates for changes.
*   **Reasoning:** Ensures that the test projects correctly use the unified attribute and validate the changes.

#### 7. Build and Test

*   **Action:** Compile the solution using `dotnet build`.
*   **Action:** Run the `TestApp` using `dotnet run --project test/TestApp`.
*   **Action:** Verify that the compilation is successful without errors and that the application yields expected results, confirming the interception logic works as intended with the unified attribute.
*   **Reasoning:** Essential step to validate the entire refactoring process.

### Mermaid Diagram for the Plan Flow

```mermaid
graph TD
    A[Start: Unify Annotation Approach] --> B{Define unified ObservatorTraceAttribute};
    B --> B1[Move ObservatorTraceAttribute to Abstractions];
    B1 --> B2[Modify AttributeUsage to Interface, Class, Method];
    B2 --> B3[Delete ObservatorInterfaceTraceAttribute.cs];
    B3 --> C{Update ObservatorConstants.cs};
    C --> C1[Remove ObservatorInterfaceTraceAttribute constants];
    C1 --> C2[Verify ObservatorTraceAttribute constants];
    C2 --> D{Update MethodAnalyzer.cs};
    D --> D1[Modify AnalyzeMethodDeclaration to use only ObservatorTraceAttribute];
    D1 --> D2[Adjust interface method logic in AnalyzeMethodDeclaration];
    D2 --> D3[Modify AnalyzeInterfaceDeclaration to use ObservatorTraceAttribute and rename to AnalyzeTypeDeclaration];
    D3 --> E{Update InterceptorGenerator.cs};
    E --> E1[Adjust interfaceMethods query to use ObservatorTraceAttribute];
    E1 --> E2[Verify externalAttributedMethods logic];
    E2 --> F{Update DiagnosticDescriptors.cs};
    F --> F1[Review and update diagnostic messages];
    F1 --> G{Refactor TestApp and TestLib usages};
    G --> G1[Replace ObservatorInterfaceTraceAttribute with ObservatorTraceAttribute];
    G1 --> H{Build and Test};
    H --> H1[Compile solution: dotnet build];
    H1 --> H2[Run TestApp: dotnet run --project test/TestApp];
    H2 --> H3[Verify compilation and expected results];
    H3 --> I[End: Plan Completed];