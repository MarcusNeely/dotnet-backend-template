#!/bin/bash
# Blocks direct edits to sensitive files (package.json, lock files, .env).
# Claude should use npm commands for dependency changes instead.

FILE_PATH=$(jq -r '.tool_input.file_path')

if echo "$FILE_PATH" | grep -qE '(package\.json|package-lock\.json|yarn\.lock|\.env)$'; then
  echo '{
    "hookSpecificOutput": {
      "hookEventName": "PreToolUse",
      "permissionDecision": "deny",
      "permissionDecisionReason": "Protected file. Use npm/yarn commands for dependency changes. Never edit .env files directly."
    }
  }'
  exit 0
fi

exit 0
