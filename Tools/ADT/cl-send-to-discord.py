#!/usr/bin/env python3
import os
import json
import re
import requests
from datetime import datetime

# Эмодзи, которые поддерживаются напрямую Discord
EMOJI_MAP = {
    "add": "✨",
    "remove": "❌",
    "delete": "🗑️",
    "tweak": "🛠️",
    "fix": "🐛"
}

EMOJI_ORDER = ["add", "remove", "delete", "tweak", "fix"]
DARK_GREEN = 0x1F8B4C  # Тёмно-зелёный цвет эмбеда

def extract_changelog(text):
    match = re.search(r":cl:\s*(.*?)\s*(?:<!--|\Z)", text, re.DOTALL)
    if not match:
        return None

    content = match.group(1).strip()
    groups = {key: [] for key in EMOJI_MAP}

    for line in content.splitlines():
        line = line.strip()
        if not line.startswith("-"):
            continue
        line_content = line[1:].strip()
        for key in EMOJI_MAP:
            if line_content.lower().startswith(f"{key}:"):
                desc = line_content[len(key) + 1:].strip().capitalize()
                groups[key].append(f"{EMOJI_MAP[key]} {desc}")
                break

    # если вообще нет валидных строк
    if all(len(v) == 0 for v in groups.values()):
        return None

    # ===== новый аккуратный сбор =====
    sections = []
    for key in EMOJI_ORDER:
        if groups[key]:
            sections.append("\n".join(groups[key]))
    return "\n".join(sections)

def create_embed(changelog, author_name, author_avatar, branch):
    embed = {
        "title": "Pull-Request был замержен!",
        "description": (
            f"**🆑 Автор:** {author_name}\n"
            f"**Изменения:**\n\n"
            f"{changelog}"
        ),
        "color": 0x006400,  # Тёмно-зелёный цвет
        "footer": {
            "text": f"{datetime.utcnow().strftime('%d.%m.%Y %H:%M UTC')}"
        },
        "thumbnail": {
            "url": author_avatar
        }
    }
    return embed

def main():
    event_path = os.environ.get("GITHUB_EVENT_PATH")
    webhook_url = os.environ.get("DISCORD_WEBHOOK_CL")

    if not event_path or not webhook_url:
        print("Missing required environment variables.")
        return

    with open(event_path, 'r', encoding='utf-8') as f:
        event = json.load(f)

    pr = event.get("pull_request")
    if not pr or not pr.get("merged"):
        print("PR not merged or no pull request data.")
        return

    body = pr.get("body", "")
    author = pr.get("user", {}).get("login", "Unknown")
    avatar_url = pr.get("user", {}).get("avatar_url", "")
    branch = pr.get("base", {}).get("ref", "master")

    changelog = extract_changelog(body)

    if not changelog:
        print("No valid changelog found. Skipping PR.")
        return

    embed = create_embed(changelog, author, avatar_url, branch)

    headers = {"Content-Type": "application/json"}
    payload = {"embeds": [embed]}
    response = requests.post(webhook_url, headers=headers, data=json.dumps(payload))
    if response.status_code >= 400:
        print(f"❌ Failed to send webhook: {response.status_code} - {response.text}")
    else:
        print("✅ Webhook sent successfully.")

if __name__ == "__main__":
    main()
