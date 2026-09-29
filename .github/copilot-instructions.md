# Smart Building Project Guidelines

## Git Workflow

- Before editing, check the working tree and current branch.
- Never make changes, commits, or pushes directly on `main` or `master`.
- When the current branch is protected, create and switch to a dedicated branch before editing.
- Name branches `<type>/<short-kebab-case-description>`.
- Allowed branch types: `feat`, `fix`, `chore`, `docs`, `test`, `refactor`, `perf`, and `ci`.
- Keep each branch focused on one phase or concern from the project plan.
- Use Conventional Commits: `<type>(optional-scope): <imperative summary>`.
- Do not push or merge without explicit user approval. Complete work through a pull request.

Examples:

```text
feat/access-control
fix/occupancy-session-close
chore/update-dependencies

feat(domain): add access permission validation
fix(api): return not found for unknown card
```

## Naming

- Use PascalCase for C# types, members, namespaces, and file names.
- Use camelCase for parameters and local variables; prefix interfaces with `I`.
- Suffix asynchronous methods with `Async`.
- Name tests `Subject_Scenario_ExpectedResult`.
- Use plural kebab-case API resources, for example `/api/access-points`.
- Use snake_case for PostgreSQL tables, columns, indexes, and constraints.
- Use the domain terms from `Docs/smart-building-project-plan.md`; do not introduce synonyms such as `Door` for `AccessPoint`.

## Delivery

- Implement one project phase at a time and keep Domain independent from frameworks.
- Run the narrowest relevant tests, then build the complete solution before requesting a commit.
- Report the branch name, validation result, and suggested commit message before any push.

## Documentation and Learning

- Treat documentation as part of the Definition of Done for every change.
- Update the relevant technical document in `Docs/` whenever behavior, architecture, dependencies, configuration, operations, or workflows change.
- Update the corresponding guide in `Docs/study/` with a simple explanation, technical definition, project example, interview answer, and practical exercise.
- Clearly distinguish implemented behavior from work in progress and planned behavior.
- Keep `Docs/README.md` and `Docs/study/README.md` indexes current when documents are added, renamed, or removed.
- Validate local Markdown links before requesting a commit.