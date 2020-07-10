# XRBase
Basic XR Toolset for Unity

Features
- Two-handed weapons
- Magazines + slide reloading
- Leaning over stuff while not destroying movement

Installation
- Start a new Unity project (I use URP)
- Edit > Project Settings > XR Plugin Management > Install
- Check the boxes for the headsets you want to use (SteamVR instructions TBD)
- Install the XR Interaction Toolkit from the Package Manager (you might need to enable preview packages under "Advanced")
- Install [Quick Outline](https://assetstore.unity.com/packages/tools/particles-effects/quick-outline-115488) from the Unity Asset store
- Clone this repo into the /Assets/ folder, so you end up with /Assets/XRBase/

Scene Setup
- Add XRBaseMain to an object in your scene; this is the thing that sets up and manages the scene (including object pools etc).

Layer Setup
- Create a layer named Player - it interacts with everything but itself and PlayerBodyParts
- Create a layer named PlayerHitBox - it interacts with nothing but RayCast (so the player can be hit)
- Create a layer named Grabbable - it interacts with everything but Player and PlayerHitBox

- You want the Player and its child Body object to be on the Player layer
- The Player Left Hand and Right Hand should be on the default layer (so they can grab stuff)
- But! You might want the Hand on the PlayerBodyParts layer
- All other Player parts should be on PlayerHitBox
- All grabbable objects should be on Grabbable

Big issues
- The way player body positioning is handled is a bit of an experiment - it uses character
controller to move, but a rigidbody body part to 'catch up' to the player's camera position; the
separate rigidbody is there so that if the player is leaning over an obstacle, we don't have to
move the charactercontroller into the obstacle (forcing snapback). It is probably better to do this
by doing spherecasts as we move the character controller around.