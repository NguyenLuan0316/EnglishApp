"""Collect an additional 5,000 sourced words without modifying the original package.

Unknown CEFR is explicitly N/A. IPA fallback is exact-headword, single-reading
General American from ipa-dict; ambiguous readings require dictionary IPA.
"""
from __future__ import annotations

from collections import Counter, defaultdict
import csv
from datetime import datetime, timezone
import json
import re
from urllib.parse import quote

from build_vocabulary_dataset import (CACHE, OUTPUT, TOPICS, POS, LEVELS, KAIKKI,
    LANGMASTER, CEFR, download, check_robots, normalized, get_ipa, usable_senses,
    classify, choose_sense, bilingual_example, topic_references, sha256, validate)
from import_vocabulary_dataset import connection_settings

OCTANOVE = 'https://raw.githubusercontent.com/openlanguageprofiles/olp-en-cefrj/master/octanove-vocabulary-profile-c1c2-1.0.csv'
IPA = 'https://raw.githubusercontent.com/open-dict-data/ipa-dict/master/data/en_US.txt'
ALLOWED_LEVELS = {**LEVELS, 'C2': 5, 'N/A': 6}
SENSE_PREFERENCES = {
    'browser': ('technology', 'trình duyệt'),
    'feeling': ('emotions', 'cảm giác, cảm tưởng'),
    'rhino': ('animals', 'tê giác'),
    'jet': ('transport', 'phản lực'),
    'draft': ('communication', 'bản thảo'),
}
DEFECTIVE_HEADWORDS = {
    'grandpa', 'po', 'arse', 'cracking', 'examiner', 'aftermath', 'benjamin',
    'substrate', 'gym', 'condo', 'dungeon', 'infinity', 'intern', 'vendor',
    'functional', 'backwards', 'onwards', 'observed', 'capita',
}


def expansion_senses(entry):
    return [s for s in usable_senses(entry)
            if not re.search(r'\[\[|\]\]|[æɑəɪʊ]|Uây oác|Nhuấy', s['clean_gloss'])]


def build():
    import nltk
    from nltk.corpus import wordnet
    from wordfreq import zipf_frequency
    import psycopg

    if (OUTPUT / 'words-additional-5000.json').exists():
        raise ValueError('Expansion snapshot already exists. Validate it or explicitly archive it before collecting another batch; never overwrite imported provenance.')
    base_path = OUTPUT / 'words-3000.json'
    base = json.loads(base_path.read_text(encoding='utf8'))
    excluded = {normalized(e['word']) for e in base['entries']}
    # Read-only comparison with the confirmed local database, including any words
    # manually added since the first import. No credentials enter the manifest.
    with psycopg.connect(**connection_settings('wordwave_test')) as connection:
        excluded.update(normalized(row[0]) for row in connection.execute('SELECT word FROM vocabulary'))
    check_robots(KAIKKI, 'kaikki-robots.txt')
    check_robots(LANGMASTER, 'langmaster-robots.txt')
    paths = {
        'wiktionary': download(KAIKKI, 'viwiktionary-en.jsonl'),
        'cefrj': download(CEFR, 'cefrj.csv'),
        'langmaster': download(LANGMASTER, 'langmaster.html'),
        'octanove': download(OCTANOVE, 'octanove-c1c2.csv'),
        'ipaDict': download(IPA, 'ipa-en-us.txt'),
    }
    profiles = defaultdict(list)
    for name in ('cefrj', 'octanove'):
        for row in csv.DictReader(paths[name].open(encoding='utf-8-sig')):
            if row['CEFR'] in ALLOWED_LEVELS:
                row['_source'] = name
                for spelling in row['headword'].split('/'):
                    profiles[normalized(spelling)].append(row)
    ipa = {}
    for line in paths['ipaDict'].open(encoding='utf8'):
        if '\t' in line:
            word, reading = line.strip().split('\t', 1)
            if re.fullmatch(r'/[^/,]+/', reading) and len(reading) < 100:
                ipa[normalized(word)] = reading
    references = topic_references(paths['langmaster'].read_text(encoding='utf8'))
    nltk.data.path.insert(0, str(CACHE / 'nltk'))
    best = {}
    for line in paths['wiktionary'].open(encoding='utf8'):
        entry = json.loads(line)
        word, pos = normalized(entry.get('word', '')), entry.get('pos')
        if entry.get('lang_code') != 'en' or pos not in {'noun', 'verb', 'adj', 'adv'}:
            continue
        if word in excluded or word in DEFECTIVE_HEADWORDS or len(word) < 3 or not re.fullmatch(r'[a-z]+(?:[ -][a-z]+)*', word):
            continue
        # Exclude proper names even where the export uses a generic POS.
        if entry.get('word', '') != entry.get('word', '').lower():
            continue
        frequency = zipf_frequency(word, 'en')
        if frequency < 3.0:
            continue
        senses = expansion_senses(entry)
        phonetic = get_ipa(entry)
        own_ipa = bool(phonetic)
        phonetic = phonetic or ipa.get(word, '')
        if not senses or not phonetic:
            continue
        matching = [p for p in profiles[word] if POS.get(p['pos']) == pos]
        profile = min(matching, key=lambda p: ALLOWED_LEVELS[p['CEFR']]) if matching else {}
        context = {'CoreInventory 1': '', 'CoreInventory 2': '', **profile}
        topic, method = classify(word, pos, context, references, wordnet)
        sense, bonus = choose_sense(senses, topic, references.get(word, {}).get('gloss', ''), word)
        if word in SENSE_PREFERENCES:
            preferred_topic, text = SENSE_PREFERENCES[word]
            preferred = [s for s in senses if text in s['clean_gloss'].casefold()]
            if not preferred:
                continue
            topic, method, sense = preferred_topic, 'reviewed-source-sense', preferred[0]
        example, translation = bilingual_example(sense, word)
        record = {
            'word': word, 'phonetic': phonetic, 'meaning': sense['clean_gloss'],
            'example': example, 'exampleMeaning': translation,
            'level': profile.get('CEFR', 'N/A'), 'topic': topic,
            'partOfSpeech': pos, 'sourceSenseId': sense.get('id', ''), 'topicMethod': method,
            'sources': {
                'dictionary': 'https://vi.wiktionary.org/wiki/' + quote(entry['word'].replace(' ', '_')),
                'phonetic': 'wiktionary-entry' if own_ipa else IPA,
                'level': profile.get('_source', 'not-classified'),
                'topic': LANGMASTER if method == 'langmaster-reference' else
                    'https://wordnet.princeton.edu/' if method.startswith('wordnet') else
                    'cefrj-1.5' if method == 'cefrj-theme' else 'WordWave-topic-rules',
            },
        }
        if word in SENSE_PREFERENCES:
            record['editorialNote'] = 'Selected an existing source sense matching the learning topic; gloss not generated.'
        score = frequency + (0.2 if example else 0) + (0.15 if profile else 0) + bonus * 0.3
        if word not in best or score > best[word][0]:
            best[word] = (score, record)
    ranked = sorted(best.values(), key=lambda item: (-item[0], item[1]['word']))
    selected, seen = [], set()
    # Cover existing topics where candidates exist, then prioritize frequency.
    for topic in TOPICS:
        for _, entry in [x for x in ranked if x[1]['topic'] == topic][:25]:
            selected.append(entry)
            seen.add(entry['word'])
    counts = Counter(e['topic'] for e in selected)
    for _, entry in ranked:
        if len(selected) >= 5000:
            break
        if entry['word'] not in seen and counts[entry['topic']] < (700 if entry['topic'] in {'properties', 'daily'} else 450):
            selected.append(entry)
            seen.add(entry['word'])
            counts[entry['topic']] += 1
    for _, entry in ranked:
        if len(selected) >= 5000:
            break
        if entry['word'] not in seen:
            selected.append(entry)
            seen.add(entry['word'])
    validate(selected, 5000, ALLOWED_LEVELS)
    if seen & excluded:
        raise ValueError('Expansion overlaps existing vocabulary')
    selected.sort(key=lambda e: (list(TOPICS).index(e['topic']), ALLOWED_LEVELS[e['level']], e['word']))
    package = {**{k: v for k, v in base.items() if k != 'entries'}, 'entries': selected}
    package['attribution'] += ' Octanove Labs C1/C2; dohliam ipa-dict / lingz cmudict-ipa (MIT).'
    target = OUTPUT / 'words-additional-5000.json'
    target.write_text(json.dumps(package, ensure_ascii=False, indent=2) + '\n', encoding='utf8')
    report = {
        'generatedAt': datetime.now(timezone.utc).isoformat(), 'count': len(selected),
        'uniqueHeadwords': len(seen), 'excludedHeadwords': len(excluded),
        'baseDatasetSha256': sha256(base_path), 'datasetSha256': sha256(target),
        'eligibleHeadwords': len(best), 'topics': dict(Counter(e['topic'] for e in selected)),
        'levels': dict(Counter(e['level'] for e in selected)),
        'withBilingualUsageExamples': sum(bool(e['example']) for e in selected),
        'phoneticSources': dict(Counter(e['sources']['phonetic'] for e in selected)),
        'sourceUrls': {'wiktionary': KAIKKI, 'cefrj': CEFR, 'langmaster': LANGMASTER,
                       'octanove': OCTANOVE, 'ipaDict': IPA},
        'sources': {name: {'sha256': sha256(path), 'bytes': path.stat().st_size} for name, path in paths.items()},
        'qualityNote': 'Automated collection and topic classification; editorial review remains needed. N/A means no POS-matched reference CEFR, not an estimated level. IPA fallback uses one exact-headword reading only. Missing bilingual examples stay empty.',
    }
    (OUTPUT / 'manifest-additional-5000.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf8')
    for url, name in [
        ('https://raw.githubusercontent.com/open-dict-data/ipa-dict/master/LICENSE', 'ipa-dict-LICENSE.txt'),
        ('https://raw.githubusercontent.com/lingz/cmudict-ipa/master/LICENSE', 'cmudict-ipa-LICENSE.txt'),
    ]:
        (OUTPUT / 'licenses' / name).write_bytes(download(url, name).read_bytes())
    print(json.dumps(report, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    import argparse
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--validate-only', action='store_true')
    arguments = parser.parse_args()
    if arguments.validate_only:
        path = OUTPUT / 'words-additional-5000.json'
        entries = json.loads(path.read_text(encoding='utf8'))['entries']
        validate(entries, 5000, ALLOWED_LEVELS)
        manifest = json.loads((OUTPUT / 'manifest-additional-5000.json').read_text(encoding='utf8'))
        if sha256(path) != manifest['datasetSha256']:
            raise ValueError('Expansion differs from its manifest')
        print('Validated 5000 unique sourced expansion entries')
    else:
        build()
