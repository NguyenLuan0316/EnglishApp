"""Build an attributed EN/VI vocabulary package from public learning-data sources.

Raw downloads stay outside the repository. The published JSON keeps sense/POS,
source URLs and licensing metadata; no translation or CEFR level is invented.
"""
from __future__ import annotations

import argparse
from collections import Counter, defaultdict
import csv
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import unicodedata
from urllib.parse import quote, urlparse
from urllib.request import HTTPRedirectHandler, Request, build_opener
from urllib.robotparser import RobotFileParser

ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "frontend/data/vocabulary"
CACHE = Path(os.environ.get("LOCALAPPDATA", Path.home() / ".cache")) / "WordWave/vocabulary-cache"
LANGMASTER = "https://langmaster.edu.vn/tong-hop-3000-tu-vung-tieng-anh-thong-dung-theo-chu-de"
KAIKKI = "https://kaikki.org/viwiktionary/Ti%E1%BA%BFng%20Anh/kaikki.org-dictionary-Ti%E1%BA%BFngAnh.jsonl"
CEFR = "https://raw.githubusercontent.com/openlanguageprofiles/olp-en-cefrj/master/cefrj-vocabulary-profile-1.5.csv"
HEADERS = {"User-Agent": "WordWaveVocabularyResearch/1.0 (educational vocabulary collection)"}
ALLOWED_HOSTS = {"langmaster.edu.vn", "kaikki.org", "raw.githubusercontent.com"}
MAX_BYTES = 350 * 1024 * 1024
POS = {"noun": "noun", "verb": "verb", "modal verb": "verb", "adjective": "adj", "adverb": "adv", "preposition": "prep", "conjunction": "conj", "determiner": "det", "pronoun": "pron", "interjection": "intj", "number": "num"}
LEVELS = {"A1": 0, "A2": 1, "B1": 2, "B2": 3, "C1": 4}
TOPICS = {
    "daily": "Hàng ngày", "food": "Ẩm thực", "travel": "Du lịch", "work": "Công việc",
    "technology": "Công nghệ", "health": "Sức khỏe", "education": "Giáo dục", "science": "Khoa học",
    "society": "Xã hội", "shopping": "Mua sắm", "family": "Gia đình", "weather": "Thời tiết",
    "animals": "Động vật", "environment": "Môi trường & thực vật", "clothing": "Quần áo & thời trang",
    "personality": "Tính cách", "emotions": "Cảm xúc", "sports": "Thể thao", "arts": "Nghệ thuật & giải trí",
    "home": "Nhà cửa & đồ dùng", "finance": "Tài chính & ngân hàng", "transport": "Giao thông",
    "communication": "Giao tiếp & ngôn ngữ", "numbers": "Số đếm & đo lường", "time": "Thời gian",
    "people": "Con người", "actions": "Hành động", "colors": "Màu sắc", "geography": "Địa lý & địa điểm",
    "culture": "Văn hóa & lễ hội",
    "properties": "Đặc điểm & trạng thái", "manner": "Cách thức & mức độ",
}
REFERENCE_TOPICS = dict(enumerate([
    "family", "weather", "work", "clothing", "personality", "food", "environment", "animals", "food", "education",
    "travel", "colors", "transport", "emotions", "food", "work", "culture", "clothing", "culture", "home", "sports",
    "food", "people", "geography", "animals", "society", "sports", "shopping", "geography", "culture", "education",
    "actions", "daily", "numbers", "shopping", "health", "health", "environment", "arts", "food", "communication", "finance"
], 1))
INVENTORY_TOPICS = {
    "work and jobs": "work", "education": "education", "objects and rooms": "home",
    "news, lifestyles and current affairs": "society", "things in the town, shops and shopping": "shopping",
    "food and drink": "food", "travel and services vocab": "travel", "hobbies and pastimes": "arts",
    "nationalities and countries": "geography", "family life": "family", "clothes": "clothing",
    "leisure activities": "arts", "books and literature": "arts", "media": "communication", "shopping": "shopping",
    "personal information": "people", "ways of travelling": "transport", "arts": "arts", "art": "arts",
    "technical and legal language": "society", "holidays": "culture", "film": "arts", "scientific development": "science",
    "colours": "colors", "dimensions": "numbers", "adjectives: personality, description, feelings": "personality",
}
LEXNAME_TOPICS = {
    "noun.food": "food", "noun.animal": "animals", "noun.body": "health", "noun.plant": "environment",
    "noun.feeling": "emotions", "noun.time": "time", "noun.quantity": "numbers", "noun.location": "geography",
    "noun.possession": "finance", "noun.communication": "communication", "noun.person": "people",
    "noun.group": "society", "noun.cognition": "education", "noun.substance": "science", "noun.process": "science",
    "noun.phenomenon": "science", "noun.artifact": "home", "verb.communication": "communication",
    "verb.emotion": "emotions", "verb.motion": "actions", "verb.contact": "actions", "verb.cognition": "education",
    "verb.consumption": "food", "verb.body": "actions", "verb.social": "society", "verb.possession": "finance",
    "verb.change": "actions", "verb.creation": "actions", "verb.perception": "actions", "verb.competition": "sports",
    "verb.weather": "weather", "noun.attribute": "properties", "noun.state": "properties",
}
WORD_TOPICS = {}
for _topic, _words in {
    "technology": "technology technological computer computing software hardware internet website database keyboard digital online electronics electronic network browser laptop smartphone",
    "science": "science scientific scientist physics chemistry chemical biology biological mathematics mathematical laboratory experiment experimental research researcher molecule atom atomic oxygen hydrogen electricity electrical energy gravity evolution theory hypothesis formula",
    "time": "always often never sometimes soon now later early late already yet again currently recently occasionally frequently rarely usually eventually finally immediately previously subsequently daily weekly monthly yearly annual annually tomorrow yesterday today beforehand afterwards meanwhile nowadays formerly future past present",
    "numbers": "many much few little less more most least several enough all each every both half whole double triple single multiple large small long short high low deep wide narrow huge tiny enormous thin thick heavy light equal unequal greater smaller larger maximum minimum average amount degree measurement measure size length height width weight depth distance volume total sum percent percentage",
    "home": "chair desk table cupboard wardrobe sofa couch curtain ceiling carpet furniture floor roof wall shelf mirror kitchen room bedroom bathroom apartment apron",
    "culture": "culture cultural tradition traditional festival ceremony custom celebration parade holiday",
    "environment": "bloom flower blossom plant forest tree seed leaf root branch cereal bush shrub nature natural smoke pollution rubbish waste",
    "transport": "bike bicycle scooter motorcycle train bus taxi vehicle car van truck lorry ferry boat ship railway railroad harbour harbor haven port transport transportation traffic",
    "communication": "article report news newspaper reporter journal magazine broadcast broadcasting journalism",
    "daily": "can could shall should may might must will would",
    "arts": "tune melody violin piano guitar drum flute song music concert album painting sculpture ballet opera cinema theatre theater",
    "finance": "tip bond budget banking currency cash coin loan mortgage insurance debt credit debit invoice revenue profit tax payment salary wage income investment interest saving savings",
    "society": "religion religious ghost belief surrender war conflict",
}.items():
    WORD_TOPICS.update(dict.fromkeys(_words.split(), _topic))


def normalized(value: str) -> str:
    return " ".join(unicodedata.normalize("NFKC", value).casefold().replace("_", " ").split())


def sha256(path: Path) -> str:
    with path.open("rb") as source:
        return hashlib.file_digest(source, "sha256").hexdigest()


class SafeRedirects(HTTPRedirectHandler):
    def redirect_request(self, request, response, code, message, headers, url):
        if urlparse(url).scheme != "https" or urlparse(url).hostname not in ALLOWED_HOSTS:
            raise ValueError("Download redirected outside the source allowlist")
        return super().redirect_request(request, response, code, message, headers, url)


def download(url: str, name: str) -> Path:
    if urlparse(url).scheme != "https" or urlparse(url).hostname not in ALLOWED_HOSTS:
        raise ValueError("Source is outside the HTTPS allowlist")
    CACHE.mkdir(parents=True, exist_ok=True)
    destination = CACHE / name
    if destination.exists():
        return destination
    temporary = destination.with_suffix(destination.suffix + ".part")
    opener = build_opener(SafeRedirects())
    with opener.open(Request(url, headers=HEADERS), timeout=90) as response, temporary.open("wb") as output:
        total = 0
        while block := response.read(1024 * 1024):
            total += len(block)
            if total > MAX_BYTES:
                raise ValueError("Source download exceeds the size limit")
            output.write(block)
        expected = response.headers.get("Content-Length")
        if expected and total != int(expected):
            raise ValueError("Incomplete source download")
    temporary.replace(destination)
    return destination


def check_robots(url: str, cached_name: str) -> None:
    origin = urlparse(url)
    file = download(f"{origin.scheme}://{origin.netloc}/robots.txt", cached_name)
    parser = RobotFileParser()
    parser.parse(file.read_text(encoding="utf-8").splitlines())
    if not parser.can_fetch(HEADERS["User-Agent"], url):
        raise ValueError(f"Source robots policy disallows {url}")


def topic_references(html: str) -> dict[str, dict]:
    from bs4 import BeautifulSoup
    result = {}
    for table in BeautifulSoup(html, "html.parser").find_all("table"):
        heading = table.find_previous(["h2", "h3", "h4"])
        match = re.match(r"(\d+)\.", heading.get_text(strip=True)) if heading else None
        if not match or int(match[1]) not in REFERENCE_TOPICS:
            continue
        for row in table.find_all("tr")[1:]:
            cells = row.find_all(["td", "th"])
            if len(cells) < 3:
                continue
            word = normalized(re.sub(r"\s*\([^)]*\)\s*$", "", cells[0].get_text(" ", strip=True)))
            if re.fullmatch(r"[a-z]+(?:[ '-][a-z]+)*", word):
                part = re.search(r"\(([^)]+)\)", cells[0].get_text(" ", strip=True))
                pos = {"n": "noun", "v": "verb", "adj": "adj", "adv": "adv"}.get(part[1].strip().rstrip(".").lower(), "") if part else ""
                result.setdefault(word, {"topic": REFERENCE_TOPICS[int(match[1])], "pos": pos, "gloss": cells[2].get_text(" ", strip=True).casefold()})
    return result


def get_ipa(entry: dict) -> str:
    for sound in entry.get("sounds", []):
        text = sound.get("ipa", "").strip()
        if not text or len(text) > 100 or any(c.isdigit() for c in text) or re.search(r"[A-Z]|[{}<>]", text):
            continue
        if text.startswith(("/", "[")) and text.endswith(("/", "]")):
            return text
        if any(ord(c) > 127 for c in text) and re.fullmatch(r"[a-z\u0250-\u02ff\u0300-\u036f.\- ()ɡ]+", text):
            return "/" + text + "/"
    return ""


def usable_senses(entry: dict) -> list[dict]:
    result = []
    for sense in entry.get("senses", []):
        tags = set(sense.get("tags", []))
        if tags.intersection({"obsolete", "archaic", "vulgar", "offensive", "derogatory", "slang", "form-of", "no-gloss"}):
            continue
        if any(re.search(r"(?i)(từ cổ|tục|từ lóng|xúc phạm)", tag) for tag in sense.get("raw_tags", [])):
            continue
        if any(re.search(r"(?i)(từ lóng|từ xấu|từ xúc phạm|từ cổ|từ lỗi thời)", category.get("name", "")) for category in sense.get("categories", [])):
            continue
        gloss = " ".join(sense.get("glosses", [])).strip()
        if not 3 <= len(gloss) <= 220 or re.search(r"[<>{}]|https?://", gloss):
            continue
        if re.match(r"(?i)(xem |như |dạng |số nhiều|quá khứ|hiện tại phân từ|viết tắt)", gloss):
            continue
        if normalized(gloss.rstrip(".")) == normalized(entry.get("word", "")):
            continue
        result.append({**sense, "clean_gloss": re.sub(r"\s+", " ", gloss)})
    return result


def bilingual_example(sense: dict, word: str) -> tuple[str, str]:
    for example in sense.get("examples", []):
        text, translation = example.get("text", "").strip(), example.get("translation", "").strip()
        if any(example.get(key) for key in ("ref", "author", "title", "year")):
            continue  # Avoid attributed literary quotations with potentially different rights.
        if text and translation and 5 <= len(text) <= 240 and len(translation) <= 280 and re.search(r"\b" + re.escape(word) + r"[a-z]*\b", text, re.I):
            if not re.search(r"[<>{}]|https?://", text + translation):
                return re.sub(r"\s+", " ", text), re.sub(r"\s+", " ", translation)
    return "", ""


def classify(word: str, pos: str, profile: dict, references: dict, wordnet) -> tuple[str, str]:
    if word in WORD_TOPICS:
        return WORD_TOPICS[word], "wordwave-topic-rule"
    if word in references:
        if not references[word]["pos"] or references[word]["pos"] == pos:
            return references[word]["topic"], "langmaster-reference"
    for field in ("CoreInventory 1", "CoreInventory 2"):
        if profile[field].casefold() in INVENTORY_TOPICS:
            return INVENTORY_TOPICS[profile[field].casefold()], "cefrj-theme"
    wn_pos = {"noun": "n", "verb": "v", "adj": "a", "adv": "r"}.get(pos)
    synsets = wordnet.synsets(word.replace(" ", "_"), pos=wn_pos) if wn_pos else []
    if synsets:
        synset = synsets[0]
        ancestors = {s.name().split(".")[0] for s in synset.closure(lambda s: s.hypernyms())}
        if ancestors.intersection({"computer", "computing_machine", "software", "electronic_equipment", "electronic_device"}): return "technology", "wordnet-hypernym"
        if ancestors.intersection({"vehicle", "transport", "public_transport", "road", "railway"}): return "transport", "wordnet-hypernym"
        if ancestors.intersection({"clothing", "garment", "footwear", "fabric"}): return "clothing", "wordnet-hypernym"
        if ancestors.intersection({"sport", "athletic_game", "sports_equipment"}): return "sports", "wordnet-hypernym"
        if ancestors.intersection({"musical_instrument", "musical_composition", "performing_arts"}): return "arts", "wordnet-hypernym"
        if ancestors.intersection({"weather", "atmospheric_condition", "atmospheric_phenomenon"}): return "weather", "wordnet-hypernym"
        if ancestors.intersection({"disease", "illness", "medical_care", "medical_instrument"}): return "health", "wordnet-hypernym"
        if synset.lexname() in LEXNAME_TOPICS:
            return LEXNAME_TOPICS[synset.lexname()], "wordnet-lexname"
    if pos == "adj": return "properties", "part-of-speech"
    if pos == "adv": return "manner", "part-of-speech"
    return "daily", "general-vocabulary"


SENSE_HINTS = {
    "animals": ["ong", "chim", "động vật", "thú", "côn trùng", "cá", "mèo", "chó", "loài"],
    "food": ["thức ăn", "món", "quả", "rau", "thịt", "cá", "bữa", "uống", "củ", "bánh", "sữa"],
    "health": ["tai", "ngực", "cơ thể", "bệnh", "đau", "thuốc", "thân thể", "răng", "tóc", "mắt", "xương", "máu", "đầu", "lưỡi", "não", "sức khỏe", "sức khoẻ", "ăn", "dinh dưỡng", "kiêng", "sung sức"],
    "home": ["ghế", "bàn", "giường", "tủ", "nhà", "phòng", "cửa", "trần", "sàn", "đèn", "chổi", "sân"],
    "finance": ["ngân hàng", "tài chính", "tiền", "tín dụng", "tài khoản", "thanh toán", "hóa đơn", "chi phí", "lãi suất"],
    "sports": ["thể thao", "bóng", "cầu thủ", "khung thành", "bàn thắng", "trận đấu", "thi đấu"],
    "arts": ["nghệ thuật", "câu lạc bộ", "âm nhạc", "nhạc", "đàn", "tranh", "phim", "vẽ", "giải trí"],
    "technology": ["máy tính", "bàn phím", "điện tử", "phần mềm", "mạng", "công nghệ", "kỹ thuật"],
    "environment": ["hoa", "nở", "cây", "rừng", "môi trường", "lá", "hạt", "khói", "ô nhiễm", "rác"],
    "personality": ["tử tế", "tốt bụng", "nhân hậu", "hiền", "tính", "thật thà", "khiêm tốn"],
    "emotions": ["cảm", "vui", "buồn", "sợ", "giận", "hạnh phúc", "xúc động", "thích", "lo lắng"],
    "colors": ["màu", "sắc", "xanh", "đỏ", "vàng", "đen", "trắng", "tím", "nâu"],
    "numbers": ["lớn", "nhỏ", "cao", "thấp", "nhiều", "ít", "rộng", "dài", "ngắn", "số", "đếm", "độ", "nặng", "nhẹ", "bằng"],
    "culture": ["văn hóa", "văn hoá", "truyền thống", "phong tục", "lễ", "hội", "diễu hành", "kỷ niệm"],
    "education": ["lớp", "học", "giáo dục", "trường", "bài học", "sách", "kiến thức", "giảng"],
    "people": ["con người", "đàn ông", "đàn bà", "trai", "gái", "xuất thân", "ngoại hình", "hoàn cảnh"],
    "shopping": ["mua", "bán", "chợ", "cửa hàng", "giá", "hàng hóa", "hàng hoá"],
    "family": ["chú rể", "chàng rể", "cô dâu", "vợ", "chồng", "con", "gia đình", "họ hàng"],
}


SENSE_CONTEXT_WORDS = {"ear", "bank", "chest", "class", "club", "banana", "fitness", "feel", "groom", "yard", "gold", "violin", "diet", "goal"}


def choose_sense(senses: list[dict], topic: str, reference_gloss: str = "", word: str = "") -> tuple[dict, float]:
    hints = SENSE_HINTS.get(topic, [])
    reference_terms = set(re.findall(r"\w+", reference_gloss)) - {"sự", "của", "cái", "một", "là", "có", "và", "người"}
    def score(item):
        index, sense = item
        text = sense["clean_gloss"].casefold()
        domain = (not word or word in SENSE_CONTEXT_WORDS) and any(re.search(r"(?<!\w)" + re.escape(hint) + r"(?!\w)", text) for hint in hints)
        overlap = len(reference_terms & set(re.findall(r"\w+", text))) / max(1, len(reference_terms))
        if overlap < 0.6: overlap = 0
        return domain * 4 + overlap * 5 - index * 0.04
    item = max(enumerate(senses), key=score)
    return item[1], score(item)


def validate(entries: list[dict], count: int = 3000, allowed_levels=None) -> None:
    if len(entries) != count:
        raise ValueError(f"Expected {count} entries, received {len(entries)}")
    seen = set()
    for entry in entries:
        key = normalized(entry["word"])
        if key in seen or not re.fullmatch(r"[a-z]+(?:[ '-][a-z]+)*", key):
            raise ValueError(f"Duplicate or invalid headword: {key}")
        seen.add(key)
        if entry["level"] not in (allowed_levels or LEVELS) or entry["topic"] not in TOPICS or not entry["meaning"] or not entry["phonetic"]:
            raise ValueError(f"Incomplete entry: {key}")
        if bool(entry["example"]) != bool(entry["exampleMeaning"]):
            raise ValueError(f"Unpaired bilingual example: {key}")
        if not entry.get("sourceSenseId") or not entry.get("sources") or not entry.get("partOfSpeech"):
            raise ValueError(f"Missing provenance: {key}")
        if any(re.search(r"[<>{}]", entry[field]) for field in ("word", "meaning", "phonetic", "example", "exampleMeaning")):
            raise ValueError(f"Markup in entry: {key}")


def build() -> None:
    import nltk
    from nltk.corpus import wordnet
    from wordfreq import zipf_frequency
    check_robots(LANGMASTER, "langmaster-robots.txt")
    check_robots(KAIKKI, "kaikki-robots.txt")
    paths = {
        "wiktionary": download(KAIKKI, "viwiktionary-en.jsonl"),
        "cefrj": download(CEFR, "cefrj.csv"),
        "langmaster": download(LANGMASTER, "langmaster.html"),
    }
    nltk.data.path.insert(0, str(CACHE / "nltk"))
    if not (CACHE / "nltk/corpora/wordnet.zip").exists():
        if not nltk.download("wordnet", download_dir=str(CACHE / "nltk"), quiet=True):
            raise RuntimeError("WordNet download failed")
    references = topic_references(paths["langmaster"].read_text(encoding="utf-8"))
    profiles = defaultdict(list)
    for row in csv.DictReader(paths["cefrj"].open(encoding="utf-8-sig")):
        if row["CEFR"] in LEVELS:
            profiles[normalized(row["headword"])].append(row)
    pronunciations = defaultdict(set)
    for line in paths["wiktionary"].open(encoding="utf-8"):
        entry = json.loads(line)
        word = normalized(entry.get("word", ""))
        if entry.get("lang_code") == "en" and word in profiles and (ipa := get_ipa(entry)):
            pronunciations[(word, entry.get("pos"))].add(ipa)
    heteronyms = {"bass", "bow", "lead", "read", "row", "wind", "tear", "minute", "close", "live", "invalid", "refuse", "entrance", "content", "object", "project", "present", "record", "produce", "desert"}
    best = {}
    rejected = Counter()
    for line in paths["wiktionary"].open(encoding="utf-8"):
        entry = json.loads(line)
        word = normalized(entry.get("word", ""))
        if entry.get("lang_code") != "en" or word not in profiles or not re.fullmatch(r"[a-z]+(?:[ '-][a-z]+)*", word):
            continue
        matching = [p for p in profiles[word] if POS.get(p["pos"]) == entry.get("pos")]
        if not matching:
            rejected["unmatched_part_of_speech"] += 1
            continue
        profile = min(matching, key=lambda p: LEVELS[p["CEFR"]])
        senses = usable_senses(entry)
        phonetic = get_ipa(entry)
        shared_ipa = False
        available = pronunciations[(word, entry.get("pos"))]
        if not phonetic and word not in heteronyms and len(available) == 1:
            phonetic = next(iter(available))
            shared_ipa = True
        frequency = zipf_frequency(word, "en")
        if not senses or not phonetic or frequency < 3.0:
            rejected["missing_gloss_ipa_or_low_frequency"] += 1
            continue
        topic, method = classify(word, entry["pos"], profile, references, wordnet)
        sense, context_score = choose_sense(senses, topic, references.get(word, {}).get("gloss", ""), word)
        example, translation = bilingual_example(sense, word)
        record = {
            "word": word, "phonetic": phonetic, "meaning": sense["clean_gloss"],
            "example": example, "exampleMeaning": translation, "level": profile["CEFR"], "topic": topic,
            "partOfSpeech": entry["pos"], "sourceSenseId": sense.get("id", ""), "topicMethod": method,
            "sources": {"dictionary": "https://vi.wiktionary.org/wiki/" + quote(entry["word"].replace(" ", "_")), "level": "cefrj-1.5"},
        }
        if shared_ipa: record["sources"]["phonetic"] = "wiktionary-same-headword-and-pos"
        if word == "kind" and entry["pos"] == "adj" and "xử lý, để gia công" in record["meaning"]:
            record["meaning"] = record["meaning"].split("xử lý, để gia công")[0].strip().rstrip(".") + "."
            record["editorialNote"] = "Removed a malformed merged tail from the source gloss; retained the original kindness sense."
        if method == "langmaster-reference": record["sources"]["topic"] = LANGMASTER
        elif method.startswith("wordnet"): record["sources"]["topic"] = "https://wordnet.princeton.edu/"
        else: record["sources"]["topic"] = "cefrj-1.5" if method == "cefrj-theme" else "WordWave-topic-rules"
        context_bonus = 0.6 if word in references and references[word]["pos"] == entry["pos"] else 0
        score = frequency + (0.25 if example else 0) + context_bonus + context_score * 0.3 - 0.1 * LEVELS[profile["CEFR"]]
        if word not in best or score > best[word][0]:
            best[word] = (score, record)
    ranked = sorted(best.values(), key=lambda item: (-item[0], item[1]["word"]))
    selected, selected_words, counts = [], set(), Counter()
    # Establish topic coverage first, then favour common vocabulary with a broad mix.
    for topic in TOPICS:
        for score, record in [x for x in ranked if x[1]["topic"] == topic][:20]:
            selected.append(record); selected_words.add(record["word"]); counts[topic] += 1
    for score, record in ranked:
        if len(selected) >= 3000: break
        cap = 400 if record["topic"] == "daily" else 230
        if record["word"] not in selected_words and counts[record["topic"]] < cap:
            selected.append(record); selected_words.add(record["word"]); counts[record["topic"]] += 1
    # Do not discard common words merely to manufacture equal-sized topics.
    for score, record in ranked:
        if len(selected) >= 3000: break
        if record["word"] not in selected_words:
            selected.append(record); selected_words.add(record["word"]); counts[record["topic"]] += 1
    print("Candidate counts: " + json.dumps(dict(Counter(e["topic"] for _, e in ranked))), flush=True)
    validate(selected)
    selected.sort(key=lambda x: (list(TOPICS).index(x["topic"]), LEVELS[x["level"]], x["word"]))
    OUTPUT.mkdir(parents=True, exist_ok=True)
    package = {"schemaVersion": 1, "license": "CC-BY-SA-4.0", "attribution": "Wiktionary contributors via Kaikki; CEFR-J Wordlist v1.5, Yukio Tono/TUFS; Princeton WordNet; wordfreq/SUBTLEX contributors. See sources.htm.", "topics": TOPICS, "entries": selected}
    (OUTPUT / "words-3000.json").write_text(json.dumps(package, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    report = {
        "generatedAt": datetime.now(timezone.utc).isoformat(), "count": len(selected), "uniqueHeadwords": len(selected_words),
        "topics": dict(counts), "levels": dict(Counter(e["level"] for e in selected)),
        "withBilingualUsageExamples": sum(bool(e["example"]) for e in selected), "eligibleHeadwords": len(best),
        "rejected": dict(rejected), "topicMethods": dict(Counter(e["topicMethod"] for e in selected)),
        "datasetSha256": sha256(OUTPUT / "words-3000.json"),
        "sources": {name: {"sha256": sha256(path), "bytes": path.stat().st_size} for name, path in paths.items()},
        "sourceUrls": {"wiktionary": KAIKKI, "cefrj": CEFR, "langmaster": LANGMASTER, "wordnet": "https://wordnet.princeton.edu/", "wordfreq": "https://github.com/rspeer/wordfreq"},
        "qualityNote": "Automatically collected and classified. CEFR-J is matched by POS; topic/sense selection needs editorial review. Usage examples may be phrases. Missing examples are empty, never generated.",
    }
    (OUTPUT / "manifest.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    licenses = OUTPUT / "licenses"
    licenses.mkdir(exist_ok=True)
    import zipfile
    with zipfile.ZipFile(CACHE / "nltk/corpora/wordnet.zip") as archive:
        (licenses / "wordnet-LICENSE.txt").write_bytes(archive.read("wordnet/LICENSE"))
    import wordfreq
    dist = Path(wordfreq.__file__).parent.parent / "wordfreq-3.1.1.dist-info"
    for path in dist.rglob("*LICENSE*"):
        if path.is_file(): shutil.copyfile(path, licenses / ("wordfreq-" + path.name))
    from importlib.metadata import metadata
    (licenses / "wordfreq-NOTICE.md").write_text(metadata("wordfreq").get_payload(), encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--validate-only", action="store_true")
    arguments = parser.parse_args()
    if arguments.validate_only:
        validate(json.loads((OUTPUT / "words-3000.json").read_text(encoding="utf-8"))["entries"])
        print("Validated 3000 unique, sourced vocabulary entries")
    else:
        build()
