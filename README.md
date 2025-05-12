# LaunchBox Game Size Manager Plugin

**Project Goal:** Develop a LaunchBox plugin to calculate, display, and manage game disk size information, helping users identify the storage footprint of their games, including estimates for uninstalled storefront titles.

**Current Status:** Phase 2 Development (Storefront Integration)

## Overview

This plugin integrates with LaunchBox to provide detailed information about how much disk space your games occupy. It can scan local installations and ROM files, storing the calculated size directly within LaunchBox custom fields for easy viewing and sorting.

## Core Features (Implemented - v1.0.0 Functionality)

*   Calculates the size of local game installations, ROM files, and CUE sheet related files.
*   Stores data in configurable LaunchBox custom fields:
    *   `Game Size` (e.g., "1.23 GB")
    *   `Game Size Last Scanned` (e.g., "MM/dd/yyyy")
    *   `Game Size Tier` (e.g., "01) > 200 GB", for sorting)
*   Integrates with LaunchBox via:
    *   "Tools" menu for platform-wide scans/data clearing.
    *   Game right-click context menu for operating on selected games.
*   Provides dialogs for selecting scan options and viewing scan issue reports.
*   Handles various path types, including specific logic for PC games, ROMs, and disc images.
*   Identifies games linked via storefront URLs (Steam, Epic, GOG, etc.).
*   Dark themed UI dialogs.

## Current Development Focus (Phase 2)

*   **Fetching Estimated Required Disk Space for Storefront Games:** Implementing functionality to query external databases (initially targeting the [RAWG API](https://rawg.io/apidocs)) to retrieve the "Required Disk Space" for games represented by storefront launcher URLs (e.g., `steam://rungameid/...`).
    *   This data will be stored in a *new*, separate custom field (e.g., `"Est. Required Space"`).
    *   Online lookups will be opt-in per scan and only performed for identified storefront games.
    *   Requires a RAWG API key for the lookup functionality (details on setup TBD - currently requires placing key in `RAWG_API.txt` in project root, which is gitignored).

## Technology

*   C# (.NET Framework 4.7.2)
*   Windows Forms (for UI dialogs)
*   Unbroken.LaunchBox.Plugins API (v13.21.0.0)

## Setup & Installation (Placeholder)

*(Instructions for building the plugin from source or installing a release package will go here once available.)*

---

*Developed by: [reg0lino]*