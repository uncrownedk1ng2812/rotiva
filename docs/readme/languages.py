"""Tulis ulang baris pilihan bahasa di README.md dan semua terjemahannya (docs/readme/README.<kode>.md).

Pemakaian (dari root repo):  python docs/readme/languages.py
Baris bahasa berada di antara <!-- languages --> dan <!-- /languages -->.
"""
import pathlib
import re

ROOT = pathlib.Path(__file__).resolve().parents[2]
LANGUAGES = [
    ("en", "English"),
    ("es", "Español"),
    ("ja", "日本語"),
    ("ko", "한국어"),
    ("zh-CN", "简体中文"),
    ("zh-TW", "繁體中文"),
    ("pl", "Polski"),
    ("hi", "हिन्दी"),
    ("ru", "Русский"),
    ("ar", "العربية"),
    ("pt-BR", "Português (Brasil)"),
    ("pt-PT", "Português (Portugal)"),
    ("fr", "Français"),
    ("th", "ไทย"),
    ("tr", "Türkçe"),
    ("id", "Bahasa Indonesia"),
    ("it", "Italiano"),
]
BLOCK = re.compile(r"<!-- languages -->.*?<!-- /languages -->", re.S)


def path_of(code):
    return ROOT / "README.md" if code == "en" else ROOT / "docs" / "readme" / f"README.{code}.md"


def bar(current):
    here = path_of(current).parent
    links = []
    for code, name in LANGUAGES:
        if code == current:
            links.append(f"<b>{name}</b>")
        else:
            href = path_of(code).relative_to(ROOT).as_posix()
            if here != ROOT:
                href = "../../" + href
            links.append(f'<a href="{href}">{name}</a>')
    return '<!-- languages -->\n<p align="center">\n  ' + " ·\n  ".join(links) + "\n</p>\n<!-- /languages -->"


for code, _ in LANGUAGES:
    path = path_of(code)
    if not path.exists():
        print(f"missing: {path.relative_to(ROOT)}")
        continue
    text = path.read_text(encoding="utf-8")
    if not BLOCK.search(text):
        raise SystemExit(f"{path.relative_to(ROOT)} has no <!-- languages --> block")
    path.write_text(BLOCK.sub(lambda _: bar(code), text), encoding="utf-8", newline="\n")
