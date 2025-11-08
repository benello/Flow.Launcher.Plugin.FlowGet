# Winget plugin for FlowLauncher

---

This plugin allows you to manage applications using the [Windows Package Manager (winget)](https://learn.microsoft.com/en-us/windows/package-manager/) directly via [FlowLauncher](https://www.flowlauncher.com/).

With FlowGet, you can search, install, uninstall, and update applications effortlessly — all from Flow Launcher’s search bar.

---

## Getting Started

### Prerequisites

To install and use this plugin, you will need the following:

- [FlowLauncher](https://www.flowlauncher.com/)
- [Windows Package Manager (winget)](https://learn.microsoft.com/en-us/windows/package-manager/)

Please make sure you have the latest versions of both installed before proceeding.

---

### Installing

Run the following command in FlowLauncher to install FlowGet:

![Install](assets/installation.png)

---

## Usage

- **Actionword:** `fget`

![Example](assets/usage.gif)

When the actionword is called, it displays available `winget` operations and matching applications.

### Examples

| Action                     | Command                 | Description                      |
|----------------------------|-------------------------|----------------------------------|
| 🔍 Search for a package    | `fget search vscode`    | Searches for Visual Studio Code  |
| 📦 Install a package       | `fget install vscode`   | Installs VS Code via winget      |
| 🔁 Update a package        | `fget update`           | List available apps for update   |
| ❌ Uninstall a package     | `fget uninstall vscode` | Removes VS Code from your system |

*Info:*
- Requires **winget** to be properly configured on your system.
- Some operations may require administrative privileges.
- Internet connection is needed for installing and updating packages.

---

## License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.
