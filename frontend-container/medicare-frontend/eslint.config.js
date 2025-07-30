import js from "@eslint/js";
import globals from "globals";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import boundaries from "eslint-plugin-boundaries";
import tseslint from "@typescript-eslint/eslint-plugin";
import tsparser from "@typescript-eslint/parser";
import importPlugin from "eslint-plugin-import";
import simpleImportSort from "eslint-plugin-simple-import-sort";

export default [
  { ignores: ["dist", "**/*.d.ts"] },

  // Configuration files
  {
    files: ["*.config.{js,ts}", "*.config.*.{js,ts}"],
    languageOptions: {
      ecmaVersion: 2020,
      globals: {
        ...globals.browser,
        ...globals.node,
      },
    },
  },

  // JavaScript files
  {
    files: ["**/*.{js,jsx}"],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
      parserOptions: {
        ecmaVersion: "latest",
        ecmaFeatures: { jsx: true },
        sourceType: "module",
      },
    },
    plugins: {
      "react-hooks": reactHooks,
      "react-refresh": reactRefresh,
      boundaries: boundaries,
    },
    rules: {
      ...js.configs.recommended.rules,
      ...reactHooks.configs.recommended.rules,
      "no-unused-vars": ["error", { varsIgnorePattern: "^[A-Z_]" }],
      "react-refresh/only-export-components": [
        "warn",
        { allowConstantExport: true },
      ],
    },
  },

  // TypeScript files
  {
    files: ["**/*.{ts,tsx}"],
    languageOptions: {
      parser: tsparser,
      ecmaVersion: 2020,
      globals: globals.browser,
      parserOptions: {
        ecmaVersion: "latest",
        ecmaFeatures: { jsx: true },
        sourceType: "module",
      },
    },
    plugins: {
      "@typescript-eslint": tseslint,
      "react-hooks": reactHooks,
      "react-refresh": reactRefresh,
      boundaries: boundaries,
      import: importPlugin,
      "simple-import-sort": simpleImportSort,
    },
    settings: {
      "boundaries/elements": [
        {
          type: "shared",
          pattern: "src/shared/**",
        },
        {
          type: "feature",
          pattern: "src/features/**",
        },
        {
          type: "layout",
          pattern: "src/layout/**",
        },
        {
          type: "app",
          pattern: "src/{App,main}.{js,jsx,ts,tsx}",
        },
      ],
    },
    rules: {
      ...js.configs.recommended.rules,
      ...tseslint.configs.recommended.rules,
      ...reactHooks.configs.recommended.rules,
      "@typescript-eslint/no-unused-vars": [
        "error",
        {
          varsIgnorePattern: "^_",
          argsIgnorePattern: "^_",
          caughtErrorsIgnorePattern: "^_",
          destructuredArrayIgnorePattern: "^_",
        },
      ],
      "no-unused-vars": "off", // Turn off base rule as it can report incorrect errors
      "react-refresh/only-export-components": [
        "warn",
        { allowConstantExport: true },
      ],

      // Architectural boundaries enforcement
      "boundaries/element-types": [
        "error",
        {
          default: "disallow",
          rules: [
            {
              from: "feature",
              allow: ["shared", "feature"],
            },
            {
              from: "shared",
              allow: ["shared"],
            },
            {
              from: "layout",
              allow: ["shared", "feature"],
            },
            {
              from: "app",
              allow: ["shared", "feature", "layout"],
            },
          ],
        },
      ],

      // Custom import architecture rules
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            {
              group: ["../../../*", "../../../../*", "../../../../../*"],
              message:
                "Avoid deep relative imports (more than 2 levels). Use absolute imports with @ aliases instead.",
            },
            {
              group: [
                "*/features/*/components/*",
                "*/features/*/*/components/*",
              ],
              message:
                'Import feature components through the feature index: import { Component } from "@features/featureName"',
            },
            {
              group: ["./components/*", "../components/*"],
              message:
                'Import components through the feature index: import { Component } from "./index"',
            },
            {
              group: ["src/*"],
              message:
                'Use absolute imports with @ aliases: import { Component } from "@/shared/components"',
            },
          ],
        },
      ],

      // Enforce consistent React imports
      "react/jsx-uses-react": "off",
      "react/react-in-jsx-scope": "off",

      // Enforce proper naming conventions
      "@typescript-eslint/naming-convention": [
        "error",
        {
          selector: "variable",
          filter: "^_",
          format: null,
        },
        {
          selector: "interface",
          format: ["PascalCase"],
          custom: {
            regex: "^I[A-Z]",
            match: false,
          },
        },
        {
          selector: "typeAlias",
          format: ["PascalCase"],
        },
        {
          selector: "enum",
          format: ["PascalCase"],
        },
        {
          selector: "variable",
          format: ["camelCase", "UPPER_CASE", "PascalCase"],
        },
      ],

      // Import sorting and organization
      "simple-import-sort/imports": [
        "error",
        {
          groups: [
            // React and external libraries
            ["^react", "^@?\\w"],
            // Internal absolute imports (with @)
            ["^@/"],
            // Parent imports
            ["^\\.\\.(?!/?$)", "^\\.\\./?$"],
            // Other relative imports
            ["^\\./(?=.*/)(?!/?$)", "^\\.(?!/?$)", "^\\./?$"],
            // Style imports
            ["^.+\\.s?css$"],
          ],
        },
      ],
      "simple-import-sort/exports": "error",
      "import/no-duplicates": "error",
      "import/newline-after-import": "error",
    },
  },

  // Specific rules for feature modules
  {
    files: ["src/features/**/*.{js,jsx,ts,tsx}"],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            {
              group: ["../../features/**", "../../../features/**"],
              message:
                "Cross-feature imports should go through feature index files or use shared modules",
            },
          ],
        },
      ],
    },
  },
];
