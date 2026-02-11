---
name: steps
description: Use this skill when implementing a number of steps from a steps markdown file.
---

- If steps are broken down into multiple phases, ask the user if they want you to work on all remaining phases, or just the next one.
- Begin implementing the steps one by one.
- After completing each step, mark its checkbox as done (`[x]`) in the steps file (if there is one).
- After implementing code changes, build the solution and run relevant tests. Fix any failures before moving on.
- Make commits at logical boundaries (e.g. after completing a step or a coherent group of related steps) so that a human reviewer can follow the progress in the git history. Don't batch all changes into a single commit at the end.
