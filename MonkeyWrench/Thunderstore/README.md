# MonkeyWrench

Allows you to wield Great Swords, Great Axes, and Sledges with one hand.

A remake of the original MonkeyWrench plugin, created upon request.

## How it works

Unlike the original version, this remake does not modify the items themselves. Instead, it changes how the game handles animations, allowing two-handed weapons to be wielded as one-handed weapons while keeping the original item properties intact.

1. When equipping a two-handed weapon, your character uses the one-handed stance.
2. When attacking, the game uses the corresponding one-handed attack animations.
3. The configured modifiers are applied to attacks while using a modified weapon.
4. When equipping a shield, the plugin checks whether your current weapon can be wielded with one hand. If it can, you can equip the shield alongside it.

## Settings

```yml
## Set key to switch between one-handed and two-handed [Synced with Server]
# Setting type: KeyboardShortcut
# Default value: Y
Switch Key = Y

## List of items that cannot be monkey wrenched [Synced with Server]
# Setting type: String
# Default value:
Blacklist =

## Set attack stamina use multiplier while monkey wrenched [Synced with Server]
# Setting type: Single
# Default value: 0
Stamina Use Increase = 0

## Set damage reduction while monkey wrenched [Synced with Server]
# Setting type: Single
# Default value: 0
Damage Reduction = 0
```

### Stamina Use Increase

This setting increases the stamina cost of attacks made with a modified weapon. The value determines how much the stamina cost is increased.

For example, a value of `0.5` increases stamina cost by 50%, while `1` doubles it. The default value of `0` leaves stamina costs unchanged.

### Damage Reduction

This setting reduces the damage dealt by attacks made with a modified weapon. The value determines how much the damage multiplier is reduced.

For example, a value of `0.2` reduces the damage multiplier by 20%, while `0` leaves damage unchanged.