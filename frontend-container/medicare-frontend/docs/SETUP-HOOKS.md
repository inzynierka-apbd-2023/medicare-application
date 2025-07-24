# Cross-Platform Git Hooks Setup Guide

This guide helps set up Git hooks for the Medicare frontend application on **any operating system**.

## 🚀 Quick Setup (Any OS)

### 1. Install Dependencies
```bash
npm install
```

### 2. Setup Git Hooks
```bash
# Run the cross-platform setup script
./setup-hooks.sh
```

### 3. Test the Setup
```bash
# Navigate to repository root (important!)
cd ../../

# Test commit (this should trigger hooks)
echo "test" > test.txt
git add test.txt
git commit -m "feat(test): verify hooks are working"

# Clean up
git reset HEAD~1
rm test.txt
```

## 🖥️ Platform-Specific Notes

### Windows
- **Git Bash is required** - Install Git for Windows
- **PowerShell/CMD won't work** for commits - use Git Bash
- The setup script automatically configures Windows-specific settings

### macOS/Linux
- Works with default Terminal and bash
- No additional configuration needed

## 🔧 What the Setup Script Does

1. **Detects your operating system** automatically
2. **Configures Git settings:**
   - Sets hooks path to `.husky`
   - Prevents line ending conversion issues
   - Forces Unix-style line endings

3. **Prepares hook files:**
   - Converts to Unix line endings (if needed)
   - Makes hooks executable
   - Verifies bash shebangs

4. **Tests configuration:**
   - Validates commitlint setup
   - Ensures hooks are accessible

5. **Creates helper script** for easy committing

## 📋 How to Commit (After Setup)

### Option 1: Manual (from repository root)
```bash
cd ../..  # Go to repository root
git add .
git commit -m "feat(ui): add new component"
```

### Option 2: Use Helper Script
```bash
cd ../..  # Go to repository root
./commit-helper.sh "feat(ui): add new component"
```

## 🛡️ Security Features

The hooks will automatically:
- ✅ **Validate commit messages** (Conventional Commits format)
- ✅ **Run ESLint and Prettier** on staged files
- ✅ **Check for console.log statements**
- ✅ **Warn about TODO/FIXME comments**
- ✅ **Prevent commits to main branch**

## 🎯 Commit Message Format

```
type(scope): description

Examples:
feat(auth): add two-factor authentication
fix(appointments): resolve booking conflicts
docs(api): update authentication endpoints
refactor(dashboard): simplify patient data rendering
```

### Valid Types:
- `feat`: New features
- `fix`: Bug fixes
- `docs`: Documentation
- `style`: Code formatting
- `refactor`: Code refactoring
- `test`: Tests
- `chore`: Maintenance

### Valid Scopes:
- `auth`, `appointments`, `patients`, `doctors`
- `dashboard`, `profile`, `wallet`, `documents`
- `scheduler`, `api`, `ui`, `config`, `deps`

## 🔧 Troubleshooting

### "cannot spawn .husky/pre-commit"
**Solution:** Run the setup script again:
```bash
./setup-hooks.sh
```

### Hooks not running
**Solution:** Ensure you're committing from repository root:
```bash
cd ../..  # Must be in root directory
git commit -m "your message"
```

### Windows: "bash command not found"
**Solution:** Install Git for Windows and use Git Bash terminal

### Permission denied errors
**Solution:** The setup script should fix this, but manually:
```bash
chmod +x .husky/pre-commit .husky/commit-msg
```

## 🆘 Emergency Bypass

**Only use in emergencies** - bypasses all safety checks:
```bash
git commit -m "your message" --no-verify
```

## 📞 Getting Help

If hooks still don't work after running setup:

1. **Check your terminal:** Must use bash-compatible terminal
2. **Verify location:** Must commit from repository root
3. **Re-run setup:** `./setup-hooks.sh` should fix most issues
4. **Check Git version:** Ensure Git 2.0+ is installed

## 🔄 For New Team Members

1. Clone repository
2. Run `cd frontend-container/medicare-frontend && ./setup-hooks.sh`
3. Always commit from repository root
4. Use Git Bash on Windows

That's it! The hooks will work consistently across all platforms. 🎉
