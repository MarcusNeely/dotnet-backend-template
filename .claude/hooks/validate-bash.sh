#!/bin/bash
# Blocks destructive shell commands before Claude executes them.
# Exit 0 with a "deny" decision = blocked. Exit 0 with no output = allowed.

COMMAND=$(jq -r '.tool_input.command')

# Patterns that should never run
if echo "$COMMAND" | grep -qE 'rm -rf|git push --force|git push -f|git reset --hard|git clean -f'; then
  echo '{
    "hookSpecificOutput": {
      "hookEventName": "PreToolUse",
      "permissionDecision": "deny",
      "permissionDecisionReason": "Destructive command blocked by safety hook. Use a safer alternative or ask the user for explicit approval."
    }
  }'
  exit 0
fi

# Allow everything else
exit 0
