# Contributing to Rotiva

Thanks for helping! Bug reports, ideas and pull requests are all welcome. Rotiva is maintained by one
person, so replies can take a few days.

By taking part you agree to follow the [Code of Conduct](CODE_OF_CONDUCT.md).

## Reporting a bug

Open an [issue](https://github.com/uncrownedk1ng2812/rotiva/issues) with:

- your OS and version (e.g. Windows 11 24H2, macOS 15 on Apple Silicon, Ubuntu 24.04 on X11/Wayland)
- the Rotiva version (tray menu header, or the file name you downloaded)
- what you did, what you expected and what happened
- for a failed conversion: the file type and size, and the message Rotiva showed. Please don't attach
  private files; a small sample file that shows the problem is best.

Found a security problem? Don't open an issue; see [SECURITY.md](SECURITY.md).

## Suggesting a feature

Open an issue and describe what you want to do, not only the feature (for example "I convert screenshots
to WebP every day" rather than "add a WebP button"). That helps find the simplest fit for the wheel.

## Pull requests

1. For anything bigger than a small fix, open an issue first so we can agree on the approach.
2. Fork the repo and create a branch from `main`.
3. Build and test (see [BUILDING.md](BUILDING.md)):

   ```
   dotnet build src/Rotiva/Rotiva.csproj           # Windows app
   dotnet build src/Rotiva.Desktop                 # macOS & Linux app
   dotnet test src/Rotiva.Core.Tests               # conversion core
   ```

   Try your change in the running app too, not only the tests. For a change to the wheel, say on which
   OS you tried it.
4. Keep the pull request focused on one thing, and describe what changed and why.

### Where things are

| Folder | What |
|---|---|
| `src/Rotiva.Core` | Formats and conversions, shared by all platforms |
| `src/Rotiva` | Windows app (WPF) |
| `src/Rotiva.Desktop` | macOS & Linux app (Avalonia) |
| `installer/` | Windows installer, macOS `.dmg` and Linux AppImage scripts |
| repo root | Website (GitHub Pages): `index.html`, `id/`, `app.js` |

### Style

- Match the code around your change: naming, layout and how much it is commented.
- Text that users see (app, installer, website, README) is in **English**. The Indonesian website is in
  `id/`. Code comments may be in English or Indonesian.
- No new runtime dependencies or bundled engines without discussing it in an issue first; every engine
  adds to the installer size and needs its license listed in
  [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
- Rotiva works offline. Changes must not send files or data over the network.

## Translating the README

The README is translated in [`docs/readme/`](docs/readme) (`README.<language>.md`). When the English
README changes, the translations should follow; fixes by native speakers are very welcome. To add a
language, add it to `LANGUAGES` in [`docs/readme/languages.py`](docs/readme/languages.py), create the file
with an empty `<!-- languages -->` `<!-- /languages -->` block at the top, and run
`python docs/readme/languages.py` to refresh the language links in every README.

## License

Rotiva is licensed under [GPL-3.0](LICENSE). By sending a pull request you agree that your contribution
is licensed under the same terms. The name "Rotiva", the logo and the icon are not covered by the GPL;
see [TRADEMARKS.md](TRADEMARKS.md).
