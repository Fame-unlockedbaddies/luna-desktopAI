# Luna

An open desktop AI coding agent for Windows. Built on the MIT-licensed
[OpenCode](https://github.com/sst/opencode) engine, with Luna's own interface,
themes and provider-agnostic model backend. Pick between a free hosted model,
your own API key, or a local endpoint.

## Download

👉 **[Luna v0.3.0 for Windows (x64)](https://www.mediafire.com/folder/2weuuevvredw6/LUNA)**
— `Luna-Setup-0.3.0.exe` + `SHA256.txt` in that folder.

Requirements: Windows 10+, 64-bit, ~500 MB free space.

## What it does

- Chat with an AI agent against a real project folder — read/write files, run
  shell commands, search, git — all with approval prompts.
- Stuck on a page Luna can't reach? Luna retries blocked sites via the `r.jina.ai` reader proxy.
- Roblox Studio: Luna can drive it live through the built-in
  [Studio MCP server](https://create.roblox.com/docs/studio/mcp).
- Luna can type into apps and websites for you, click buttons, and read/record
  audio playback (SMTC).
- Every install is private to your PC: Luna keeps a local account database
  (hashed with scrypt) gating the app, no cloud sign-in required — or sign up
  with Discord for a recoverable account.

## First run

1. Install `Luna-Setup-0.3.0.exe`.
2. On the welcome screen: **Sign up** — Luna creates a local account, no email,
   no cloud. Or authorize with Discord the same way (the app opens the
   Discord `Authorize` page for Luna's bundled public client — the redirect
   is locked to `http://127.0.0.1:45555/callback`).
3. Click **Project folder…** and point Luna at your codebase.
4. Ask Luna something. By default she asks before every tool call; flip
   **Ask → Default → Unrestricted** in Settings when you're ready to trust her.

## Requirements

- Windows 10+, 64-bit, ~500 MB disk
- Optional: an API key from Anthropic / OpenAI / OpenRouter / Google / Cerebras
  for the strongest models, or a local model endpoint (Ollama, llama.cpp, …)

## What it's not

- Not Jac Code. Different app, different engine.
- Not a paid product. Free.
- Not code-signed — Windows SmartScreen will show "unknown publisher".
  That's normal for unsigned free software.

## Source / building it yourself

```powershell
git clone https://github.com/Fame-unlockedbaddies/luna-desktopAI
cd luna-desktopAI
npm install
npm run dist     # produces dist\Luna-Setup-<version>.exe
