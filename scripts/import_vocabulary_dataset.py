"""Preview or transactionally import the attributed vocabulary package into LOCAL PostgreSQL.

Existing headwords are preserved. --apply requires an explicit matching database
name. Credentials are read from existing configuration, never logged or exported.
"""
from __future__ import annotations

import argparse
from datetime import datetime, timezone
import json
import os
from pathlib import Path
import re

from build_vocabulary_dataset import CACHE, OUTPUT, ROOT, normalized, sha256, validate


def connection_settings(expected_database: str | None) -> dict:
    value = os.environ.get("ConnectionStrings__DefaultConnection")
    if not value:
        raw = (ROOT / "backend/WordWave.Api/appsettings.json").read_text(encoding="utf-8-sig")
        match = re.search(r'"DefaultConnection"\s*:\s*("(?:[^"\\]|\\.)*")', raw)
        if not match:
            raise ValueError("DefaultConnection is not configured")
        value = json.loads(match[1])
    settings = dict(part.split("=", 1) for part in value.split(";") if "=" in part)
    settings = {key.strip().casefold(): item for key, item in settings.items()}
    if settings.get("host") not in {"127.0.0.1", "localhost", "::1"}:
        raise ValueError("The importer only supports a confirmed local PostgreSQL target")
    if expected_database and settings.get("database") != expected_database:
        raise ValueError("Configured database does not match --expected-database")
    return {"host": settings["host"], "port": int(settings.get("port", 5432)), "dbname": settings["database"], "user": settings["username"], "password": settings["password"], "connect_timeout": 10}


def import_data(apply: bool, expected_database: str | None, package_path: Path | None = None, manifest_path: Path | None = None) -> dict:
    import psycopg
    from psycopg.rows import dict_row
    if apply and not expected_database:
        raise ValueError("--apply requires --expected-database")
    package_path = package_path or OUTPUT / "words-3000.json"
    package = json.loads(package_path.read_text(encoding="utf-8"))
    entries = package["entries"]
    manifest = json.loads((manifest_path or OUTPUT / "manifest.json").read_text(encoding="utf-8"))
    validate(entries, manifest["count"], {"A1", "A2", "B1", "B2", "C1", "C2", "N/A"})
    if sha256(package_path) != manifest["datasetSha256"]:
        raise ValueError("Dataset differs from its validated manifest")
    settings = connection_settings(expected_database)
    with psycopg.connect(**settings, row_factory=dict_row) as connection:
        with connection.cursor() as cursor:
            if apply:
                cursor.execute("SELECT pg_advisory_xact_lock(81357042)")
                cursor.execute("LOCK TABLE vocabulary IN SHARE ROW EXCLUSIVE MODE")
            cursor.execute("SELECT * FROM vocabulary ORDER BY id")
            previous = cursor.fetchall()
            existing = {normalized(row["word"]) for row in previous}
            incoming = [entry for entry in entries if normalized(entry["word"]) not in existing]
            report = {
                "applied": apply, "database": settings["dbname"], "host": settings["host"], "port": settings["port"],
                "before": len(previous), "newWords": len(incoming), "alreadyPresent": len(entries) - len(incoming),
                "datasetSha256": manifest["datasetSha256"], "importedIds": [],
            }
            if apply:
                stamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%S%fZ")
                backup_dir = CACHE.parent / "vocabulary-backups"
                backup_dir.mkdir(parents=True, exist_ok=True)
                backup = backup_dir / f"vocabulary-before-{stamp}.json"
                backup.write_text(json.dumps(previous, ensure_ascii=False, indent=2, default=str) + "\n", encoding="utf-8")
                for entry in incoming:
                    cursor.execute(
                        "INSERT INTO vocabulary (word, phonetic, meaning, example, example_meaning, level, topic, created_at) "
                        "VALUES (%s, %s, %s, %s, %s, %s, %s, %s) RETURNING id",
                        (entry["word"], entry["phonetic"], entry["meaning"], entry["example"], entry["exampleMeaning"], entry["level"], entry["topic"], datetime.now(timezone.utc)),
                    )
                    report["importedIds"].append(cursor.fetchone()["id"])
                cursor.execute("SELECT count(*) AS count FROM vocabulary")
                report["after"] = cursor.fetchone()["count"]
                if report["after"] != len(previous) + len(incoming):
                    raise ValueError("Post-import count differs from the expected count; transaction rolled back")
                report["backup"] = str(backup)
                report_path = backup_dir / f"import-{stamp}.json"
                report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
                report["report"] = str(report_path)
        # Psycopg commits only after the complete validated import succeeds.
    print(json.dumps({key: value for key, value in report.items() if key != "importedIds"}, ensure_ascii=False, indent=2))
    return report


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apply", action="store_true", help="Commit the import; default is read-only preview")
    parser.add_argument("--expected-database", help="Required confirmation of the database name when applying")
    parser.add_argument("--package", type=Path, help="Dataset JSON; defaults to the original 3000 words")
    parser.add_argument("--manifest", type=Path, help="Manifest matching the selected dataset")
    arguments = parser.parse_args()
    import_data(arguments.apply, arguments.expected_database, arguments.package, arguments.manifest)
