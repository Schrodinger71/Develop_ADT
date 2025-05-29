#!/usr/bin/env python3
import os
import json
import re
import requests
from datetime import datetime, timezone

# Поддерживаемые эмодзи и порядок отображения
EMOJI_MAP = {
    "add": "✨",
    "remove": "❌",
    "delete": "🗑️",
    "tweak": "🛠️",
    "fix": "🐛",
}
EMOJI_ORDER = ["add", "remove", "delete", "tweak", "fix"]

MAX_FIELD_LENGTH = 1024  # Максимальная длина поля Embed
DARK_GREEN = 0x1F8B4C    # Цвет Embed

def smart_truncate(text, max_length):
    """Умная обрезка текста: обрезает до максимальной длины, не разрывая слова или предложения."""
    if len(text) <= max_length:
        return text

    truncated_text = text[:max_length]
    last_period = truncated_text.rfind(".")
    if last_period == -1:
        return truncated_text.strip() + "..."
    return truncated_text[:last_period + 1].strip() + "..."

def extract_changelog_section(text):
    """Извлечение и форматирование блока :cl: с изменениями."""
    text = re.sub(r"<!--.*?-->", "", text, flags=re.DOTALL)
    match = re.search(r"^(:cl:.*?(?:\n- .+)+)", text, re.MULTILINE | re.DOTALL)
    if not match:
        return None, text.strip()

    cl_text = match.group(1).strip()
    remaining = text[match.end():].strip()

    # Разбор :cl: секции
    groups = {key: [] for key in EMOJI_MAP}
    for line in cl_text.splitlines():
        line = line.strip()
        if not line.startswith("-"):
            continue
        content = line[1:].strip()
        for key in EMOJI_MAP:
            if content.lower().startswith(f"{key}:"):
                description = content[len(key) + 1:].strip().capitalize()
                groups[key].append(f"{EMOJI_MAP[key]} {description}")
                break

    if all(len(v) == 0 for v in groups.values()):
        return None, text.strip()

    # Форматирование в секции по порядку
    formatted_sections = []
    for key in EMOJI_ORDER:
        if groups[key]:
            formatted_sections.append("\n".join(groups[key]))

    return "\n".join(formatted_sections), remaining

def create_embed(pr_data):
    """Создание объекта Embed для Discord Webhook."""
    description = f"{pr_data['changelog']}"
    if pr_data["remaining"]:
        description += f"\n\n{pr_data['remaining']}"

    description = smart_truncate(description, MAX_FIELD_LENGTH)

    embed = {
        "title": f"Пулл-реквест замержен: {pr_data['title']}",
        "url": pr_data["url"],
        "color": DARK_GREEN,
        "timestamp": pr_data["merged_at"].isoformat(),
        "thumbnail": {
            "url": pr_data["avatar_url"]
        },
        "fields": [
            {
                "name": "Изменения:",
                "value": description,
                "inline": False,
            },
            {
                "name": "Автор:",
                "value": pr_data["author"],
                "inline": False,
            },
        ],
        "footer": {
            "text": "Дата мержа",
        },
    }

    return embed

def main():
    event_path = os.environ.get("GITHUB_EVENT_PATH")
    webhook_url = os.environ.get("DISCORD_WEBHOOK_CL")

    if not event_path or not webhook_url:
        print("❌ Переменные окружения не заданы.")
        return

    with open(event_path, "r", encoding="utf-8") as f:
        event = json.load(f)

    pr = event.get("pull_request")
    if not pr or not pr.get("merged"):
        print("❌ PR не замержен или данные отсутствуют.")
        return

    # Извлечение и обработка данных PR
    merged_at = datetime.strptime(pr["merged_at"], "%Y-%m-%dT%H:%M:%SZ").replace(tzinfo=timezone.utc)
    author = pr.get("user", {}).get("login", "Unknown")
    title = pr.get("title", "Без названия")
    url = pr.get("html_url", "")
    body = pr.get("body", "").strip()
    avatar_url = pr.get("user", {}).get("avatar_url", "")

    changelog, remaining = extract_changelog_section(body)

    if not changelog:
        print("❌ Не удалось найти changelog.")
        return

    pr_data = {
        "merged_at": merged_at,
        "title": title,
        "url": url,
        "author": author,
        "avatar_url": avatar_url,
        "changelog": changelog,
        "remaining": remaining,
    }

    embed = create_embed(pr_data)
    payload = {"embeds": [embed]}
    headers = {"Content-Type": "application/json"}

    try:
        response = requests.post(webhook_url, headers=headers, data=json.dumps(payload))
        response.raise_for_status()
        print("✅ Webhook успешно отправлен.")
    except requests.RequestException as e:
        print(f"❌ Ошибка при отправке Webhook: {e}")

if __name__ == "__main__":
    main()
