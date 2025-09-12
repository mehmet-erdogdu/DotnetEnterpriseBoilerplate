#!/usr/bin/env node
/*
 Script to sort and format files alphabetically.
 
 For MessageService.*.json files:
 - Recursively sorts object keys alphabetically
 - Pretty-prints with 2-space indentation
 - Only rewrites files when content changes
 
 For GlobalUsings.cs files:
 - Sorts using statements alphabetically
 - Only rewrites files when content changes
 
 Usage:
   node scripts/sort-files.js               # process all MessageService.*.json files and GlobalUsings.cs files
   node scripts/sort-files.js <file1> ...   # process provided files
*/

const fs = require('fs');
const path = require('path');

const repoRoot = process.cwd();
const resourcesDir = path.join(
  repoRoot,
  'src',
  'BlogApp.Application',
  'Resources'
);

function sortObjectKeysDeep(value) {
  if (Array.isArray(value)) {
    return value.map(sortObjectKeysDeep);
  }
  if (value && typeof value === 'object') {
    const sorted = {};
    const keys = Object.keys(value).sort((a, b) => a.localeCompare(b));
    for (const key of keys) {
      sorted[key] = sortObjectKeysDeep(value[key]);
    }
    return sorted;
  }
  return value;
}

function formatJsonStable(obj) {
  return JSON.stringify(obj, null, 2) + '\n';
}

function readJsonFile(filePath) {
  const raw = fs.readFileSync(filePath, 'utf8');
  try {
    return JSON.parse(raw);
  } catch (err) {
    console.error(`Failed to parse JSON: ${filePath}`);
    throw err;
  }
}

function writeIfChanged(filePath, newContent) {
  const oldContent = fs.readFileSync(filePath, 'utf8');
  if (oldContent !== newContent) {
    fs.writeFileSync(filePath, newContent, 'utf8');
    return true;
  }
  return false;
}

function isMessageServiceFile(filePath) {
  const normalized = filePath.replace(/\\/g, '/');
  return /src\/BlogApp\.Application\/Resources\/MessageService\..*\.json$/.test(
    normalized
  );
}

function isGlobalUsingsFile(filePath) {
  const normalized = filePath.replace(/\\/g, '/');
  return normalized.endsWith('GlobalUsings.cs');
}

function getDefaultTargets() {
  if (!fs.existsSync(resourcesDir)) return [];
  const entries = fs.readdirSync(resourcesDir);
  return entries
    .filter((name) => /^MessageService\..*\.json$/.test(name))
    .map((name) => path.join(resourcesDir, name));
}

function processJsonFile(filePath) {
  const absolutePath = path.isAbsolute(filePath)
    ? filePath
    : path.join(repoRoot, filePath);

  if (!fs.existsSync(absolutePath)) {
    console.warn(`Skip (not found): ${filePath}`);
    return { file: filePath, changed: false, skipped: true };
  }

  if (!isMessageServiceFile(absolutePath)) {
    // Limit processing to matching files only
    return { file: filePath, changed: false, skipped: true };
  }

  const json = readJsonFile(absolutePath);
  const sorted = sortObjectKeysDeep(json);
  const formatted = formatJsonStable(sorted);
  const changed = writeIfChanged(absolutePath, formatted);
  return { file: filePath, changed, skipped: false };
}

function processGlobalUsingsFile(filePath) {
  const absolutePath = path.isAbsolute(filePath)
    ? filePath
    : path.join(repoRoot, filePath);

  if (!fs.existsSync(absolutePath)) {
    console.warn(`Skip (not found): ${filePath}`);
    return { file: filePath, changed: false, skipped: true };
  }

  if (!isGlobalUsingsFile(absolutePath)) {
    // Limit processing to matching files only
    return { file: filePath, changed: false, skipped: true };
  }

  // Read the file content
  const content = fs.readFileSync(absolutePath, 'utf8');
  
  // Split into lines, sort them, and join back
  const lines = content.split('\n');
  const sortedLines = lines.sort((a, b) => a.localeCompare(b));
  const sortedContent = sortedLines.join('\n');
  
  const changed = writeIfChanged(absolutePath, sortedContent);
  return { file: filePath, changed, skipped: false };
}

function processFile(filePath) {
  if (isMessageServiceFile(filePath)) {
    return processJsonFile(filePath);
  } else if (isGlobalUsingsFile(filePath)) {
    return processGlobalUsingsFile(filePath);
  } else {
    // Skip files that don't match our criteria
    return { file: filePath, changed: false, skipped: true };
  }
}

function main() {
  const args = process.argv.slice(2);
  let targets = args.length > 0 ? args : getDefaultTargets();

  // If no arguments provided, also look for GlobalUsings.cs files
  if (args.length === 0) {
    // Find all GlobalUsings.cs files in the project
    const globalUsingsFiles = findGlobalUsingsFiles();
    targets = targets.concat(globalUsingsFiles);
  }

  if (targets.length === 0) {
    // Nothing to do
    process.exit(0);
  }

  let changedCount = 0;
  for (const t of targets) {
    const result = processFile(t);
    if (!result.skipped && result.changed) changedCount += 1;
  }

  if (changedCount > 0) {
    console.log(`Formatted ${changedCount} file(s).`);
  }
}

function findGlobalUsingsFiles() {
  const globalUsings = [];
  
  // Helper function to find GlobalUsings.cs files in a directory
  function findInDirectory(dirPath) {
    if (fs.existsSync(dirPath)) {
      const projects = fs.readdirSync(dirPath);
      for (const project of projects) {
        const projectPath = path.join(dirPath, project);
        const stat = fs.statSync(projectPath);
        if (stat.isDirectory()) {
          const globalUsingsPath = path.join(projectPath, 'GlobalUsings.cs');
          if (fs.existsSync(globalUsingsPath)) {
            globalUsings.push(globalUsingsPath);
          }
        }
      }
    }
  }
  
  // Check src directory
  findInDirectory(path.join(repoRoot, 'src'));
  
  // Also check tests directory
  findInDirectory(path.join(repoRoot, 'tests'));
  
  return globalUsings;
}



try {
  main();
} catch (err) {
  console.error(err);
  process.exit(1);
}