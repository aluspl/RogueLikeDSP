# Wydanie z CI (do przeniesienia na Bitrise)

Workflow GitHub Actions został usunięty – budowanie i wysyłkę do sklepów przejmie Bitrise. Poniższa lista sekretów
i kroków (te same skrypty co lokalnie: `GODOT/tools/testflight_upload.sh`, `GODOT/tools/android_release.sh --upload`)
posłuży do konfiguracji Bitrise.
wysyła build iOS do TestFlight i AAB do Google Play (ścieżka internal). Te same skrypty co lokalnie:
`GODOT/tools/testflight_upload.sh`, `GODOT/tools/android_release.sh --upload`.

## Sekrety (Settings → Secrets and variables → Actions)

Repo jest publiczne – wartości trafiają tylko do sekretów GitHuba, nigdy do plików.

| Sekret | Skąd |
|---|---|
| `ASC_KEY_P8_BASE64` | `base64 -i AuthKey_XXXX.p8 \| pbcopy` (klucz App Store Connect API) |
| `ASC_KEY_ID`, `ASC_ISSUER_ID` | App Store Connect → Users and Access → Integrations (jak w `GODOT/.env.local`) |
| `APPLE_TEAM_ID` | Membership (jak w `GODOT/.env.local`) |
| `IOS_DIST_P12_BASE64` | Pęk kluczy → „Apple Distribution: …” → Eksportuj jako .p12 (z hasłem), potem `base64 -i dist.p12 \| pbcopy` |
| `IOS_DIST_P12_PASSWORD` | hasło nadane przy eksporcie .p12 |
| `ANDROID_KEYSTORE_BASE64` | `base64 -i release.jks \| pbcopy` (klucz uploadu) |
| `ANDROID_KEY_ALIAS`, `ANDROID_KEYSTORE_PASSWORD` | jak w `GODOT/.env.local` |
| `PLAY_SERVICE_ACCOUNT_JSON` | cała zawartość pliku JSON konta serwisowego |

Wygodnie przez `gh`: `gh secret set ASC_KEY_ID` (wkleja się wartość), a pliki:
`base64 -i plik | gh secret set NAZWA`.

## Uwagi
- Pierwsze uruchomienie warto zrobić ręcznie (workflow_dispatch) i obejrzeć logi.
- TestFlight wymaga zaakceptowanych umów w App Store Connect (inaczej 403 „required agreement”).
