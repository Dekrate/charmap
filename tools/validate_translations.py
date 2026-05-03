#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Translation validator for charmap application.
Checks for missing translations, inconsistent keys, and hardcoded strings.
Usage: python validate_translations.py [solution_root]
"""
import sys
import os
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import Set, List, Tuple

POLISH_CHARS = set("ąćęłńóśźżĄĆĘŁŃÓŚŹŻ")

REQUIRED_KEYS = {
    "AppTitle", "FontLabel", "HelpButton", "CopyLabel",
    "SelectButton", "CopyButton", "AdvancedView",
    "HelpTitle", "HelpContent", "CloseButton"
}

HARDCODED_POLISH_STRINGS = [
    "Czcionka:", "Znaki do skopiowania:", "Widok zaawansowany",
    "Pomoc", "Wybierz", "Kopiuj", "Zamknij"
]


def load_resw_keys(path: Path) -> Set[str]:
    tree = ET.parse(path)
    root = tree.getroot()
    keys = set()
    for data in root.findall("data"):
        name = data.get("name")
        if name:
            keys.add(name)
    return keys


def check_keys(pl_keys: Set[str], en_keys: Set[str]) -> List[str]:
    errors = []
    missing_in_en = pl_keys - en_keys
    missing_in_pl = en_keys - pl_keys
    if missing_in_en:
        errors.append(f"Keys missing in EN: {sorted(missing_in_en)}")
    if missing_in_pl:
        errors.append(f"Keys missing in PL: {sorted(missing_in_pl)}")
    all_keys = pl_keys | en_keys
    missing_required = REQUIRED_KEYS - all_keys
    if missing_required:
        errors.append(f"Required keys missing in both languages: {sorted(missing_required)}")
    return errors


def check_empty_values(path: Path) -> List[str]:
    errors = []
    tree = ET.parse(path)
    root = tree.getroot()
    for data in root.findall("data"):
        name = data.get("name")
        value_elem = data.find("value")
        if value_elem is None or not value_elem.text or not value_elem.text.strip():
            errors.append(f"Empty value for key '{name}' in {path.name}")
    return errors


def check_polish_chars(path: Path) -> List[str]:
    errors = []
    text = path.read_text(encoding="utf-8")
    if not any(c in text for c in POLISH_CHARS):
        errors.append(f"File {path.name} does not contain any Polish characters")
    return errors


def check_hardcoded_strings(src_dir: Path) -> List[str]:
    errors = []
    cs_files = list(src_dir.rglob("*.cs"))
    for file in cs_files:
        lines = file.read_text(encoding="utf-8").splitlines()
        for i, line in enumerate(lines, start=1):
            if "GetString(" in line:
                continue
            for s in HARDCODED_POLISH_STRINGS:
                if f'"{s}"' in line:
                    errors.append(f"Hardcoded Polish string '{s}' at {file.name}:{i}")
    return errors


def check_xaml_named_controls(xaml_path: Path) -> List[str]:
    errors = []
    required = [
        'x:Name="AppTitleBar"', 'x:Name="TitleTextBlock"',
        'x:Name="FontLabel"', 'x:Name="FontComboBox"',
        'x:Name="HelpButton"', 'x:Name="CharacterGridView"',
        'x:Name="CopyLabel"', 'x:Name="CopyTextBox"',
        'x:Name="SelectButton"', 'x:Name="CopyButton"',
        'x:Name="AdvancedViewCheckBox"'
    ]
    text = xaml_path.read_text(encoding="utf-8")
    for req in required:
        if req not in text:
            errors.append(f"Missing XAML control: {req}")
    return errors


def check_mica_usage(xaml_path: Path, cs_dir: Path) -> List[str]:
    errors = []
    xaml_text = xaml_path.read_text(encoding="utf-8")
    cs_files = list(cs_dir.rglob("*.cs"))
    has_mica_in_xaml = "MicaBackdrop" in xaml_text or "MicaKind.BaseAlt" in xaml_text
    has_mica_in_cs = any("MicaBackdrop" in f.read_text(encoding="utf-8") for f in cs_files)
    if not has_mica_in_xaml and not has_mica_in_cs:
        errors.append("Mica backdrop not found in XAML or C# code")
    return errors


def main() -> int:
    root = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).parent.parent / "src"
    charmap_dir = root / "charmap"
    pl_resw = charmap_dir / "Strings" / "pl" / "Resources.resw"
    en_resw = charmap_dir / "Strings" / "en" / "Resources.resw"
    xaml_path = charmap_dir / "MainWindow.xaml"

    all_errors: List[str] = []

    if not pl_resw.exists():
        all_errors.append(f"PL resw not found: {pl_resw}")
    if not en_resw.exists():
        all_errors.append(f"EN resw not found: {en_resw}")

    if pl_resw.exists() and en_resw.exists():
        pl_keys = load_resw_keys(pl_resw)
        en_keys = load_resw_keys(en_resw)
        all_errors.extend(check_keys(pl_keys, en_keys))
        all_errors.extend(check_empty_values(pl_resw))
        all_errors.extend(check_empty_values(en_resw))
        all_errors.extend(check_polish_chars(pl_resw))

    if charmap_dir.exists():
        all_errors.extend(check_hardcoded_strings(charmap_dir))

    if xaml_path.exists():
        all_errors.extend(check_xaml_named_controls(xaml_path))
        all_errors.extend(check_mica_usage(xaml_path, charmap_dir))

    if all_errors:
        print("VALIDATION FAILED", file=sys.stderr)
        for err in all_errors:
            print(f"  [ERROR] {err}", file=sys.stderr)
        return 1

    print("VALIDATION PASSED: All translations, controls, and Mica setup are correct.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
