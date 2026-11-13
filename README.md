# Luna

An open desktop AI coding agent, built on the MIT-licensed [OpenCode](https://github.com/sst/opencode) engine. Electron shell + a local OpenCode server sidecar listening on `127.0.0.1:4196` — the same architecture Jac Code uses, with original Luna branding and your own model provider.

**Project location: `D:\luna-ai`** (moved off the C: drive because C: ran out of space during the build).

## What works

- Chat with the agent against a real project folder (file read/write/edit, shell, search, git — via OpenCode)
- Session list + history persisted by the engine
- **Strict mode** (Settings, on by default for new chats): every tool call waits for your Approve / Always / Reject decision in-app
- **Folder-access consent**: when Luna wants to touch files on your PC, the prompt shows the exact paths/folders with **Just this time** (allow once) or **Always** (engine remembers the pattern), or Reject
- Permission prompts are surfaced in-app — Luna never auto-approves
- API key auth for any OpenCode-supported provider (Anthropic, OpenAI, Google, …), stored by the engine's auth store, never by Luna itself
- Optional `providerID/modelID` override (e.g. `anthropic/claude-sonnet-4-5`)

## Sign up / Log in (Luna account + Discord authorization)

New downloads land on the **LUNA** welcome screen with two tabs: **Sign in** and **Sign up** — no third-party login buttons. Sign up flow:

1. Choose a username (3–20 chars: letters/numbers/`_`) and a password (8+ chars). No email — Luna never asks for one.
2. Click **Create Luna account** → the browser opens Discord's `Authorize` page for Luna's bundled Discord app (`luna-discord.json` holds its client id and the authorize URL).
3. Click **Authorize** → Discord redirects to `http://127.0.0.1:45555/callback#access_token=…`
4. Luna's loopback catches the fragment → verifies the token against Discord's `/users/@me` → **only then** creates the local Luna account (`data\users.json`, scrypt-hashed password) and links your Discord identity (`id`, `username`, `global_name`, `avatar`) to it.
5. Your **Discord avatar, username, display name and ID** appear right next to the chat in the top bar, plus a **Log out** button (and Settings → **Sign out** still works).

**Sign in** takes your username + password and unlocks instantly — Discord authorization already happened at sign up.

### Required one-time setup on the Discord app owner's side

`luna-discord.json` currently ships the app id `1570850681831563424`. That app **must** have the redirect `http://127.0.0.1:45555/callback` registered under **OAuth2 → Redirects**, or the Authorize page will refuse to return to Luna ("Invalid redirect_uri").

Scopes are requested by the URL (`identify` only — no email scope) — Discord has **no portal toggle** for scopes; only the redirect matters.

**Honest limits:** local accounts are cosmetics, not DRM. The implicit token flow (`response_type=token`) is the right pattern for a desktop app without a secret — the token is only exchanged for your profile server-side at `/users/@me`, and your Luna password is hashed with scrypt. Anyone with file access to `D:\luna-ai\data\` can still wipe or read the account folder. For real multi-user security you'd need a hosted backend.

## Roblox Studio (built-in, via MCP)

Luna can drive Roblox Studio live — read the data model, write scripts, run Luau, playtest — through Studio's built-in MCP server. No extra install: Studio ships it as `%LOCALAPPDATA%\Roblox\mcp.bat`.

Setup (one time):
1. Open Roblox Studio and your place.
2. Open **Assistant** (top-right) → **… → Manage MCP Servers**.
3. Turn on **Enable Studio as MCP server**.
4. Keep Studio open while using Luna.

Then ask Luna: e.g. "Use Roblox Studio to list what's in Workspace" / "add a kill brick script to ServerScriptService". Every tool call passes a `studio_id` (look it up with `list_roblox_studios`). Luna's engine home (`D:\luna-ai\engine-home\opencode.jsonc`) registers the server, merged with your global OpenCode MCP config. Only connect Luna to Studio sessions you own — the agent's edits are click-confirmed in Luna first (Ask mode) unless you enable Unrestricted.

## Speed

Luna leans on the free GLM-Flash backend with **low** reasoning effort by default (the fastest tier this model offers). Change it anytime in the composer's Reasoning dropdown (`low`/`high`/`max`), and Luna polls for new messages every 1s instead of 1.5s. True response speed is dominated by the provider queue, not the app.

## What Luna is NOT

- Not Jac Code. "JCoder" models and the `jaccoder-*.workers.dev` gateway are proprietary — Luna can't and doesn't use them.
- Not a cheat/crack distribution service. It's a general coding agent; use it within the law and your providers' terms.

## Requirements

- Windows 10+, an OpenCode-supported provider API key (or a local model endpoint), and a project folder.

## Run from source

```powershell
cd D:\luna-ai
npm install
npm start
```

First run: **Project folder…** → pick the codebase, **Settings** → provider ID + API key (e.g. `anthropic` + `sk-…`) → **Save**. Strict mode is on; turn it off in Settings only if you understand the risk.

## Distributing Luna & auto-updates

Luna ships with a built-in auto-updater: it polls `<updateUrl>/latest.json` on a host you control (every 6h and on demand in Settings → **Check for updates now**), and if the remote `version` is newer, prompts the user, downloads the installer, verifies its SHA-256, and runs it silently into the new version.

**You need two things to distribute Luna:**

1. **An update host.** Easiest: a GitHub repo (even private, as long as users have access). Releases give free hosting + TLS + a stable `latest/download` URL. Or any static HTTPS host (Netlify/Cloudflare Pages/R2/your own server).
2. **The build.** One command produces everything:

```powershell
cd D:\luna-ai
node scripts\release.mjs 0.3.0 github:you\luna-releases
```

(that bumps `version`, runs `npm run dist`, writes the SHA-256'd `dist/latest.json` and prints the exact upload steps). Or use a generic host: `node scripts\release.mjs 0.3.0 generic:https://updates.example.com/luna/`.

**How users get the app:** hand them the installer NSIS exe from your release — one file is the whole app (~350 MB; the OpenCode engine is bundled).

**Honesty notes:** no code signing (SmartScreen will show "unknown publisher" on first launch; a real cert fixes that); the updater only accepts HTTPS manifests and verifies the installer's SHA-256 before running it; whoever controls the update host controls the app, so protect the GitHub account/token you use for releases. On launch, packaged builds re-check silently every 6h; the "install now / later" prompt is on the user.

## Build the Windows installer

```powershell
cd D:\luna-ai
npm run dist
```

Produces `D:\luna-ai\dist\Luna-Setup-<version>.exe` (NSIS, per-user install, desktop + Start Menu shortcuts, uninstalls cleanly). The installer bundles the OpenCode engine binary (`resources\engine\opencode.exe`) and OpenCode's MIT license notice. Bump `version` first (or use `node scripts\release.mjs <x.y.z> <feed>`).

## Code signing

Signing is **not** free and cannot be faked: you need a real Authenticode certificate (OV from ~$200/yr, or EV for instant SmartScreen reputation) from a CA like SSL.com, DigiCert, or Sectigo.

Once you have one, sign automatically by setting environment variables before `npm run dist` — electron-builder picks them up with no config changes:

```powershell
# Recommended (keeps the key out of the repo):
$env:CSC_LINK = "C:\path\to\luna.pfx"          # or a base64-encoded PFX string / HSM URL
$env:CSC_KEY_PASSWORD = "pfx-password"
npm run dist
```

You can also set `win.certificateFile` / `win.signtoolOptions` in `package.json`'s `build` section, or use Azure Trusted Signing (Microsoft's ~$10/mo Authenticode service) via `signtoolOptions.sign` — see electron-builder docs. Do **not** self-sign and install your own root CA to fake trust; that breaks the security model instead of satisfying it. Unsigned builds work but show SmartScreen's "unknown publisher" warning.

## Security notes

- The engine listens on loopback only; Luna sets a random `OPENCODE_SERVER_PASSWORD` per launch, so other local processes get 401s.
- The renderer CSP forbids direct network access; all engine traffic goes through the IPC proxy in `main.cjs` (whitelisted methods/paths, loopback origin only).
- `luna-control.exe` gives Luna (and anything that can reach it) the ability to **type, click and focus windows on this PC** — same power as a person at the keyboard. Ask mode gates every command Luna wants to run; Unrestricted removes that gate.
- Strict mode = session-scoped `[{permission:"*", pattern:"*", action:"ask"}]` ruleset verified against the engine. Applies to chats created after the toggle is on; existing chats keep their rules.

## Layout

- `main.cjs` — Electron main: engine sidecar, IPC proxy, event forwarding (permission asks, idle, errors)
- `preload.cjs` — minimal `window.luna` bridge
- `renderer/` — chat UI (no frameworks; strict CSP)
- `build/icon.png` — Luna icon (crescent + wordmark)
- `dist/` — installer output (gitignored)
- `data/luna.db` — **Luna's private conversation database** (gitignored). Your connected providers are shared with OpenCode; Luna's chats are not.

License: MIT (Luna). OpenCode remains MIT © its authors.
