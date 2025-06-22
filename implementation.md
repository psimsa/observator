# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---

# Observator Implementation Plan

## Project Overview
Observator is an AOT-compatible source generator package that automatically instruments .NET assemblies with OpenTelemetry tracing, logging, and metrics capabilities using compile-time code generation and interceptors.

## Architecture

### Core Principles
- **AOT-First Design**: Zero runtime reflection, all code generation at compile-time
- **Minimal Dependencies**: Only reference `System.Diagnostics.DiagnosticSource` (built into .NET)
- **Zero-Config**: Works out-of-the-box with sensible defaults
- **Attribute-Driven**: Selective instrumentation via method decoration
- **Cross-Assembly Compatible**: Works across project boundaries

### Component Architecture
```
Observator.Generator (netstandard2.0)
├── InfrastructureGenerator.cs      # ActivitySource, Meter, Attributes generation
├── InterceptorGenerator.cs         # Method call interception
├── Diagnostics/
│   ├── AsyncMethodAnalyzer.cs      # Warns about Task-returning non-async methods
│   ├── LoggerFieldAnalyzer.cs      # Validates logger field availability
│   └── DiagnosticDescriptors.cs    # All diagnostic definitions
├── Templates/
│   ├── InfrastructureTemplate.cs   # Code templates for generated infrastructure
│   └── InterceptorTemplate.cs      # Code templates for interceptors
└── Utils/
    ├── SyntaxHelpers.cs            # Roslyn syntax utilities
    └── AssemblyInfoExtractor.cs    # Assembly metadata extraction
```

## Implementation Phases

### Phase 1: Infrastructure Generation (Week 1-2)
**Status: ✅ Completed**

All goals for Phase 1 have been met:
- [x] Source generator project setup and dependencies
- [x] Assembly metadata extraction
- [x] Infrastructure code generation (ActivitySource, Meter, Attribute)
- [x] TestApp integration and verification
- [x] Console output confirms generated code is valid and usable

---

### Phase 2: Basic Interceptor Generation (Week 3-4)
**Goal**: Intercept calls to attributed methods with basic tracing

#### 2.2 InterceptorGenerator Implementation Plan
- Discover all methods decorated with `[ObservatorTrace]` using Roslyn.
- For each such method, find all call sites in the solution.
- For each call site, generate a static interceptor method with `[InterceptsLocation]` that:
  - Starts an Activity using the generated ActivitySource
  - Calls the original method
  - Handles both sync and async methods (await if needed)
  - Sets Activity status on error
- Emit all interceptors as a `.g.cs` file in `Observator.Generated`.
- Test in TestApp: only attributed methods should be intercepted, console output should show Activity start/stop.
- Handle edge cases: overloads, static/instance, async, diagnostics for unsupported patterns.

Next: Implement `InterceptorGenerator.cs` in `Observator.Generator` and update TestApp for verification.

---