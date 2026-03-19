# Input System Extensions

This package aims to provide useful extensions to Unity’s Input System

## Install via Package Manager (git URL)

1. Open the Package Manager
2. Click `+` to *Install package from git URL…*
3. Paste the following:

```
https://github.com/M-Horth21/InputSystemExtensions.git
```

## Button Pair

Unity's Input System has no built-in action binding that fires when two buttons are pressed simultaneously. Button Pair adds a composite and interaction that detects pairs like L1+R1 within a configurable time window, and optionally resets conflicting actions so they don't fire alongside the pair.

### Setting Up a Button Pair

1. Open an `InputActionAsset`, or create a new one
2. Create the `Action` you want to drive with the button pair
3. Click `+` on the `Action` to add a new binding
4. Select *Add Button Pair*

> [!WARNING]
> If you just installed and Button Pair doesn’t appear in the list when creating bindings, you may have to restart the Unity Editor.

5. Click `+` on the Button Pair binding’s *Interactions*
6. Choose *Button Pair*
7. Expand the Button Pair binding and bind each part as you would normally

### How It Works
Button Pair is implemented as two components that work together:

- `ButtonPairComposite` defines the two-button binding. It outputs 1 when both buttons are held, a partial value when only one is held, and 0 when neither is held.
- `ButtonPairInteraction` monitors the composite's output and only performs if the value goes from 0 to 1 within the window.

### Parameters
|Parameter                                      | Default   | Description                                                                                                           |
| -                                             | -         | -                                                                                                                     |
| `Window`                                        | 0.05s     | Time in seconds within which both buttons must be pressed for the action to perform                                   |
| `ResetOtherActionsWithMatchingBindings`    | false     | When enabled, resets any actions in the same Action Map that share a binding with this Button Pair when it performs.  |

### Conflict Resolution
By default, if you have separate actions bound to a control that participates in the Button Pair, those actions will also fire when performing the Button Pair action. To prevent this, enable `ResetOtherActionsWithMatchingBindings` on the Button Pair interaction.

> [!IMPORTANT]
> Actions that share a binding with your Button Pair must use an interaction with a release condition such as Unity's built-in Tap or Hold. Otherwise they fire instantly on button down and can't be reset. Without this, conflict resolution will have no effect.

