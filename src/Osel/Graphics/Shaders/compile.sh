#!/bin/bash
# Compiles GLSL shaders to SPIR-V for the Osel engine.
# Requires glslc (from the Vulkan SDK) or glslangValidator.
#
# Usage: ./compile.sh

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
COMPILED_DIR="$SCRIPT_DIR/Compiled"

mkdir -p "$COMPILED_DIR"

COMPILER=""
if command -v glslc &>/dev/null; then
    COMPILER="glslc"
elif command -v glslangValidator &>/dev/null; then
    COMPILER="glslangValidator"
else
    echo "Error: Neither glslc nor glslangValidator found in PATH."
    echo "Install the Vulkan SDK to get these tools."
    exit 1
fi

echo "Using compiler: $COMPILER"

compile_shader() {
    local input="$1"
    local output="$2"

    if [ "$COMPILER" = "glslc" ]; then
        glslc "$input" -o "$output"
    else
        glslangValidator -V "$input" -o "$output"
    fi

    if [ $? -eq 0 ]; then
        echo "  OK: $(basename "$input") -> $(basename "$output")"
    else
        echo "  FAIL: $(basename "$input")"
        exit 1
    fi
}

echo "Compiling GLSL -> SPIR-V..."
compile_shader "$SCRIPT_DIR/sprite_batch.vert.glsl" "$COMPILED_DIR/sprite_batch.vert.spv"
compile_shader "$SCRIPT_DIR/sprite_batch.frag.glsl" "$COMPILED_DIR/sprite_batch.frag.spv"
echo "Done."
