# 4Fingers

<h1>Overview</h1>

<p>This application is a VR simulation where players take on the role of a planetary rover exploring the surface of Mars to collect geological samples and assess the planet’s potential habitability. Players investigate rocks, perform scans, and analyze environmental clues while navigating hazards such as dust storms that threaten visibility and progress. Throughout the experience, users must solve science-based puzzles using information discovered during exploration. The core objective of the simulation is to educate Secondary School students about Martian geology, environmental conditions, and the scientific process used in planetary exploration.</p>

<h1>Movement & Camera</h1>
| Input            | Action                  |
| ---------------- | ----------------------- |
| Left Thumbstick  | Player movement         |
| Right Thumbstick | Camera / look direction |

<h1>Interaction</h1>

| Input             | Action                                  |
| ----------------- | --------------------------------------- |
| Left Trigger      | Press UI buttons                        |
| Right Trigger     | Press UI buttons                        |
| Right Grip Button | Grab objects / Start drilling mini-game |
| Input                       | Action         |
| --------------------------- | -------------- |
| Y Button (Left Controller)  | Open inventory |
| B Button (Right Controller) | Initiate scan  |
| A Button (Right Controller) | Jump           |

<h1>⚠️ Limitations & Known Bugs</h1>

<p>Due to performance constraints and headset deployment differences, the following issues are present:

Headset Porting Inconsistencies
Some behaviors differ between the Unity editor and the standalone VR headset build.

Flag Graphics Missing in Headset
Flag visuals do not render correctly on the standalone device.

Rock Information Display Issue
Rock information UI appears only for the first scanned rock and does not update afterward.

Reduced View Distance
Environmental view range is intentionally limited to reduce lag and maintain performance on standalone VR hardware (Quest-class devices).
</p>

<h1>Answers</h1>

| Clue                                                                   | Answer              |
| ---------------------------------------------------------------------- | ------------------- |
| Formed where water existed for long periods and slowly evaporated      | **Gypsum (Yellow)** |
| Most abundant volcanic rock on Mars                                    | **Basalt (Blue)**   |
| High levels of Perchlorate (Contains Chlorine)                         | **Regolith (Red)**  |
| Formed when liquid water slowly alters volcanic rock over long periods | **Smectite Clay**   |
| Can store carbon dioxide from the atmosphere in solid form             | **Carbonate**       |

<h1>External Assets</h1>

| Asset                 | Source                                                                                                                                                       |
| --------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Sand Overlay          | [https://www.pinterest.com/pin/1337074889231063/](https://www.pinterest.com/pin/1337074889231063/)                                                           |
| UI Screen             | [https://www.pinterest.com/search/pins/?q=sci%20fi%20UI%20screen&rs=typed](https://www.pinterest.com/search/pins/?q=sci%20fi%20UI%20screen&rs=typed)         |
| Background Wind Sound | [https://pixabay.com/sound-effects/nature-blizzard-wind-463217/](https://pixabay.com/sound-effects/nature-blizzard-wind-463217/)                             |
| Correct Sample SFX    | [https://pixabay.com/sound-effects/film-special-effects-correct-choice-43861/](https://pixabay.com/sound-effects/film-special-effects-correct-choice-43861/) |
| Scan SFX              | [https://pixabay.com/sound-effects/film-special-effects-deepscanmp3-14662/](https://pixabay.com/sound-effects/film-special-effects-deepscanmp3-14662/)       |
| Rock Destruction SFX  | [https://pixabay.com/sound-effects/film-special-effects-rock-destroy-6409/](https://pixabay.com/sound-effects/film-special-effects-rock-destroy-6409/)       |
| Wrong Sample SFX      | [https://pixabay.com/sound-effects/film-special-effects-wronganswer-37702/](https://pixabay.com/sound-effects/film-special-effects-wronganswer-37702/)       |


<h1>Credits</h1>

| Credit To                       | Area Used                                          |
| ------------------------------- | -------------------------------------------------- |
| ChatGPT                         | Drilling MiniGame, StormDOT                        |
| DeepSeek                        | RockInfoDisplay, IntroSequence, AchievementManager |
| Smooth Fade Transition Tutorial | Scene transition implementation                    |
| Body Socketing Tutorial         | XR item socketing system                           |
| Terrain Scanner Tutorial        | Scanning system implementation                     |

<h1>Hardware Requirements</h1>

Standalone VR headset (Quest-class recommended)

Motion controllers required for full functionality
