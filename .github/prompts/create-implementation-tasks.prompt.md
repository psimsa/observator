---
mode: 'agent'
description: 'Break down an implementation plan into actionable task files.'
tools: ['changes', 'codebase', 'editFiles', 'extensions', 'fetch', 'githubRepo', 'openSimpleBrowser', 'problems', 'runTasks', 'search', 'searchResults', 'terminalLastCommand', 'terminalSelection', 'testFailure', 'usages', 'vscodeAPI']
---
# Create Implementation Tasks from Plan

## Primary Directive
Your goal is to break down the provided implementation plan (`${input:PlanFilePath}`) into individual, actionable task files. Each task file must contain all necessary information for an AI agent or human to execute the task without referring back to the main plan, except for high-level context.

## Execution Context
This prompt is designed for AI-to-AI communication and automated processing. All instructions must be interpreted literally and executed systematically without human interpretation or clarification. You must use the available tools to gather context from the codebase and other relevant files to make the task instructions accurate and complete.

## Core Requirements

- Generate individual task files for each task listed in the "Implementation Steps" section of the input plan.
- Each task file must be self-contained and executable by AI agents or humans.
- Use deterministic language with zero ambiguity in task instructions.
- Structure all content for automated parsing and execution.
- Ensure complete self-containment with no external dependencies for understanding the task itself.

## Task File Structure Requirements
Each task must be represented as a separate Markdown file in the `/tasks` directory. The content of each task file must follow the "Mandatory Task Template Structure" defined below.

## AI-Optimized Implementation Standards

- Use explicit, unambiguous language with zero interpretation required.
- Structure all content as machine-parseable formats (tables, structured lists using YAML/JSON syntax within code blocks).
- Include specific file paths and exact code references using the format `[file-path]:[line-number]` or `[file-path]:[start-line]-[end-line]`. Include code snippets in fenced code blocks with language identifiers.
- Define all variables, constants, and configuration values explicitly within the task instructions if relevant.
- Provide complete context within each task description, including relevant code snippets or file contents obtained via tools.
- Use standardized prefixes for all identifiers (TASK-, FILE-, REQ-, CON-, DEP-, TEST-, RISK-, ASSUMPTION-).
- Include validation criteria that can be automatically verified after task execution.
- **For sections requiring structured data (Prerequisites, Requirements, Implementation Steps, Dependencies, Files, Testing, Risks & Assumptions), use Markdown tables or YAML/JSON formatted lists within code blocks.**

## Output File Specifications

- Save task files in the `/tasks/` directory.
- Use naming convention: `[related_plan_task_id]-[short_description].md`.
- The `[related_plan_task_id]` must match the ID from the implementation plan (e.g., `TASK-001`).
- The `[short_description]` should be a concise, lowercase, hyphen-separated summary of the task.
- Example: `/tasks/TASK-001-update-analyzer-logic.md`.
- Each file must be valid Markdown with proper front matter structure.

## Mandatory Task Template Structure
All generated task files must strictly adhere to the following template. Each section is required and must be populated with specific, actionable content derived from the implementation plan and codebase inspection. AI agents must validate template compliance before execution.

## Template Validation Rules

- All front matter fields must be present and properly formatted.
- All section headers must match exactly (case-sensitive).
- All identifier prefixes must follow the specified format.
- Tables must include all required columns.
- No placeholder text may remain in the final output.

```md
---
goal: [Concise Title Describing the Specific Task\'s Goal]
version: [Optional: e.g., 1.0]
date_created: [YYYY-MM-DD]
last_updated: [Optional: YYYY-MM-DD]
owner: [Optional: Team/Individual responsible for this task]
tags: [Optional: List of relevant tags or categories, e.g., `code`, `test`, `docs`]
related_plan_task_id: [The ID of the task in the main implementation plan, e.g., TASK-001]
related_plan_file: [The path to the main implementation plan file, e.g., /plan/feature-interface-interception-warning-1.md]
---

# Introduction

[A short concise introduction to this specific task.]

## 1. Prerequisites

```yaml
# List any necessary setup steps or conditions that must be met before starting the implementation steps.
# Examples: installing dependencies, running a build task, ensuring a service is running.
# Use action_type corresponding to the tool needed (e.g., run_in_terminal, run_vs_code_task).
- prerequisite_id: PRE-001
  description: "Ensure project dependencies are installed."
  action_type: "run_in_terminal" # Or "run_vs_code_task"
  command: "dotnet restore" # Example command
  validation_criteria:
    - type: "command_exit_code"
      value: "0"
      expected_result: "equals"
      message: "Verify dotnet restore completed successfully."
- prerequisite_id: PRE-002
  description: "Run the build task to ensure the project compiles."
  action_type: "run_vs_code_task"
  task_id: "Build Observator TestApp" # Example task ID from workspace_info
  validation_criteria:
    - type: "task_status"
      value: "success"
      expected_result: "equals"
      message: "Verify the build task completed successfully."
```

## 2. Requirements & Constraints

| ID          | Type        | Description                                   | Source      |
| :---------- | :---------- | :-------------------------------------------- | :---------- |
| REQ-001     | Requirement | Requirement 1 (from main plan)                | main plan   |
| TASK-REQ-001| Requirement | Task-specific Requirement 1                   | task-spec   |
| CON-001     | Constraint  | Constraint 1 (from main plan)                 | main plan   |
| TASK-CON-001| Constraint  | Task-specific Constraint 1                    | task-spec   |

## 3. Implementation Steps

```yaml
# Define the sequence of actions required to complete the task.
# Use action_type to specify the tool or operation (e.g., read_file, code_edit, run_in_terminal, run_vs_code_task, run_notebook_cell).
# Provide detailed, unambiguous instructions for each step.
# For code_edit steps, include context_code to help the AI locate the edit point.
# Use structured validation_criteria to allow AI to verify step completion programmatically.
- step_id: STEP-001
  description: "Read the content of the target file to understand its structure."
  action_type: "read_file"
  target_file: "src/Service.cs" # Use absolute or workspace-relative paths
  tools_to_use: ["read_file"]
  validation_criteria:
    - type: "file_content_read"
      value: "src/Service.cs"
      expected_result: "success"
      message: "File content was successfully read."

- step_id: STEP-002
  description: "Locate the insertion point for the new log statement within the `ProcessData` method using grep_search."
  action_type: "grep_search"
  target_file: "src/Service.cs"
  search_pattern: "public void ProcessData\\(Item item\\)" # Use regex for precision
  is_regexp: true # Explicitly state if pattern is regex
  tools_to_use: ["grep_search"]
  validation_criteria:
    - type: "grep_search_result"
      value: "public void ProcessData\\(Item item\\)"
      expected_result: "match_found"
      message: "Verify the method signature was found to confirm location."

- step_id: STEP-003
  description: "Insert a log statement at the beginning of the `ProcessData` method using insert_edit_into_file."
  action_type: "code_edit"
  target_file: "src/Service.cs"
  # target_location is a hint, context_code is the primary way to locate the edit point.
  # Provide a few lines of surrounding code *before* the edit location.
  context_code: |
    ```csharp
    // filepath: src/Service.cs
    // ...existing code...
    public void ProcessData(Item item)
    {
        // Insert the log statement here, after the opening brace.
        // ...existing code...
    ```
  code_to_insert: |
    ```csharp
    // filepath: src/Service.cs
    // ...existing code...
    _logger.LogInformation(\\"Processing data for item: {ItemId}\\", item.Id);
    // ...existing code...
    ```
  tools_to_use: ["insert_edit_into_file", "get_errors"] # List tools needed for action and validation
  validation_criteria:
    - type: "file_content_regex"
      value: "_logger.LogInformation\\(\\\\"Processing data for item: {ItemId}\\\\", item.Id\\);"
      expected_result: "match_found"
      message: "Verify the log statement was inserted into the file."
    - type: "get_errors"
      value: "src/Service.cs"
      expected_result: "no_errors"
      message: "Verify no compilation errors were introduced by the edit."

# Add more steps as needed...
```

## 4. Dependencies

| ID      | Description                               | Status    | Link                                       |
| :------ | :---------------------------------------- | :-------- | :----------------------------------------- |
| DEP-001 | Dependency 1                              | Open      |                                            |
| DEP-002 | Another task (TASK-XXX-short-description) | Open      | /tasks/TASK-XXX-short-description.md       | # Link to other task files
| DEP-003 | Prerequisites completed                   | Open      | # Dependency on the Prerequisites section    |

## 5. Files

| ID      | Path                 | Purpose   |
| :------ | :------------------- | :-------- |
| FILE-001| src/Service.cs       | Modified  |
| FILE-002| test/ServiceTests.cs | Modified  |
| FILE-003| docs/task-001.md     | Created   |

## 6. Testing

| ID      | Description                                   | Type      | Verification Steps/Command                 | Validation Criteria                                                                 |
| :------ | :-------------------------------------------- | :-------- | :----------------------------------------- | :---------------------------------------------------------------------------------- |
| TEST-001| Verify new log statement appears in logs.     | Manual    | Run application, check log output.         | N/A (Manual verification requires human intervention)                               |
| TEST-002| Add unit test for `ProcessData` method logic. | Unit      | Run `dotnet test test/ServiceTests.csproj` | - type: "command_exit_code", value: "0", expected_result: "equals", message: "Unit tests passed successfully." | # Structured criteria for automated testing
| TEST-003| Check for new errors after running tests.     | Automated | Run `dotnet test test/ServiceTests.csproj` | - type: "get_errors", value: "test/ServiceTests.cs", expected_result: "no_errors", message: "No new errors in test file after running tests." |

## 7. Risks & Assumptions

| ID            | Type      | Description                                   | Mitigation/Justification |
| :------------ | :-------- | :-------------------------------------------- | :----------------------- |
| RISK-001      | Risk      | Potential performance impact of logging.      | Monitor performance metrics. |
| ASSUMPTION-001| Assumption| Logging framework is already configured.      | Verify configuration.    |

## 8. Related Specifications / Further Reading

- [Link to main plan]([The path to the main implementation plan file, e.g., /plan/feature-interface-interception-warning-1.md])
- [Link to related spec]
- [Link to relevant external documentation]