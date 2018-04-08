# MinerProxy2
MinerProxy 2 is the next version of [MinerProxy](https://github.com/LostSoulFly/MinerProxy) currently in development. It aims to be a multi-coin and multi-algorithm proxy that acts as a server for your miners. The goal is to facilitate easy monitoring and centrilized pool management with multiple miners in a way that helps reduce bandwidth usage by only having one connection to the mining pool and potentially increase mining profitability by sending work to all miners. This version of the proxy is a complete rewrite and can support multiple proxies in a single instance.

Currently only Ethereum is implemented, but I've been working towards a streamlined architecture that will make supporting other cryptocoins relatively easy. Development is ongoing, but I think that the current Ethereum progress is getting stable enough to start testing.

This is a pre-alpha project currently. Many things are not implemented yet. I do not recommend using this proxy in critical deployments until its reliability can be more thoroughly tested and more features are added. I'm opening up the repository for testing and additional insight into its development!

## How to use (Windows)
There are no current releases. To use this project in its current state you must compile it.


1. [Download the .NET Core Runtime installer](https://www.microsoft.com/net/download/Windows/run), unless you have the SDK already installed.

2. [Download the newest MinerProxy2 release](https://github.com/LostSoulFly/MinerProxy2/releases) and extract it somewhere.

3. Run the batch file to start MinerProxy2 the first time for it to create its extra directories and default files.

4. Open the Pools folder and open the single JSON file in [Notepad++](https://notepad-plus-plus.org/), and edit it as you see fit, replacing pool/wallet/worker information, then save it.

5. Start MinerProxy2 again using the batch file and it should load up and start waiting for your miners.

You can have as many proxies in a single MinerProxy2 instance as you want, simply add another JSON file with different settings listening on a different port. Each JSON file in the Pools directory is loaded and a proxy is created based on that JSON file's contents.

Work in progress!
