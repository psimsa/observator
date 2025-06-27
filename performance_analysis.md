# Performance Analysis and Optimization Plan for Observator Generator

This document details the analysis and proposed optimization strategies for the `GetAttributedMethodsFromReferences` method, as identified in `todo.md` (Section 2: Performance).

## 2. Performance: `GetAttributedMethodsFromReferences` Optimization

### Analysis Recap
The `GetAttributedMethodsFromReferences` method within `InterceptorGenerator.cs` currently iterates through all types and methods across all referenced assemblies. For larger projects with numerous references, this comprehensive iteration can significantly degrade performance, acting as a bottleneck in the code generation process.

### Optimization Options

To mitigate the performance issues identified, the following optimization strategies can be explored:

1.  **Caching Mechanism:**
    *   **Option Description:** Implement a caching layer for the results of `GetAttributedMethodsFromReferences`. This would store previously computed results, allowing for quicker retrieval on subsequent calls rather than re-iterating through all assemblies.
    *   **Pros:** Significant performance improvement for repeated builds or incremental changes where the set of attributed methods doesn't change.
    *   **Cons:** Requires careful invalidation logic to ensure cache coherence when relevant source code or references change. Adds complexity to the generator.
    *   **Implementation Considerations:**
        *   Determine appropriate cache key (e.g., hash of relevant assembly versions, compilation state).
        *   Choose a suitable caching strategy (e.g., in-memory cache, potentially leveraging Roslyn's `IncrementalGenerator` context for more robust caching across compilations).

2.  **Targeted Assembly Scanning:**
    *   **Option Description:** Instead of scanning *all* referenced assemblies, identify and scan only those assemblies that are likely to contain `ObservatorTraceAttribute`. This could involve configuration (e.g., a list of assemblies to scan) or heuristic-based detection (e.g., assemblies that directly reference `Observator.Abstractions`).
    *   **Pros:** Reduces the scope of the search, leading to faster initial processing.
    *   **Cons:** May require manual configuration or intelligent heuristics that could miss valid attributed methods if not carefully designed. Could be brittle if project structure changes.
    *   **Implementation Considerations:**
        *   Investigate Roslyn APIs to efficiently determine direct or indirect references to `Observator.Abstractions`.
        *   Consider adding an MSBuild property for users to explicitly list assemblies to scan.

3.  **Leveraging Roslyn's Incremental Features for External Symbols:**
    *   **Option Description:** Utilize the capabilities of Roslyn's `IncrementalGenerator` API to more effectively track and react to changes in external symbols (types and methods in referenced assemblies). This involves using `IncrementalValueProvider` and `IncrementalValuesProvider` to declare inputs that the generator depends on, allowing Roslyn to optimize recompilations.
    *   **Pros:** The most robust and idiomatic approach for Roslyn source generators, designed for incremental compilation. Roslyn handles much of the invalidation logic.
    *   **Cons:** Higher initial learning curve and implementation complexity compared to simple caching.
    *   **Implementation Considerations:**
        *   Identify the `Compilation` and `ReferencedAssemblies` as `IncrementalValueProvider` inputs.
        *   Define `IncrementalValuesProvider` for types with `ObservatorTraceAttribute` across referenced assemblies.
        *   Ensure the generator's execution pipeline correctly leverages these incremental inputs.

### Proposed Plan

1.  **Phase 1: Research and Design (Focus on Roslyn Incremental Features)**
    *   Deep dive into Roslyn `IncrementalGenerator` capabilities, specifically how they handle external symbol analysis and caching.
    *   Evaluate how `GetAttributedMethodsFromReferences` can be refactored to fit into an incremental pipeline.
    *   Design the caching strategy within the `IncrementalGenerator` context, if direct caching is still deemed necessary alongside incremental features.
    *   Investigate the feasibility of targeted assembly scanning as a preliminary filter within the incremental pipeline.

2.  **Phase 2: Proof of Concept (PoC)**
    *   Implement a minimal PoC demonstrating the chosen optimization strategy (likely focusing on Roslyn incremental features).
    *   Measure the performance impact of the PoC using existing test projects.

3.  **Phase 3: Integration and Refinement**
    *   Integrate the optimized logic into `InterceptorGenerator.cs`.
    *   Thoroughly test the changes, including unit and integration tests, to ensure correctness and performance gains.
    *   Document the implementation details and any new configuration options.

4.  **Phase 4: Monitoring and Future Enhancements**
    *   Monitor performance in real-world scenarios (if possible).
    *   Consider further refinements or additional optimization techniques based on observed performance.
