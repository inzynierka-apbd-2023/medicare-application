#!/bin/bash
# Medicare Frontend - Cross-Platform Git Hooks Setup Script
# This script sets up git hooks for commit message validation on ANY OS

set -e  # Exit on any error

echo "🩺 Setting up Medicare Frontend git hooks for cross-platform compatibility..."

# Detect operating system
OS="$(uname -s)"
case "${OS}" in
    Linux*)     PLATFORM=Linux;;
    Darwin*)    PLATFORM=Mac;;
    CYGWIN*|MINGW*|MSYS*) PLATFORM=Windows;;
    *)          PLATFORM="UNKNOWN:${OS}"
esac

echo "🖥️  Detected platform: ${PLATFORM}"

# Ensure we're in the frontend directory
cd "$(dirname "$0")"
FRONTEND_DIR="$(pwd)"
echo "📂 Frontend directory: ${FRONTEND_DIR}"

# Navigate to repository root (two levels up)
REPO_ROOT="$(cd ../.. && pwd)"
echo "📁 Repository root: ${REPO_ROOT}"

# Ensure .husky directory exists
HUSKY_DIR="${REPO_ROOT}/.husky"
if [ ! -d "${HUSKY_DIR}" ]; then
    echo "❌ .husky directory not found at ${HUSKY_DIR}"
    echo "Please run this script from the correct location"
    exit 1
fi

echo "📋 Setting up Git configuration for hooks..."

# Configure Git to use bash and proper line endings
cd "${REPO_ROOT}"
git config core.hooksPath .husky
git config core.autocrlf false  # Prevent line ending issues
git config core.eol lf          # Force LF line endings

# For Windows: ensure Git uses bash for hooks
if [ "${PLATFORM}" = "Windows" ]; then
    echo "🪟 Configuring Windows-specific Git settings..."
    # Try to find Git Bash
    if command -v git >/dev/null 2>&1; then
        GIT_PATH="$(which git)"
        GIT_DIR="$(dirname "$(dirname "${GIT_PATH}")")"
        if [ -f "${GIT_DIR}/bin/bash.exe" ]; then
            echo "✅ Found Git Bash at: ${GIT_DIR}/bin/bash.exe"
        fi
    fi
fi

echo "🔧 Setting up hook files with proper permissions and line endings..."

# Function to setup a hook file
setup_hook() {
    local hook_name="$1"
    local hook_file="${HUSKY_DIR}/${hook_name}"
    
    if [ -f "${hook_file}" ]; then
        echo "🔄 Processing ${hook_name} hook..."
        
        # Ensure Unix line endings
        if command -v dos2unix >/dev/null 2>&1; then
            dos2unix "${hook_file}" 2>/dev/null || true
        else
            # Fallback: use sed to convert line endings
            sed -i 's/\r$//' "${hook_file}" 2>/dev/null || true
        fi
        
        # Make executable
        chmod +x "${hook_file}"
        
        # Verify it starts with proper shebang
        if ! head -1 "${hook_file}" | grep -q "^#!.*bash"; then
            echo "⚠️  Warning: ${hook_name} may not have proper bash shebang"
        else
            echo "✅ ${hook_name} hook setup complete"
        fi
    else
        echo "❌ ${hook_name} hook file not found"
        return 1
    fi
}

# Setup all hooks
setup_hook "pre-commit"
setup_hook "commit-msg"

echo "🧪 Testing Git hooks functionality..."

# Test if hooks are executable and accessible
cd "${REPO_ROOT}"
if [ -x "${HUSKY_DIR}/commit-msg" ] && [ -x "${HUSKY_DIR}/pre-commit" ]; then
    echo "✅ Hooks are executable"
else
    echo "❌ Hooks are not properly executable"
    exit 1
fi

# Test commitlint configuration (from frontend directory)
echo "🔍 Testing commitlint configuration..."
cd "${FRONTEND_DIR}"
if echo "feat(test): testing commit message validation" | npx commitlint; then
    echo "✅ Commitlint configuration is working correctly"
else
    echo "❌ Commitlint configuration has issues"
    exit 1
fi

# Create a helper script for developers
cd "${REPO_ROOT}"
cat > commit-helper.sh << 'EOF'
#!/bin/bash
# Helper script for committing with proper hooks
# Usage: ./commit-helper.sh "your commit message"

set -e

if [ $# -eq 0 ]; then
    echo "Usage: $0 \"commit message\""
    echo "Example: $0 \"feat(ui): add new button component\""
    exit 1
fi

# Ensure we're in the repository root
cd "$(git rev-parse --show-toplevel)"

# Make the commit
git commit -m "$1"
EOF

chmod +x commit-helper.sh

echo ""
echo "🎉 Cross-platform Git hooks setup complete!"
echo ""
echo "📋 What was configured:"
echo "   ✅ Git hooks path: .husky"
echo "   ✅ Line endings: LF (Unix style)"
echo "   ✅ Hook permissions: executable"
echo "   ✅ Cross-platform compatibility"
echo ""
echo "🚀 How to use on ANY machine:"
echo "   1. Clone the repository"
echo "   2. Run: cd frontend-container/medicare-frontend && ./setup-hooks.sh"
echo "   3. Commit from repository root: cd ../.. && git commit -m \"your message\""
echo "   4. Or use helper: ./commit-helper.sh \"your message\""
echo ""
echo "📖 Commit Convention:"
echo "   Format: type(scope): description"
echo "   Example: feat(auth): add two-factor authentication"
echo "   Scopes: ui, auth, appointments, patients, doctors, dashboard, etc."
echo ""
echo "� Platform: ${PLATFORM}"
echo "💡 Always use bash-compatible terminals for Git operations"
