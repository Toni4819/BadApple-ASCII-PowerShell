# BadApple-ASCII-Terminal (No dependency)

## Windows

### PowerShell
```powershell
irm "https://github.com/Toni4819/BadApple-ASCII-Terminal/raw/refs/heads/main/BadApple_ASCII.ps1" | iex
```

### Win + R
```
powershell -c "irm 'https://github.com/Toni4819/BadApple-ASCII-Terminal/raw/refs/heads/main/BadApple_ASCII.ps1' | iex"
```
or (if execution policy blocks it)
```
powershell -ExecutionPolicy Bypass -c "irm 'https://github.com/Toni4819/BadApple-ASCII-Terminal/raw/refs/heads/main/BadApple_ASCII.ps1' | iex"
```

---

## Linux (all distros)

### Install PowerShell first

**Debian / Ubuntu / Mint**
```bash
sudo apt update && sudo apt install -y powershell
```

**Any distro (snap)**
```bash
sudo snap install powershell --classic
```

### Run
```bash
pwsh -c "irm 'https://github.com/Toni4819/BadApple-ASCII-Terminal/raw/refs/heads/main/BadApple_ASCII.ps1' | iex"
```
