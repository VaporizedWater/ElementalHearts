# Welcome to the Boss Checklist Wiki!

This mod adds a checklist that automatically checks off bosses you have defeated as you play. Along with this comes a ton of boss information that both this mod and your favorite mods provide in the form of a log book. If you are a mod developer and want to know more on how to provide all the information about your boss so it can appear on the checklist/log, a full explanation on adding your boss can be found within the [[1.4.4] Boss Log Entry Mod Call](https://github.com/JavidPack/BossChecklist/wiki/%5B1.4.4%5D-Boss-Log-Entry-Mod-Call) guide.

## Debug Tools for Mod Developers

Boss Checklist offers some debug tools for mod developers to assist them. These tools can help you understand how the process works, give access to information you might not have, or even help you create submissions. You can find these tools under the `BossChecklist: Boss Log Customization` configs, and they are accessible in-game or on the mod menu. The section will look like the image below:

![debugtools](https://github.com/JavidPack/BossChecklist/assets/36808587/10993fb4-4e16-4c59-a16a-5d7e376decb6)

* ### Mod.Call Log Verbose
Enable this config if you want more information on the process of how you data is handled and want confirmation that all your data is successfully submitted. All logged messages related to your mod calls should assist in figuring out how the process is handled. When not actively modding, it would be best to disable this config as the client.log may get cluttered with information you will not need.

*Note: Any errors or invalid submissions will be logged regardless of this config's state.*

* ### Disable Auto Localization
BossChecklist offers auto-localization for your submissions Boss Log content. Texts such as entry name, spawn information, and despawn messages. This does not *populate* the values with texts, but rather creates the for you to edit. This config might be preferable if you are currently posting your mod

* ### Access Entry Keys
Each entry added to the Boss Log generates an entry key. Entry keys are what is needed to submit any cross-mod content to the Boss Log. If you need to have access to an entry key, turn on this config. When enabled, each entry page will add a key icon button found underneath the entry name and mod. Hovering over the icon button will display the entry key. You can also click the icon button to copy the entry key it to your clipboard. However, it is a known issue that the clipboard data in-game and on text editor programs somewhat conflict and might not as intended due to limitations.

* ### Access Progression Values
Entries are ordered by their progression. If you need to figure out where to place your entry based on other entries, you can enable this config to display the all progression values. The values appear on each entry page next to the entry name, as well as on the table of contents when hovering over an entry. For vanilla entries you can also reference this [wiki page](https://github.com/JavidPack/BossChecklist/wiki/Boss-Progression-Values).

* ### Show Auto-Detected Collectible Types
When you set up your entry's collectible list, BossChecklist will automatically categorize the items. *more later...*

* ### Enable Reset Data Options
This config will allow you to reset certain data for testing purposes, including boss record resets and the loot checklist. *more later...*

## Common Issues
If you are having trouble with boss checklist, check here to see if a solution to you problem has already been found. If your issue is not listed below, feel free to submit an issue here on GitHub, or ask on [Jopojelly Mods Discord server](http://discord.gg/VzYmqu6) (under `#boss-checklist`). When doing so, please provide a client.log and/or images and videos of your issue. You can find you client.log by going to tModLoader on your Steam library, go to `Properties -> Installed Files -> Browse` then open the 'tModLoader-Logs' folder.

* ### Clicking anywhere on the Boss Log closes it.
Solution: Go to `Settings -> Controls -> Key Bindings` and make sure the 'Toggle Creative Powers' hotkey is not set to Mouse1 (left-click).