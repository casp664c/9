# Kaninbanker project rules

- Android is the primary and production target.
- The project must remain buildable through Unity Build Automation without requiring the user to own or connect a PC.
- Prefer code-driven scene/bootstrap setup so cloud builds do not depend on manual Editor clicks.
- Keep dependencies minimal and reversible.
- Never commit secrets, GitHub tokens, Unity credentials, keystores, or passwords.
- Validate changes with the strongest available evidence; do not claim Unity compilation or APK success without Unity/CI build logs.
- Preserve GitHub `main` as the stable branch. Develop substantial changes on branches and review before merge.
