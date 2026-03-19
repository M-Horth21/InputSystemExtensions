using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.InputSystem;

namespace OutputEnable.InputSystemCustomizations
{
  /// <summary>
  /// An input interaction that performs when both buttons of a <see cref="ButtonPairComposite"/>
  /// are pressed within a configurable time window.
  /// </summary>
  /// <remarks>
  /// Designed exclusively for use with <see cref="ButtonPairComposite"/>. Behavior is undefined
  /// if applied to other types of bindings.
  ///
  /// <para/>The action transitions through three states:
  /// <list type="table">
  /// <item><term>Waiting</term><description> no input</description></item>
  /// <item><term>Started</term><description> one of the buttons pressed, timer begins</description></item>
  /// <item><term>Performed</term><description> second button pressed within the <see cref="Window"/></description></item>
  /// </list>
  /// If the timer expires before both buttons are pressed, the interaction cancels and
  /// returns to Waiting. The interaction will not restart until all buttons are fully released.
  /// </remarks>
  [DisplayName("Button Pair")]
  public class ButtonPairInteraction : IInputInteraction
  {
    /// <summary>
    /// Time in seconds within which both buttons must be pressed for the action to perform.
    /// </summary>
    public float Window = 0.05f;

    /// <summary>
    /// When enabled, performing this interaction will call <see cref="InputAction.Reset"/> on
    /// any other actions in the same <see cref="InputActionMap"/> that share the same binding.
    /// Use this to prevent constituent buttons from firing other actions.
    /// </summary>
    /// <remarks>
    /// Unity offers <see cref="UnityEngine.InputSystem.InputSettings.shortcutKeysConsumeInput"/>
    /// to mitigate the issue of overlapping bindings, however it only works for the final control
    /// that caused an action to perform.
    /// 
    /// For example: If you have 3 separate actions bound to Shift, Z, and Shift+Z.
    /// Pressing Shift will perform the Shift action, then pressing Z will perform the Shift+Z action,
    /// but not the Z action. This is not the behavior we want, since the first action performed.
    /// 
    /// <para/>
    /// This setting will instead look at all other actions in the same action map and check
    /// if they use a control that matches either of the controls that were used to perform this interaction.
    /// If they do, that action will be reset. This does mean that the actions in question must have an
    /// interaction on them to avoid performing instantly,
    /// such as a <see cref="UnityEngine.InputSystem.Interactions.TapInteraction"/>
    /// </remarks>
    public bool ResetOtherActionsWithMatchingBindings = false;

    bool _hasBeenReleased = true;
    readonly HashSet<string> _activeBindingPaths = new(2, StringComparer.OrdinalIgnoreCase);
    readonly HashSet<string> _actionsToReset = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void Process(ref InputInteractionContext context)
    {
      // Checking for not context.ControlIsActuated() is logically sound and may read better here.
      // However it was returning false even when buttons were still held if canceled by timeout.
      // So we explicitly read the value instead.
      if (context.ReadValue<float>() == 0)
      {
        _hasBeenReleased = true;
        if (!context.isWaiting)
          context.Canceled();
        return;
      }

      if (context.timerHasExpired)
      {
        context.Canceled();
        return;
      }

      switch (context.phase)
      {
        case InputActionPhase.Waiting:
          if (_hasBeenReleased && context.ControlIsActuated())
          {
            _hasBeenReleased = false;
            context.Started();
            context.SetTimeout(Window);
          }
          break;
        case InputActionPhase.Started:
          if (context.ControlIsActuated(1))
          {
            context.PerformedAndStayPerformed();

            if (ResetOtherActionsWithMatchingBindings)
              ResetOtherActionsInMap(context);
          }
          break;
        case InputActionPhase.Performed:
          if (!context.ControlIsActuated(1))
            context.Canceled();
          break;
      }
    }

    ///  <inheritdoc/>
    // Don't want to reset _hasBeenReleased here, because Unity will call this method
    // whenever .Canceled() runs, including timing out while 1 button is still pressed.
    public void Reset() { }

    void ResetOtherActionsInMap(InputInteractionContext context)
    {
      PopulateBindingPaths(context);

      var mapBindings = context.action.actionMap.bindings;

      _actionsToReset.Clear();
      for (int i = 0; i < mapBindings.Count; i++)
      {
        if (!_activeBindingPaths.Contains(mapBindings[i].effectivePath)) continue;
        if (mapBindings[i].action == context.action.name) continue;

        // Would love for this to be a reference to the action itself and reset right here,
        // unfortunately it's just a string of the name. Pleeeeeeease Unity??
        _actionsToReset.Add(mapBindings[i].action);
      }

      var actions = context.action.actionMap.actions;
      for (int i = 0; i < actions.Count; i++)
      {
        // The string stored by InputBinding.action could be name or ID. Have chosen to check name
        // only to avoid allocation of converting Guid to string.
        // Expecting that unnamed actions are exceedingly rare.
        if (_actionsToReset.Contains(actions[i].name))
          actions[i].Reset();
      }
    }

    void PopulateBindingPaths(InputInteractionContext ctx)
    {
      var bindings = ctx.action.bindings;

      // Bindings don't expose properties that we can use to traverse their relationships like a graph.
      // Instead, rely on them being stored in order and trust that the composite is 1 or 2 indexes
      // before the triggering control.
      var compositeRootIndex = ctx.action.GetBindingIndexForControl(ctx.control);
      while (!bindings[compositeRootIndex].isComposite) compositeRootIndex--;

      _activeBindingPaths.Clear();
      for (int i = compositeRootIndex + 1; i < bindings.Count; i++)
      {
        if (!bindings[i].isPartOfComposite)
          break;

        _activeBindingPaths.Add(bindings[i].effectivePath);
      }
    }
  }
}