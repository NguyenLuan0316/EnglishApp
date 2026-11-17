# Vocabulary dataset and local import

The checked-in package at `frontend/data/vocabulary/words-3000.json` contains 3,000 unique English headwords with Vietnamese glosses, phonetics, a POS-matched CEFR-J level and one topic per headword. It has 32 topic keys, preserves the ten existing topic keys, and adds Vietnamese labels for the additional groups in `frontend/js/domain/vocabulary.js`.

The initial local import on 16 November 2026 inserted 3,000 rows into `wordwave_test` at `127.0.0.1:55432`. The table was empty before import. A second preview detected all 3,000 words and proposed zero additions. No existing vocabulary, learner accounts, attempts or progress were overwritten.

## Data and attribution

### Additional 5,000 words (16 November 2026)

`words-additional-5000.json` and `manifest-additional-5000.json` preserve a second, disjoint batch. The local import added exactly 5,000 rows, bringing `wordwave_test` at `127.0.0.1:55432` from 3,000 to **8,000 distinct headwords across the same 32 topics**. All 3,000 original rows were compared with the pre-import backup and remain unchanged. Repeating the import preview proposes zero inserts. The original dataset and its hash are unchanged.

The expansion reuses the identified dictionary/CEFR-J/Langmaster source snapshots from the cache and downloads two additional sources: Octanove Labs' C1/C2 Vocabulary Profile v1.0 (CC BY-SA 4.0), and the General American portion of dohliam's ipa-dict (MIT, based on lingz's cmudict-ipa). Their URLs and SHA-256 hashes are recorded in the expansion manifest; MIT notices are included in `licenses/`. Only an exact-headword, single-reading IPA fallback is accepted. IPA remains source-derived, not synthesized. This fallback is not POS-specific, so pronunciation review may still be needed for heteronyms.

Every new entry has a dictionary-derived Vietnamese gloss and phonetic transcription. 1,354 have paired source examples; 3,646 have empty example fields. Levels: A1 142, A2 268, B1 621, B2 647, C1 210, C2 62, and **3,050 unclassified**. `N/A` is rendered as “Chưa phân loại” and can be filtered; it does not imply a CEFR estimate. Grammar remains A1–C1. Selection uses frequency >= 3.0, topic coverage and caps on broad groups; it does not claim equal topic counts or complete manual editorial review. Targeted checks excluded corrupt source glosses and selected existing source senses for browser, feeling, rhino, jet and draft.

```powershell
python scripts/build_vocabulary_expansion.py --validate-only
python scripts/import_vocabulary_dataset.py --expected-database wordwave_test --package frontend/data/vocabulary/words-additional-5000.json --manifest frontend/data/vocabulary/manifest-additional-5000.json
# Add --apply to commit to the confirmed local database.
```

The collector reads existing local headwords before selection and refuses to overwrite an existing expansion snapshot. It is intended to collect a new batch once, with cached raw sources retained for audit. Archive and name any future batch explicitly instead of replacing provenance for imported words. Backups and ID-level reports remain under `%LOCALAPPDATA%\WordWave\vocabulary-backups`; the successful expansion report is `import-20261116T155855026429Z.json`.

- **Vietnamese Wiktionary via Kaikki / Wiktextract:** Vietnamese glosses, IPA, bilingual usage examples and sense identifiers. The source JSONL is 114,882,953 bytes. Attribution and share-alike information are included in the public package and `sources.htm`. Literary quotations with attribution fields are excluded from usage examples.
- **CEFR-J Wordlist v1.5:** headwords, part of speech, A1–B2 labels and theme metadata. Compiled by Yukio Tono, Tono Laboratory, Tokyo University of Foreign Studies; retrieved from the Open Language Profiles mirror on 16 November 2026. Its terms permit research/commercial use with proper citation. The levels are reference vocabulary levels, not learner assessments.
- **Langmaster:** the supplied article is fetched once and used for factual word/topic membership and to help distinguish dictionary senses. Article prose, translations and IPA from its tables are not copied into the published package. The imported glosses and IPA come from the open dictionary source.
- **Princeton WordNet 3.0:** semantic grouping and hypernym hints, under its redistribution license. The original license notice is included under `licenses/`.
- **wordfreq 3.1.1:** prioritizes common words. Frequency tables are not republished. Its Apache license and complete project/data acknowledgments are included under `licenses/`, including the freely available SUBTLEX data by Marc Brysbaert and colleagues.

Source links, download hashes, topic/level counts, example counts and a package hash are in `manifest.json`. Each entry retains `partOfSpeech`, `sourceSenseId`, `sources` and `topicMethod`. The combined published dataset is CC BY-SA 4.0 with the component acknowledgments retained; this does not change the application's code license. The frontend links to the public Vietnamese attribution page at `/data/vocabulary/sources.htm`.

## Quality and limits

All 3,000 entries have a Vietnamese meaning, phonetic transcription, source sense ID, valid level and topic. They are unique after Unicode/case/whitespace normalization. The selected CEFR-J distribution is A1: 722; A2: 718; B1: 932; B2: 628.

1,528 entries have a source-provided English usage example or phrase paired with Vietnamese. The other 1,472 entries deliberately have empty example fields. Examples and missing translations are never generated to meet a completeness quota. Some examples are collocations rather than complete sentences.

This is an automatically collected, normalized and classified dataset with targeted editorial checks, not a fully reviewed dictionary. Source dictionaries contain old spelling, legacy senses and occasional extraction defects. Topic selection can be broad, and ambiguous terms may still need review in the learning context. Checks included common homonyms such as ear/bank/diet/class/violin/groom/yard, retaining the original source sense identifiers. A malformed merged tail in the adjective “kind” was trimmed and recorded in `editorialNote`.

IPA is used as supplied by the dictionary. A missing IPA may be shared only from a dictionary entry with the same headword and POS when exactly one transcription is available; known heteronyms are excluded from that fallback and the source method is marked in the JSON. No pronunciation is synthesized.

## Reproduce

Use a Python virtual environment and install:

```powershell
python -m pip install -r scripts/requirements-vocabulary.txt
python scripts/build_vocabulary_dataset.py
python scripts/build_vocabulary_dataset.py --validate-only
python -m unittest discover -s scripts/tests -v
```

The builder uses HTTPS allowlisted sources, checks Langmaster/Kaikki robots policies, uses timeouts and a download size limit, and writes completed downloads atomically. Raw source files and the WordNet download stay under `%LOCALAPPDATA%\WordWave\vocabulary-cache`, outside Git. Existing cache files are reused to reproduce the checked-in snapshot; source hashes identify the actual inputs. To intentionally refresh sources, archive the corresponding cache files first and rebuild, then review the package diff and manifest before importing.

The builder prefers topic coverage and common vocabulary. It does not force equal topic sizes by inserting obscure words or inventing classifications. Single headwords have one selected POS/sense, so the list is not a complete dictionary of all senses of each word, nor a claim to be an official “Oxford 3000” list.

## Import safely

```powershell
python scripts/import_vocabulary_dataset.py --expected-database wordwave_test
python scripts/import_vocabulary_dataset.py --apply --expected-database wordwave_test
```

The default is read-only preview. Applying requires an explicit expected database name. The connection comes from `ConnectionStrings__DefaultConnection` or the existing API `appsettings.json`; credentials are never printed. Remote targets and mismatched database names are rejected.

The importer validates the package and its manifest hash, preserves existing normalized headwords, serializes concurrent imports and commits the inserts as one transaction. It writes a pre-import vocabulary snapshot and an ID-level import report under `%LOCALAPPDATA%\WordWave\vocabulary-backups`. Failures roll back the database writes. It does not modify the schema, re-seed on startup, create users or alter learning attempts.

The current database model has no provenance columns. Provenance is therefore preserved in the checked-in package/manifest, with actual inserted IDs in the machine-local import report. This keeps the existing API and model compatible.

## Verification

Eight Python tests cover full-package validity, normalized duplicate rejection, short unaccented Vietnamese glosses, homonym sense selection, paired examples/quotation filtering, frontend topic labels and local-only import targeting.

The API test suite additionally loads the exact package into a disposable local PostgreSQL database and checks the existing paged vocabulary endpoint, all 32 topics, combined level/topic/search filters, random practice words and the final page. It leaves the real `wordwave_test` accounts and learning records untouched. Set `WORDWAVE_REPO_ROOT` to the repository path when running the .NET tests from an external artifacts directory; normal in-repository output paths work without it.

No production deployment/import was performed. Use Ctrl+F5 on `/vocab` to refresh the new topic labels in an already-open browser tab.
