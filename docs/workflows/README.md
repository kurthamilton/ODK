# Workflows

Some flows in this app vary by platform, by entry point and by what the member already has — account
creation most of all. Rather than let that live as branches inside long service methods, those flows are
modelled as **state machines**: a set of states, the triggers that move between them, the conditions that
decide which move is legal, and the ordered work each move performs.

The framework is `ODK.Core.Workflows`, and it depends on nothing — not even `ODK.Core`.

## The pages on this folder are generated

| Machine | Page | Scope |
|---|---|---|
| Account | [account.md](account.md) | Site-level: does an address have an account, and can it sign in |
| Chapter membership | [chapter-membership.md](chapter-membership.md) | Per group: invited, applied, joined |
| Chapter publication | [chapter-publication.md](chapter-publication.md) | Per group: draft, approved, published |

**Do not edit them by hand.** Each is produced by walking the definition the app actually executes, and
`WorkflowDocumentationTests` fails the build when a committed page no longer matches its definition —
regenerating it in the process, so the fix is to review the change and commit it. That test is the whole
reason these pages can be trusted: a diagram maintained separately from the code drifts, and this one
cannot.

The account and membership machines are deliberately separate. An account and a group membership are
independent lifecycles: signing up on Drunken Knitwits writes a membership *before* the account can sign in,
so a member can be `Joined` in one machine and `Registered` in the other, both true at once. Holding both in
one machine meant edges that existed only to reconcile them.

## How a definition is built

A definition is written in code — a fluent builder in a `static Create()` method — and validated when it is
built. Four rules shape it:

- **State is derived, never stored.** No workflow column anywhere. A state resolver computes the current
  state from the domain: whether an account is activated, whether an invitation is outstanding, whether a
  membership row exists and is approved. Two sources of truth for "is this activated" would be worse than
  the branching this replaced. The cost is that derivation must be *total*, which each machine has a test
  for: every combination of the domain it reads lands on exactly one state.
- **Guards are pure and self-describing.** A guard is a small class over the context that reads no database
  and takes no dependencies, carrying a description that reads as a condition — "requiring approval". That
  description is what labels an edge, so a diagram of unlabelled arrows is impossible by construction, and
  purity is what lets a guard be unit tested against a context you construct by hand.
- **The context is loaded once, up front.** A factory issues one batched round-trip and hands the machine
  everything its guards, resolver and steps will read. This is the same convention the view-model services
  follow, and it is what makes guards pure.
- **Steps declare what kind of work they do** — a decision, a write, a commit, or an irreversible external
  effect such as an email or a queued job. The builder uses that to enforce the atomicity rule the codebase
  states in prose: an external effect may not run while a write is still uncommitted, because an effect
  taken against state that can still roll back cannot be taken back.

The builder rejects a definition that breaks any of this, along with a state that is never entered or left,
a trigger never fired, a guard with no description, and a transition declared twice. Those are programming
errors, so they throw where the definition is built rather than failing a request.

## How a definition executes

A runner resolves the current state from the context, finds the transition whose trigger and guards match,
and runs that transition's steps in order, stopping at the first failure. A step that fails returns a
message written for the member, which the calling service surfaces as it would any other failure.

Steps are referenced by type in the definition and resolved from the container at execution time, which is
what lets a definition be built and walked with no container and no database — by the diagram generator, and
by tests. It also means **a machine registers its own steps**: the container learns them from the
definition, so a step added to a transition cannot be forgotten in the wiring.

Where a trigger is not legal from the state the member is actually in, the runner reports it rather than
performing it — so "you are already a member of this group" is refused by the shape of the graph rather than
by a check somebody has to remember to write.

## Viewing the diagrams

The pages embed [Mermaid](https://mermaid.js.org) in fenced code blocks, which **GitHub renders natively** —
open [account.md](account.md) in the GitHub UI and you see the diagram, not the source. That is the easiest
way to look at one.

Locally, most editors need an extension for Mermaid in a markdown preview (Visual Studio and VS Code both
show the fenced source by default). The app also carries a site-admin page at `/siteadmin/workflows` that
renders every registered machine, which is the option that needs nothing installed.

## Where the code lives

| | |
|---|---|
| Framework | `ODK.Core.Workflows` |
| Framework tests | `ODK.Core.Workflows.Tests` |
| Account machine | `ODK.Services/Members/Workflows/Account` |
| Chapter membership machine | `ODK.Services/Members/Workflows/ChapterMembership` |
| Chapter publication machine | `ODK.Services/Chapters/Workflows` |
| Wiring | `DependencyRegistrar.AddAccountWorkflows` and `AddChapterWorkflows` |
| Machine tests, and the page drift test | `ODK.Services.Tests/**/Workflows` |

A machine is a definition, a context, a context factory, a state resolver, its guards and its steps. Adding
another means adding those; the framework needs no changes, and the diagram and its drift test come for
free.

**A service holds its runner as `_<machine>Workflow`** — `_accountWorkflow`, `_chapterMembershipWorkflow`,
`_chapterPublicationWorkflow` — rather than after the `StateMachineRunner` type it is declared as. The type
already says what it is; the field name should say which flow the method below it is running. Extracting a
transition usually leaves an injected dependency behind with no callers, so check the constructor for one
the steps have taken over and remove it with its `using`.
