import json
from pathlib import Path
import sys
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from build_vocabulary_dataset import OUTPUT, normalized, sha256, validate
from build_vocabulary_expansion import ALLOWED_LEVELS, expansion_senses


class VocabularyExpansionTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.entries = json.loads((OUTPUT / 'words-additional-5000.json').read_text(encoding='utf8'))['entries']
        cls.base = json.loads((OUTPUT / 'words-3000.json').read_text(encoding='utf8'))['entries']
        cls.manifest = json.loads((OUTPUT / 'manifest-additional-5000.json').read_text(encoding='utf8'))

    def test_additional_5000_are_complete_disjoint_and_in_existing_topics(self):
        validate(self.entries, 5000, ALLOWED_LEVELS)
        self.assertFalse({normalized(e['word']) for e in self.base} & {normalized(e['word']) for e in self.entries})
        self.assertEqual({e['topic'] for e in self.base}, {e['topic'] for e in self.entries})

    def test_snapshot_hashes_and_counts_match(self):
        self.assertEqual(sha256(OUTPUT / 'words-additional-5000.json'), self.manifest['datasetSha256'])
        self.assertEqual(sha256(OUTPUT / 'words-3000.json'), self.manifest['baseDatasetSha256'])
        self.assertEqual(5000, sum(self.manifest['topics'].values()))
        self.assertEqual(5000, sum(self.manifest['levels'].values()))

    def test_unknown_levels_and_ipa_fallback_are_explicit(self):
        for entry in self.entries:
            self.assertEqual(entry['level'] == 'N/A', entry['sources']['level'] == 'not-classified')
            if entry['sources']['phonetic'] != 'wiktionary-entry':
                self.assertNotIn(',', entry['phonetic'])
                self.assertTrue(entry['phonetic'].startswith('/'))
                self.assertTrue(entry['phonetic'].endswith('/'))

    def test_corrupt_gloss_is_filtered_and_reviewed_senses_are_used(self):
        self.assertEqual([], expansion_senses({'word': 'grandma', 'senses': [{'glosses': ["Uây oác['grænmə, mɑ:]."]}]}))
        by_word = {e['word']: e for e in self.entries}
        self.assertEqual('Bà.', by_word['grandma']['meaning'])
        self.assertIn('trình duyệt', by_word['browser']['meaning'])
        self.assertEqual('technology', by_word['browser']['topic'])
        self.assertNotIn('grandpa', by_word)


if __name__ == '__main__':
    unittest.main()
