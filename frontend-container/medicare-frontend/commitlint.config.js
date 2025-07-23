/** @type {import('@commitlint/types').UserConfig} */
export default {
  extends: ['@commitlint/config-conventional'],
  rules: {
    // Type validation - enforce specific types for Medicare application
    'type-enum': [
      2,
      'always',
      [
        'feat',      // New features
        'fix',       // Bug fixes
        'docs',      // Documentation changes
        'style',     // Code style changes (formatting, no logic changes)
        'refactor',  // Code refactoring
        'perf',      // Performance improvements
        'test',      // Adding or updating tests
        'chore',     // Maintenance tasks
        'ci',        // CI/CD pipeline changes
        'security',  // Security-related changes
        'revert'     // Reverting previous commits
      ]
    ],
    
    // Scope validation - specific to Medicare application domains
    'scope-enum': [
      2,
      'always',
      [
        'auth',        // Authentication/authorization
        'appointments', // Appointment management
        'patients',    // Patient management
        'doctors',     // Doctor features
        'dashboard',   // Dashboard components
        'profile',     // User profile features
        'wallet',      // Payment/subscription features
        'documents',   // Document management
        'scheduler',   // Calendar/scheduling
        'api',         // API integration
        'ui',          // UI components
        'config',      // Configuration changes
        'deps'         // Dependencies
      ]
    ],
    
    // Subject rules
    'subject-case': [2, 'always', 'lower-case'],
    'subject-empty': [2, 'never'],
    'subject-max-length': [2, 'always', 72],
    'subject-min-length': [2, 'always', 10],
    
    // Type rules
    'type-case': [2, 'always', 'lower-case'],
    'type-empty': [2, 'never'],
    
    // Header rules
    'header-max-length': [2, 'always', 100],
    
    // Body rules (optional but recommended for complex changes)
    'body-leading-blank': [1, 'always'],
    'body-max-line-length': [2, 'always', 100],
    
    // Footer rules
    'footer-leading-blank': [1, 'always']
  }
};
