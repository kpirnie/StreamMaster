/**
 * @type {import('semantic-release').GlobalConfig}
 */

module.exports = {
  branches: [
    {
      channel: "main",
      name: "main",
      prerelease: false
    },
    {
      name: "!main",
      prerelease: true
    }
  ],
  ci: false,
  debug: false,
  dryRun: false,
  plugins: [
    [
      "@semantic-release/commit-analyzer",
      {
        preset: "angular",
        releaseRules: [
          { type: "docs", scope: "README", release: "minor" },
          { type: "versionminor", release: "minor" },
          { type: "style", release: "patch" },
          { type: "minor", release: "minor" },
          { scope: "no-release", release: false },
          { type: "update", release: "patch" }
        ],
        parserOpts: {
          noteKeywords: ["BREAKING CHANGE", "BREAKING CHANGES"]
        }
      }
    ],
    "@semantic-release/release-notes-generator",
    {
      preset: "angular",
      presetConfig: {
        types: [
          { type: "feat", section: "Features" },
          { type: "fix", section: "Bug Fixes" },
          { type: "docs", section: "Documentation" },
          { type: "style", section: "Code Style" },
          { type: "refactor", section: "Refactoring" },
          { type: "perf", section: "Performance" },
          { type: "test", section: "Tests" },
          { type: "chore", section: "Maintenance" }
        ]
      }
    },
    [
      "@semantic-release/changelog",
      {
        changelogFile: "CHANGELOG.md"
      }
    ],
    [
      "@semantic-release/exec",
      {
        verifyConditionsCmd: ":",
        prepareCmd:
          "node src/updateAssemblyInfo.js ${nextRelease.version} ${nextRelease.gitHead} ${nextRelease.channel}"
      }
    ],
    [
      "@semantic-release/git",
      {
        assets: ["CHANGELOG.md", "src/StreamMaster.API/AssemblyInfo.cs"],
        message: "chore(release): ${nextRelease.version} [skip ci]\n\n${nextRelease.notes}"
      }
    ],
    [
      "@semantic-release/github",
      {
        assets: ["CHANGELOG.md"]
      }
    ]
  ]
};
