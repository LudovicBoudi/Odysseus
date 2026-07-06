#!/usr/bin/env bash
# Wrapper pour exécuter dotnet via la sandbox Flatpak GodotSharp.
# Usage: ./dev.sh build | test | restore | godot-check | godot-edit

set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"

CMD="${1:-help}"
case "$CMD" in
  restore)
    flatpak run --command=dotnet org.godotengine.GodotSharp restore "$ROOT/Odysseus.csproj"
    flatpak run --command=dotnet org.godotengine.GodotSharp restore "$ROOT/tests/Odysseus.Tests.csproj"
    ;;
  build|b)
    flatpak run --command=dotnet org.godotengine.GodotSharp build "$ROOT/Odysseus.csproj"
    ;;
  test|t)
    flatpak run --command=dotnet org.godotengine.GodotSharp test "$ROOT/tests/Odysseus.Tests.csproj"
    ;;
  godot-check|check|c)
    flatpak run org.godotengine.GodotSharp --editor --headless --quit-after 5 --path "$ROOT"
    ;;
  godot-edit|edit)
    flatpak run org.godotengine.GodotSharp --path "$ROOT"
    ;;
  all)
    "$0" build && "$0" test && "$0" godot-check
    ;;
  help|*)
    echo "Usage: $0 {restore|build|test|godot-check|godot-edit|all}"
    ;;
esac