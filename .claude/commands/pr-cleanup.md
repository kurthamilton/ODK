---
description: Post-PR cleanup — fetch, switch to master, pull, delete merged local branches
allowed-tools: Bash(git status:*), Bash(git fetch:*), Bash(git branch:*), Bash(git switch:*), Bash(git pull:*), Bash(git log:*)
---

Run the post-PR-merge local cleanup routine. This is an explicit request for git actions, so the
"don't perform git actions unless asked" rule in CLAUDE.md does not apply here.

## Steps

1. **Check the working tree is clean** — `git status --porcelain --untracked-files=no`.
   If there are uncommitted or staged changes to tracked files, **stop immediately** and report
   them. Do not stash, commit, or discard anything. The user decides what to do with in-flight work.

   Untracked files are deliberately excluded: they survive a branch switch untouched, and `.claude/`
   is permanently untracked here, so including them would block every run.

2. **Fetch and prune** — `git fetch --prune`.
   Pruning is what marks a local branch's upstream as `[gone]` once GitHub deletes the head branch
   on merge. Everything downstream depends on this running first.

3. **Switch to master and fast-forward** — `git switch master` then `git pull --ff-only`.
   `--ff-only` deliberately: if master can't fast-forward, something unexpected has happened
   (local commits on master, a force-push upstream). Stop and report rather than merging.

4. **Find dead branches** — `git branch -vv`, and take the branches whose upstream is marked
   `[gone]`. Those are the ones whose remote branch GitHub deleted when the PR merged.

   Never delete `master`. Ignore branches with **no** upstream at all — those were never pushed,
   so they may be genuine unfinished local work; list them separately as "not touched" instead.

5. **Delete them** — for each `[gone]` branch, `git branch -d <name>`.
   PRs here are squash-merged, so the local commits have different SHAs to what landed on master
   and `-d` will usually refuse with "not fully merged". For a `[gone]` branch that is expected —
   retry with `git branch -D <name>`. A deleted upstream is GitHub confirming the PR closed, and
   the SHA stays recoverable via `git reflog` / `git branch <name> <sha>`.

   Do **not** use `-D` on a branch that is not `[gone]`.

## Report

Print a short summary:

- master: the SHA it moved from → to, or "already up to date"
- deleted: each branch as `name (sha)` — include the SHA so it can be restored
- skipped: any local-only branches left alone, and why

If nothing needed doing, say so in one line. Don't narrate the individual git commands.
