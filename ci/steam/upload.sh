#!/usr/bin/env bash
#
# Upload a Windows build to Steam via steamcmd.
#
#   STEAM_USER=builder STEAM_PASS=secret ./ci/steam/upload.sh path/to/windows-build
#
# Prereqs:
#   - steamcmd installed and on PATH (https://developer.valvesoftware.com/wiki/SteamCMD)
#   - A Steamworks "build account" with 2FA handled (run steamcmd once interactively
#     to cache the Steam Guard token, or use a config/ token mount in CI).
#   - Fill in YOUR_APP_ID / YOUR_DEPOT_ID in ci/steam/app_build.vdf and depot_build.vdf.
#
set -euo pipefail

BUILD_DIR="${1:?usage: upload.sh <windows-build-dir>}"
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

: "${STEAM_USER:?set STEAM_USER}"
: "${STEAM_PASS:?set STEAM_PASS}"

# Stage the build under the content root referenced by the VDFs.
rm -rf "$HERE/content"
mkdir -p "$HERE/content" "$HERE/output"
cp -r "$BUILD_DIR"/. "$HERE/content/"

steamcmd \
	+login "$STEAM_USER" "$STEAM_PASS" \
	+run_app_build "$HERE/app_build.vdf" \
	+quit

echo "Upload submitted. Set the build live from the Steamworks partner site (or via 'setlive')."
