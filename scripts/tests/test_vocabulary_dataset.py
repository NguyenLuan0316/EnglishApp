import copy
import json
from pathlib import Path
import re
import sys
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from build_vocabulary_dataset import OUTPUT, ROOT, TOPICS, bilingual_example, choose_sense, usable_senses, validate
from import_vocabulary_dataset import connection_settings


class VocabularyDatasetTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.entries = json.loads((OUTPUT / "words-3000.json").read_text(encoding="utf-8"))["entries"]

    def test_complete_package_has_3000_distinct_sourced_entries(self):
        validate(self.entries)
        self.assertTrue({"daily", "food", "travel", "work", "technology", "health", "education", "science", "society", "shopping"}.issubset({entry["topic"] for entry in self.entries}))

    def test_duplicate_headwords_are_rejected_even_with_case_changes(self):
        entries = copy.deepcopy(self.entries)
        entries[1]["word"] = entries[0]["word"].upper()
        with self.assertRaises(ValueError):
            validate(entries)

    def test_vietnamese_without_accents_is_not_discarded(self):
        senses = usable_senses({"word": "bee", "senses": [{"id": "test", "glosses": ["Ong."]}]})
        self.assertEqual("Ong.", senses[0]["clean_gloss"])

    def test_context_can_select_a_financial_sense_after_several_other_senses(self):
        senses = [{"clean_gloss": "Bờ sông."}] * 8 + [{"clean_gloss": "Nhà ngân hàng."}]
        self.assertEqual("Nhà ngân hàng.", choose_sense(senses, "finance")[0]["clean_gloss"])

    def test_ear_in_health_uses_body_sense(self):
        senses = [{"clean_gloss": "Bông lúa."}, {"clean_gloss": "Tai."}]
        self.assertEqual("Tai.", choose_sense(senses, "health")[0]["clean_gloss"])

    def test_examples_need_a_translation_and_ignore_attributed_quotations(self):
        sense = {"examples": [{"text": "a bank account"}, {"text": "a bank account", "translation": "tài khoản ngân hàng", "ref": "A modern book"}]}
        self.assertEqual(("", ""), bilingual_example(sense, "bank"))

    def test_every_topic_has_a_vietnamese_frontend_label(self):
        frontend = (ROOT / "frontend/js/domain/vocabulary.js").read_text(encoding="utf-8")
        labels = dict(re.findall(r"^\s+(\w+): '([^']+)'", frontend, re.MULTILINE))
        for topic, label in TOPICS.items():
            self.assertEqual(label, labels.get(topic), topic)

    def test_remote_import_and_mismatched_database_are_rejected(self):
        with patch.dict("os.environ", {"ConnectionStrings__DefaultConnection": "Host=remote.example;Database=wordwave_test;Username=test;Password=unused"}):
            with self.assertRaises(ValueError):
                connection_settings("wordwave_test")
        with patch.dict("os.environ", {"ConnectionStrings__DefaultConnection": "Host=127.0.0.1;Database=another_database;Username=test;Password=unused"}):
            with self.assertRaises(ValueError):
                connection_settings("wordwave_test")


if __name__ == "__main__":
    unittest.main()
