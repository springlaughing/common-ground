// Generates the German localization seed compiled into the EF migration
// `SeedLocalizationTranslations`. Emits a C# source file with the German content as
// string literals (the same approach as the English SeedDataHelper.cs — literals are
// reliably compiled into the assembly, unlike embedded resources on this dev setup).
//
// Source of truth (human-authored, repo root):
//   - questionary_german.md            → German question + answer-option text
//   - reflection_groups_german.json    → German group titles, insight texts, dimension titles
//   - reflection-groups.json           → English dimension titles (paired for the locale-first table)
//
// Re-run after any content edit (T045):  node scripts/generate-localization-seed.mjs
//
// The German question/option structure MUST mirror the English seed in
// SeedDataHelper.cs so translation rows attach to the same (section, q, letter) IDs.

import { readFileSync, writeFileSync, mkdirSync } from 'node:fs'
import { dirname, join } from 'node:path'
import { fileURLToPath } from 'node:url'

const root = join(dirname(fileURLToPath(import.meta.url)), '..')
const OUT = join(root, 'backend/src/CommonGround.Api/Persistence/Migrations/LocalizationSeedData.g.cs')

// English seed shape (SeedDataHelper.BuildQuestions): questions per section, all 4 options.
const EXPECTED_QUESTIONS_PER_SECTION = { 1: 4, 2: 3, 3: 7, 4: 5, 5: 4, 6: 4, 7: 4, 8: 4, 9: 6, 10: 5 }
const TOTAL_QUESTIONS = Object.values(EXPECTED_QUESTIONS_PER_SECTION).reduce((a, b) => a + b, 0)

function fail(msg) {
  console.error(`✗ ${msg}`)
  process.exit(1)
}

// A C# string literal. JSON.stringify escapes ", \, and control chars exactly the way
// C# accepts (\", \\, \n, \uXXXX); non-ASCII (ä, —, „) stays literal in the UTF-8 source.
const cs = (s) => JSON.stringify(s)

// ── Parse questionary_german.md ────────────────────────────────────────────
function parseQuestionary(md) {
  const questions = []
  let section = null
  let current = null

  for (const raw of md.split(/\r?\n/)) {
    const line = raw.trim()

    const sectionMatch = line.match(/^##\s*Abschnitt\s+(\d+)/)
    if (sectionMatch) {
      section = Number(sectionMatch[1])
      continue
    }

    const questionMatch = line.match(/^\*\*Q(\d+)\s+(.*?)\*\*$/)
    if (questionMatch) {
      if (section === null) fail('Question found before any "## Abschnitt" header')
      current = { section, q: Number(questionMatch[1]), text: questionMatch[2].trim(), options: [] }
      questions.push(current)
      continue
    }

    const optionMatch = line.match(/^([A-D])\.\s+(.*\S)$/)
    if (optionMatch) {
      if (!current) fail(`Option "${optionMatch[1]}" found before any question`)
      current.options.push({ letter: optionMatch[1], text: optionMatch[2].trim() })
    }
  }
  return questions
}

// ── Validate the German structure matches the English seed shape ────────────
function validateQuestions(questions) {
  if (questions.length !== TOTAL_QUESTIONS)
    fail(`Expected ${TOTAL_QUESTIONS} questions, parsed ${questions.length}`)

  const bySection = {}
  for (const q of questions) (bySection[q.section] ??= []).push(q)

  for (const [section, expected] of Object.entries(EXPECTED_QUESTIONS_PER_SECTION)) {
    const got = bySection[section]?.length ?? 0
    if (got !== expected)
      fail(`Section ${section}: expected ${expected} questions, parsed ${got}`)

    const localQs = bySection[section].map((q) => q.q)
    const wanted = Array.from({ length: expected }, (_, i) => i + 1)
    if (JSON.stringify(localQs) !== JSON.stringify(wanted))
      fail(`Section ${section}: question numbers ${JSON.stringify(localQs)} != ${JSON.stringify(wanted)}`)
  }

  for (const q of questions) {
    const letters = q.options.map((o) => o.letter)
    if (JSON.stringify(letters) !== JSON.stringify(['A', 'B', 'C', 'D']))
      fail(`S${q.section}Q${q.q}: options ${JSON.stringify(letters)} != [A,B,C,D]`)
    for (const o of q.options) if (!o.text) fail(`S${q.section}Q${q.q} option ${o.letter}: empty text`)
  }
}

// ── Build the model ──────────────────────────────────────────────────────────
const questions = parseQuestionary(readFileSync(join(root, 'questionary_german.md'), 'utf8'))
validateQuestions(questions)

const deGroups = JSON.parse(readFileSync(join(root, 'reflection_groups_german.json'), 'utf8'))
const enGroups = JSON.parse(readFileSync(join(root, 'reflection-groups.json'), 'utf8'))

const groupTitles = deGroups.groups.map((g) => {
  if (!g.title_de) fail(`Group ${g.id}: missing title_de`)
  return { id: g.id, title: g.title_de }
})

const memberDims = enGroups.groups.flatMap((g) => g.dimensions)
const insights = memberDims.map((dim) => {
  if (!deGroups.insights[dim]) fail(`Dimension ${dim}: missing German insight`)
  return { dim, text: deGroups.insights[dim] }
})

// Locale-first dimension titles: pair EN + DE per dimension (consumed by T035).
const dimensionTitles = memberDims.map((dim) => {
  if (!enGroups.dimensionTitles[dim]) fail(`Dimension ${dim}: missing English title`)
  if (!deGroups.dimensionTitles[dim]) fail(`Dimension ${dim}: missing German title`)
  return { dim, en: enGroups.dimensionTitles[dim], de: deGroups.dimensionTitles[dim] }
})

if (groupTitles.length !== enGroups.groups.length)
  fail(`Group titles: ${groupTitles.length} != ${enGroups.groups.length}`)

// ── Emit C# ───────────────────────────────────────────────────────────────────
const lines = []
lines.push('// <auto-generated />')
lines.push('// Generated by scripts/generate-localization-seed.mjs — do not edit by hand.')
lines.push('// Source: questionary_german.md, reflection_groups_german.json, reflection-groups.json')
lines.push('')
lines.push('namespace CommonGround.Api.Persistence.Migrations;')
lines.push('')
lines.push('/// <summary>German localization content compiled into the assembly for the seed migration.</summary>')
lines.push('internal static class LocalizationSeedData')
lines.push('{')
lines.push('    internal readonly record struct OptionSeed(string Letter, string Text);')
lines.push('    internal readonly record struct QuestionSeed(int Section, int Q, string Text, OptionSeed[] Options);')
lines.push('    internal readonly record struct GroupTitleSeed(string GroupId, string Title);')
lines.push('    internal readonly record struct InsightSeed(string Dim, string Text);')
lines.push('    internal readonly record struct DimensionTitleSeed(string Dim, string En, string De);')
lines.push('')

lines.push('    internal static readonly QuestionSeed[] Questions =')
lines.push('    {')
for (const q of questions) {
  const opts = q.options.map((o) => `new(${cs(o.letter)}, ${cs(o.text)})`).join(', ')
  lines.push(`        new(${q.section}, ${q.q}, ${cs(q.text)}, new OptionSeed[] { ${opts} }),`)
}
lines.push('    };')
lines.push('')

lines.push('    internal static readonly GroupTitleSeed[] GroupTitles =')
lines.push('    {')
for (const g of groupTitles) lines.push(`        new(${cs(g.id)}, ${cs(g.title)}),`)
lines.push('    };')
lines.push('')

lines.push('    internal static readonly InsightSeed[] Insights =')
lines.push('    {')
for (const i of insights) lines.push(`        new(${cs(i.dim)}, ${cs(i.text)}),`)
lines.push('    };')
lines.push('')

lines.push('    internal static readonly DimensionTitleSeed[] DimensionTitles =')
lines.push('    {')
for (const d of dimensionTitles) lines.push(`        new(${cs(d.dim)}, ${cs(d.en)}, ${cs(d.de)}),`)
lines.push('    };')
lines.push('}')
lines.push('')

mkdirSync(dirname(OUT), { recursive: true })
writeFileSync(OUT, lines.join('\n'), 'utf8')

console.log('✓ LocalizationSeedData.g.cs generated')
console.log(`  questions:        ${questions.length} (${questions.reduce((n, q) => n + q.options.length, 0)} options)`)
console.log(`  groupTitles:      ${groupTitles.length}`)
console.log(`  insights:         ${insights.length}`)
console.log(`  dimensionTitles:  ${dimensionTitles.length} (en+de pairs)`)
