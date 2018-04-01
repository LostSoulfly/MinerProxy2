# MinerProxy2

## How to use (Windows)
1. [Download the .NET Core Runtime installer](https://www.microsoft.com/net/download/Windows/run), unless you have the SDK already installed.

2. [Download the newest MinerProxy2 release](https://github.com/LostSoulFly/MinerProxy2/releases) and extract it somewhere.

3. Run the batch file to start MinerProxy2 the first time for it to create its extra directories and default files.

4. Open the Pools folder and open the single JSON file in [Notepad++](https://notepad-plus-plus.org/), and edit it as you see fit, replacing pool/wallet/worker information, then save it.

5. Start MinerProxy2 again using the batch file and it should load up and start waiting for your miners.

You can have as many proxies in a single MinerProxy2 instance as you want, simply add another JSON file with different settings listening on a different port. Each JSON file in the Pools directory is loaded and a proxy is created based on that JSON file's contents.
