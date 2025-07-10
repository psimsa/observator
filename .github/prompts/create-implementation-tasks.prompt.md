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
- Structure all content as machine-parseable formats (tables, lists, structured data).
- Include specific file paths, line numbers, and exact code references where applicable, using information gathered from codebase inspection tools.
- Define all variables, constants, and configuration values explicitly within the task instructions if relevant.
- Provide complete context within each task description, including relevant code snippets or file contents obtained via tools.
- Use standardized prefixes for all identifiers (TASK-, FILE-, etc.).
- Include validation criteria that can be automatically verified after task execution.

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
goal: [Concise Title Describing the Specific Task's Goal]
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

## 1. Requirements & Constraints

[List any requirements & constraints specifically relevant to this task, referencing the main plan's requirements as needed.]

- **REQ-001**: Requirement 1 (from main plan)
- **TASK-REQ-001**: Task-specific Requirement 1
- **CON-001**: Constraint 1 (from main plan)

## 2. Implementation Steps

[Detailed, step-by-step instructions for completing this task. Use information gathered from codebase inspection tools to make these steps precise and actionable. Include specific file paths, function names, code snippets, or commands to run.]

- Step 1: [Description of step 1, including file paths and code references]
- Step 2: [Description of step 2, including file paths and code references]

## 3. Dependencies

[List any dependencies that need to be addressed before this task can be completed.]

- **DEP-001**: Dependency 1

## 4. Files

[List the specific files that will be affected or created by this task.]

- **FILE-001**: Path to file 1
- **FILE-002**: Path to file 2

## 5. Testing

[List the specific tests that need to be run or implemented to verify the successful completion of this task.]

- **TEST-001**: Description of test 1
- **TEST-002**: Description of test 2

## 6. Risks & Assumptions

[List any risks or assumptions specific to the implementation of this task.]

- **RISK-001**: Task-specific Risk 1
- **ASSUMPTION-001**: Task-specific Assumption 1

## 7. Related Specifications / Further Reading

[Link to the main implementation plan, relevant specification files, or external documentation.]

- [Link to main plan]
- [Link to related spec]
- [Link to relevant external documentation]
```